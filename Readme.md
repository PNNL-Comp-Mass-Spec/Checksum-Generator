The Checksum Generator program computes MD5 and SHA-1 checksums
for files matching the file mask.  Results are written to a
tab-delimited text file

Program syntax #1:
Checksum_Generator.exe FileMask [/S] [/O:OutputFile] [/F] [/Preview]

Program syntax #2:
Checksum_Generator.exe /I:FileMask [/S] [/O:OutputFile] [/F] [/Preview]

FileMask specifies the files to compute the checksums, for example, *.raw
FileMask can optionally include a folder path, e.g. C:\temp\*.raw

Use /S to process files in all subfolders
Use /O to specify the output file path for the checksum file
Use /F to write full file paths to the output file
Use /Preview to view files that would be processed

-------------------------------------------------------------------------------
Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
Copyright 2013, Battelle Memorial Institute.  All Rights Reserved.

E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
Website: http://panomics.pnnl.gov/ or http://omics.pnl.gov
-------------------------------------------------------------------------------

Licensed under the Apache License, Version 2.0; you may not use this file except 
in compliance with the License.  You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0
