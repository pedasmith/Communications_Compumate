using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Fill out the uiSerialPort ComboBox
            uiSerialPort.Items.Clear();
            var selector = SerialDevice.GetDeviceSelector();
            Log($"Selector is {selector}");

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
                        Log($"   Adding port {item.Name} id={item.Id}");
                        uiSerialPort.Items.Add(cbi);
                    }
                    catch (Exception)
                    {
                        Log($"    NOT Serial: name={item.Name} id={item.Id}");
                    }
                }
                else
                {
                    //Log($"    NOT Potential: name={item.Name} id={item.Id}");
                }
            }

            //uiSerialPort.SelectionChanged -= OnSelectSerialPort;
            if (uiSerialPort.Items.Count >= 1)
            {
                uiSerialPort.SelectedIndex = 0;
                OnSelectSerialPort(null, null); // Only one? Auto open!
            }
            //uiSerialPort.SelectionChanged += OnSelectSerialPort;
        }

        CancellationTokenSource Cts = new CancellationTokenSource();
        CancellationToken CT;
        Task ReadTask = null;
        private void OnSelectSerialPort(object sender, RoutedEventArgs e)
        {
            var cbi = uiSerialPort.SelectedValue as ComboBoxItem;
            var sd = cbi?.Tag as SerialDevice;
            if (sd == null) return;
            ReadTask = ContinuouslyReadSerial(sd, Cts.Token);
        }

        DataChunk CurrData = new DataChunk();
        private async Task ContinuouslyReadSerial(SerialDevice sd, CancellationToken ct)
        {
            Log($"OPENING PORT!");
            sd.BaudRate = 9600;
            sd.Handshake = SerialHandshake.None;
            sd.Parity = SerialParity.Even;
            sd.StopBits = SerialStopBitCount.One;
            sd.IsRequestToSendEnabled = true;
            sd.IsDataTerminalReadyEnabled = true;
            sd.ReadTimeout = new TimeSpan(0, 0, 5);

            var dr = new DataReader(sd.InputStream);
            dr.InputStreamOptions = InputStreamOptions.Partial;
            bool lastWasCtrl = false;
            while (true)
            {
                var n = await dr.LoadAsync(100);

                Log($"GOT DATA length={n}");
                var left = dr.UnconsumedBufferLength;
                var buffer = new byte[left];
                dr.ReadBytes(buffer);
                CurrData.AddBytes(buffer);
                AddBufferToScreen(buffer, ref lastWasCtrl);
            }
        }

        private void AddBufferToScreen(byte[] buffer, ref bool lastWasCtrl)
        {
            var sb = new StringBuilder();
            foreach (var b in buffer)
            {
                if (b < 0x20 || b >= 0x7f)
                {
                    if (!lastWasCtrl) sb.Append(' ');
                    sb.Append($"x{b:X2} ");
                    lastWasCtrl = true;
                }
                else
                {
                    sb.Append((char)b);
                    lastWasCtrl = false;
                }
            }
            uiOutput.Text += sb.ToString();
        }

        private void OnSceenClear(object sender, RoutedEventArgs e)
        {
            uiOutput.Text = "";
            uiLog.Text = "";
        }

        static string FileLocation_Compumate_Data = "Compumate_Data_Location";

        private async void OnDataSave(object sender, RoutedEventArgs e)
        {
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
                await FileIO.WriteBytesAsync(file, CurrData.RawBytes);
                Log($"Write {CurrData.RawBytes.Length}  bytes to file {file.DisplayName}");
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
                var buffer = await FileIO.ReadBufferAsync(file);
                var bytes = buffer.ToArray();
                CurrData.ClearBytes();
                CurrData.AddBytes(bytes);
                OnSceenClear(null, null);
                Log($"Read {CurrData.RawBytes.Length}  bytes to file {file.DisplayName}");


                //bool lastWasCtrl = false;
                //AddBufferToScreen(bytes, ref lastWasCtrl);

                var privateFiles = new CompumatePrivateFiles(CurrData);
                uiOutput.Text += privateFiles.ToString();

                var telephoneList = new CompumateTelephoneDirectory(CurrData);
                uiOutput.Text += telephoneList.ToString();

                var appointments = new CompumateAppointments(CurrData);
                uiOutput.Text += appointments.ToString();
            }
        }
    }
}
