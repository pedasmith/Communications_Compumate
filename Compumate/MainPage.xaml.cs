using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Compumate
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }
        private void Log(string str)
        {
            uiLog.Text += str + "\n";
        }
        private void QuickStatus(string str)
        {
            uiQuickStatus.Text = str;
        }
        private void Error(string str)
        {
            uiQuickStatus.Text = str;
            Log(str);
        }
        public async Task DoFilesActivated(FileActivatedEventArgs args)
        {
            if (args.Files.Count == 1)
            {
                // Open the one file.
                var file = args.Files[0] as StorageFile;
                await DoFileRead(file);
            }
        }

        private async Task FillSerialComboBox()
        {

            uiSerialPort.Items.Clear();
            var selector = SerialDevice.GetDeviceSelector();

            // Need to filter out the machine name from the list. Why? Because this is a silly API.
            var ecdi = new EasClientDeviceInformation();
            var machineName = ecdi.FriendlyName;
            Log($"ecdi.FriendlyName={ecdi.FriendlyName}");

            var list = await DeviceInformation.FindAllAsync(selector);
            foreach (var item in list)
            {
                var isbt = item.Id.StartsWith(@"\\?\BTHENUM");
                var ismachine = item.Name.Contains(ecdi.FriendlyName);
                var isPotential = !isbt && !ismachine;
                if (isPotential)
                {
                    try
                    {
                        SerialDevice sd = await SerialDevice.FromIdAsync(item.Id);
                        var cbi = new ComboBoxItem()
                        {
                            Content = item.Name,
                            Tag = sd,
                        };
                        //Log($"   Adding port {item.Name} id={item.Id}");
                        uiSerialPort.Items.Add(cbi);
                    }
                    catch (Exception)
                    {
                        //Log($"    NOT Serial: name={item.Name} id={item.Id}");
                    }
                }
                else
                {
                    //Log($"    NOT Potential: name={item.Name} id={item.Id}");
                }
            }
            switch (uiSerialPort.Items.Count)
            {
                case 0: QuickStatus("No serial ports found"); break;
                case 1: QuickStatus("One serial port found; automatically open"); break;
                default: QuickStatus("Select serial port to open it and read"); break;
            }
            if (uiSerialPort.Items.Count >= 1)
            {
                uiSerialPort.SelectedIndex = 0;
            }
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await FillSerialComboBox();
            if (uiSerialPort.Items.Count == 1)
            {
                OnSelectSerialPort(null, null); // Only one? Auto open!
            }

            // Fill in the help system.

            // Set up common markdown values
            uiHelpMarkdown.ImageStretch = Stretch.Uniform;
            uiHelpMarkdown.ImageMaxWidth = 400;
            uiHelpMarkdown.UriPrefix = "ms-appx:///Assets/HelpFiles/";
            uiHelpMarkdown.ImageResolving += UiHelpMarkdown_ImageResolving;

            // All the built-in tests
            int nerror = 0;
            nerror += ReadableBinary.Test();
        }

        private async Task ReadHelpAsync(string filename)
        {
            StorageFolder installationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var helpFolder = await installationFolder.GetFolderAsync(@"Assets\HelpFiles");
            var md = await helpFolder.GetFileAsync(filename);

            var text = await FileIO.ReadTextAsync(md);
            uiHelpMarkdown.Text = text;

            uiHelpArea.Visibility = Visibility.Visible;
        }

        private void UiHelpMarkdown_ImageResolving(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageResolvingEventArgs e)
        {
            ;
        }

        CancellationTokenSource Cts = new CancellationTokenSource();
        Task ReadTask = null;
        private void OnSelectSerialPort(object sender, RoutedEventArgs e)
        {
            var cbi = uiSerialPort.SelectedValue as ComboBoxItem;
            var sd = cbi?.Tag as SerialDevice;
            if (sd == null)
            {
                Error("Unable to open serial port");
                return;
            }
            ReadTask = ContinuouslyReadSerial(sd, Cts.Token);
        }

        DataChunk CurrData = new DataChunk();
        private async Task ContinuouslyReadSerial(SerialDevice sd, CancellationToken ct)
        {
            Log($"OPENING PORT!");
            sd.BaudRate = 9600;
            sd.DataBits = 8;
            sd.Handshake = SerialHandshake.None;
            sd.Parity = SerialParity.Even;
            sd.StopBits = SerialStopBitCount.One;
            sd.IsRequestToSendEnabled = true;
            sd.IsDataTerminalReadyEnabled = true;
            sd.ReadTimeout = new TimeSpan(0, 0, 1);

            var dr = new DataReader(sd.InputStream);
            dr.InputStreamOptions = InputStreamOptions.Partial;
            bool lastWasCtrl = false;

            while (true)
            {
                var n = await dr.LoadAsync(500);

                Log($"GOT DATA length={n}");
                var left = dr.UnconsumedBufferLength;
                var buffer = new byte[left];
                dr.ReadBytes(buffer);
                CurrData.AddBytes(buffer);
                AddBufferToScreen(buffer, ref lastWasCtrl);

                if (n != 500)
                {
                    // must be at the end?
                    ParseCurrData();
                }
            }
        }

        private void AddBufferToScreen(byte[] buffer, ref bool lastWasCtrl)
        {
            var str = ReadableBinary.ToString(buffer);
            uiLog.Text += str;
        }

        private void OnSceenClear(object sender, RoutedEventArgs e)
        {
            uiOutput.Text = "";
            uiLog.Text = "";
            CurrData.ClearBytes();
        }

        static string FileLocation_Compumate_Data = "Compumate_Data_Location";

        private async void OnDataSave(object sender, RoutedEventArgs e)
        {
            if (CurrData.RawBytes == null || CurrData.RawBytes.Length == 0)
            {
                Error("Can't save until data's been read");
                return;
            }

            var savePicker = new FileSavePicker()
            {
                SettingsIdentifier = FileLocation_Compumate_Data,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "Compumate.compumate",
            };
            savePicker.FileTypeChoices.Add("Binary", new List<string>() { ".compumate" });
            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                try
                {
                    var str = ReadableBinary.ToString(CurrData.RawBytes);
                    await FileIO.WriteTextAsync (file, str);
                    QuickStatus($"Wrote {CurrData.RawBytes.Length} bytes");
                }
                catch (Exception ex)
                {
                    Error($"Unable to write to {file.DisplayName}. {ex.Message}");
                }
            }
        }

        private async void OnDataRead(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker()
            {
                SettingsIdentifier = FileLocation_Compumate_Data,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                ViewMode = PickerViewMode.List,
            };
            openPicker.FileTypeFilter.Add(".compumate");
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                await DoFileRead(file);
            }
        }

        private async Task DoFileRead(StorageFile file)
        {
            try
            {
                var str = await FileIO.ReadTextAsync(file);
                var bytes = ReadableBinary.ToBinary(str);
                OnSceenClear(null, null);
                CurrData.ClearBytes();
                CurrData.AddBytes(bytes);
                Log($"Read {CurrData.RawBytes.Length}  bytes from file {file.DisplayName}");

                ParseCurrData();
                QuickStatus($"Read {CurrData.RawBytes.Length} bytes");
            }
            catch (Exception ex)
            {
                Error($"Unable to read {file.DisplayName}. {ex.Message}");
            }
        }

        private void ParseCurrData()
        {
            var privateFiles = new CompumatePrivateFiles(CurrData);
            uiOutput.Text += privateFiles.ToString();

            var telephoneList = new CompumateTelephoneDirectory(CurrData);
            uiOutput.Text += telephoneList.ToString();

            var appointments = new CompumateAppointments(CurrData);
            uiOutput.Text += appointments.ToString();

            var wordProcFiles = new CompumateWordProcessor(CurrData);
            uiOutput.Text += wordProcFiles.ToString();
        }

        private async void OnHelp(object sender, RoutedEventArgs e)
        {
            var file = (sender as FrameworkElement).Tag as string;
            await ReadHelpAsync(file);
        }

        private void OnGridTapped(object sender, TappedRoutedEventArgs e)
        {
            uiHelpArea.Visibility = Visibility.Collapsed;
        }
    }
}
