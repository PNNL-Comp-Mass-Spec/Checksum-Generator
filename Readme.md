# Checksum Generator

The Checksum Generator program computes MD5 and SHA-1 checksums for files matching the file mask.
Results are written to a tab-delimited text file.

## Downloads

Download a .zip file with the program from:
* https://github.com/PNNL-Comp-Mass-Spec/Checksum-Generator/releases

## Console Switches

The Checksum Generator is a console application, and must be run from the Windows command prompt.

Program syntax #1:
```
Checksum_Generator.exe FileMask [/S] [/O:OutputFile] [/F] [/Preview]
```

Program syntax #2:
```
Checksum_Generator.exe /I:FileMask [/S] [/O:OutputFile] [/F] [/Preview]
```

FileMask specifies the files to compute the checksums, for example, *.raw
* FileMask can optionally include a directory path, e.g. C:\temp\*.raw

Use /S to process files in all subdirectories

Use /O to specify the output file path for the checksum file

Use /F to write full file paths to the output file

Use /Preview to view files that would be processed

## Contacts

Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) \
E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov\
Website: https://panomics.pnnl.gov/ or https://omics.pnl.gov

## License

Checksum Generator is licensed under the Apache License, Version 2.0; you may not use this
file except in compliance with the License.  You may obtain a copy of the
License at https://opensource.org/licenses/Apache-2.0
