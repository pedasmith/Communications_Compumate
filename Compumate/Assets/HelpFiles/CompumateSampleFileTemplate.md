# Compumate Sample .compumate file

## Readable-binary file
A compumate file is a "readable-binary" file: every simple printable character is (like "A") is simply in the file. Every control character is represented with a \XX hex number. Carriage-return and line-feed are control characters and are represented by their corresponding hex followed by a new-line. And every line is shortened with new-lines.

In addition, a # is also replaced by the hex equal, as is a regular back-slash. Every back-slashes in the file is followed by a two-letter HEX value.

When a readable-binary file is read, each character is read; if it is a backslash, the next two chars are read and converted to hex. If it is a printable ASCII char (space through tilde), it is just that char. And anything else is discarded.

## Decoding a .compumate file

Each file has a set of sections that start with a 16-byte header, 7 bytes of "goo", and then a series of NUL-terminated strings. The series is either grouped by fives (most items) or by twos (the word processing docs). An exception is the Expense Account, which is more complicated and has not been decoded. The number of strings is part of the "goo" after the section name.

The sections are surrounded by more "goo" which isn't understood. To parse the file, each type of section is searched for though the entire file-blob. For example, to find all of the "private file" entries, look for the string "PRIVATE FILE" followed by 4 NUL bytes. This is "efficient enough" considering that the entire file can only be about 30K long.

## Sample .compumate file
```
{content}
```