# Communications for the Compumate (Laser pc3) computer (1989)

The Laser pc3 from vtech (also called the Compumate) was a late-1980s notebook-style computer than ran on four AA batteries. It includes a small set of simple programs (calculator, spell checker, telephone book, and some more).

The original compumate included communications software. If you don't have that program available, this one may help. If can read in the 
The compumate includes an RS232 port (wrapped in a non-standard 15-pin connector). This app, [available on the Microsoft store](	https://www.microsoft.com/store/apps/9NLKC14PWBRQ), lets you download your personal data from the compumate and can parse most of it.

# Notes about the ".compumate" file format

When I download the data, it's just raw bytes. When I save to disk, I use a "readable-binary" format: most printable ASCII chars are printed straight, and everything is printed as \XX hex chars. Backslashes are printed in hex. CR and LF are hex (they are not printable ASCII), but I also add in a \n after them, and I limit the length of each line.

The end resut is very usable for poking at the files but can also be emailed and edited with normal editors.

# Notes about parsing the raw data stream

In the old days, we'd struggle to make a byte-by-byte reader. But these are modern days: the files are realistically only going to be less than 32K, and I may as well work on on the bytes in one big chunk.

Because I don't understand the file format fully, I instead look for the segments that are understood. Each section has a 16-byte header with an uppercase name followed by NUL chars as padding. After that is 7 bytes of "stuff", one of which is the number of entries. Each entry is a set of 5 NUL terminated strings (except the word processor, where is entry is a set of 2 NUL terminated string.

Still to do
1. Understand more of the format
2. Be able to upload data instead of just download
3. Parse the expense reports
4. Parse the BASIC programs
