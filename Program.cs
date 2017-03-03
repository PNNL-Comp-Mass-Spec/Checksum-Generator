using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileProcessor;

namespace Checksum_Generator
{
    class Program
    {
        private const string PROGRAM_DATE = "December 9, 2013";

        static double mPercentComplete;

        private static string mFileMask;
        private static bool mRecurse;
        
        private static string mOutputFilePath;
        private static bool mFullPathsInResults;

        private static bool mPreviewMode;

        static int Main(string[] args)
        {
            var objParseCommandLine = new FileProcessor.clsParseCommandLine();

            mFileMask = string.Empty;
            mRecurse = false;
            
            mOutputFilePath = string.Empty;
            mFullPathsInResults = false;

            mPreviewMode = false;

            try
            {
                var success = false;

                if (objParseCommandLine.ParseCommandLine())
                {
                    if (SetOptionsUsingCommandLineParameters(objParseCommandLine))
                        success = true;
                }

                if (!success ||
                    objParseCommandLine.NeedToShowHelp ||
                    objParseCommandLine.ParameterCount + objParseCommandLine.NonSwitchParameterCount == 0)
                {
                    ShowProgramHelp();
                    return -1;

                }

                success = ComputeChecksums(mFileMask, mRecurse, mOutputFilePath, mFullPathsInResults, mPreviewMode);
                if (!success)
                {
                    Thread.Sleep(1000);
                    return -2;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred in Program->Main: " + Environment.NewLine + ex.Message);
                Console.WriteLine(ex.StackTrace);
                return -1;
            }

            return 0;
        }

        private static bool ComputeChecksums(string fileMask, bool recurse, string outputFilePath, bool fullPathsInResults, bool previewMode)
        {
            var currentFile = "No file (processing has not yet started)";

            try
            {
                var folderPath = ".";
                var slashIndex = fileMask.LastIndexOf('\\');
                if (slashIndex > -1)
                {
                    // Extract the directory info from fileMask
                    folderPath = fileMask.Substring(0, slashIndex);
                    if (slashIndex >= fileMask.Length)
                    {
                        Console.WriteLine("Note: FileMask ended in a slash; will process all files in " + folderPath);
                        fileMask = "*.*";
                    }
                    else
                    {
                        fileMask = fileMask.Substring(slashIndex + 1);
                    }
                }

                if (!(fileMask.Contains("*") || fileMask.Contains("?")))
                {
                    var testPath = Path.Combine(folderPath, fileMask);
                    if (Directory.Exists(testPath))
                    {
                        folderPath = testPath;
                        fileMask = "*.*";
                    }
                }
                
                if (string.IsNullOrWhiteSpace(outputFilePath))
                    outputFilePath = "CheckSumFile_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                var msg = "Looking for files matching " + fileMask;
                if (folderPath == ".")
                    msg += " in the current directory";
                else
                    msg += " in folder " + folderPath;

                if (recurse)
                    msg += " and its subdirectories";

                Console.WriteLine(msg);

                var diFolder = new DirectoryInfo(folderPath);
                var searchOption = SearchOption.TopDirectoryOnly;
                if (recurse)
                    searchOption = SearchOption.AllDirectories;

                var fiFiles = diFolder.GetFiles(fileMask, searchOption);

                if (fiFiles.Length == 0)
                {
                    msg = "Did not find any files matching " + fileMask + " in " + diFolder.FullName;
                    if (recurse)
                        msg += " or its subfolders";
                    ShowErrorMessage(msg);
                    return false;
                }

                if (previewMode)
                {
                    foreach (var fiFile in fiFiles)
                    {
                        Console.WriteLine(fiFile.FullName);
                    }
                    return true;
                }

                Console.WriteLine("Writing checksums to " + outputFilePath);
                var fiOutputFile = new FileInfo(outputFilePath);

                // Create the output file
                using (var swOutputFile = new StreamWriter(new FileStream(fiOutputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read)))
                {
                    swOutputFile.AutoFlush = true;
                    swOutputFile.WriteLine("MD5\tSHA1\tBytes\tFilename");

                    var checkSumGenerator = new clsChecksum
                    {
                        ThrowEvents = false
                    };

                    var filesProcessed = 0;

                    foreach (var fiFile in fiFiles)
                    {
                        if (string.Equals(fiOutputFile.FullName, fiFile.FullName, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        currentFile = fiFile.FullName;
                        var md5 = ComputeMD5(checkSumGenerator, fiFile);
                        var sha1 = ComputeSha1(checkSumGenerator, fiFile);

                        if (fullPathsInResults)
                            swOutputFile.WriteLine(md5 + "\t" + sha1 + "\t" + fiFile.Length + "\t" + fiFile.FullName);
                        else
                            swOutputFile.WriteLine(md5 + "\t" + sha1 + "\t" + fiFile.Length + "\t" + fiFile.Name);

                        filesProcessed++;
                        mPercentComplete = filesProcessed / (float)fiFiles.Length * 100;
                        Console.WriteLine(mPercentComplete.ToString("0.0") + "%: " + fiFile.Name);
                    }

                }

                currentFile = "No file (processing complete)";
                Thread.Sleep(250);

                Console.WriteLine();
                Console.WriteLine("Results:");

                // Re-open the file and show the first 5 lines
                using (var srChecksumFile = new StreamReader(new FileStream(outputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    // Set this to negative one to account for the header line
                    var filesProcessed = -1;
                    var FILES_TO_SHOW = 4;

                    while (!srChecksumFile.EndOfStream)
                    {
                        var dataLine = srChecksumFile.ReadLine();
                        if (string.IsNullOrWhiteSpace(dataLine))
                            continue;

                        filesProcessed++;
                        if (filesProcessed <= FILES_TO_SHOW)
                        {
                            Console.WriteLine(dataLine);
                        }
                    }

                    var additionalFiles = filesProcessed - FILES_TO_SHOW;
                    if (additionalFiles > 0)
                    {
                        if (additionalFiles == 1)
                            Console.WriteLine("... plus 1 more file");
                        else
                            Console.WriteLine("... plus " + additionalFiles + " others");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error computing checksums, file " + currentFile + ": " + ex.Message + Environment.NewLine + ex.StackTrace);                
                return false;
            }

        }

        private static string ComputeSha1(clsChecksum checkSumGenerator, FileInfo fiFile)
        {
            return checkSumGenerator.GenerateSha1Hash(fiFile.FullName);
        }

        private static string ComputeMD5(clsChecksum checkSumGenerator, FileInfo fiFile)
        {
            return checkSumGenerator.GenerateMD5Hash(fiFile.FullName);
        }

        private static string GetAppVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (" + PROGRAM_DATE + ")";
        }

        private static bool SetOptionsUsingCommandLineParameters(FileProcessor.clsParseCommandLine objParseCommandLine)
        {
            // Returns True if no problems; otherwise, returns false
            var lstValidParameters = new List<string> { "I", "O", "F", "S", "Preview" };

            try
            {
                // Make sure no invalid parameters are present
                if (objParseCommandLine.InvalidParametersPresent(lstValidParameters))
                {
                    var badArguments = new List<string>();
                    foreach (var item in objParseCommandLine.InvalidParameters(lstValidParameters))
                    {
                        badArguments.Add("/" + item);
                    }

                    ShowErrorMessage("Invalid commmand line parameters", badArguments);

                    return false;
                }

                // Query objParseCommandLine to see if various parameters are present						

                if (objParseCommandLine.NonSwitchParameterCount > 0)
                    mFileMask = objParseCommandLine.RetrieveNonSwitchParameter(0);

                if (!ParseParameter(objParseCommandLine, "I", "a filemask specification", ref mFileMask)) return false;

                if (!ParseParameter(objParseCommandLine, "O", "an output file path", ref mOutputFilePath)) return false;

                if (objParseCommandLine.IsParameterPresent("F"))
                    mFullPathsInResults = true;

                if (objParseCommandLine.IsParameterPresent("Preview"))
                {
                    mPreviewMode = true;
                }

                if (objParseCommandLine.IsParameterPresent("S"))
                {
                    mRecurse = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error parsing the command line parameters: " + Environment.NewLine + ex.Message);
            }

            return false;
        }

        private static bool ParseParameter(clsParseCommandLine objParseCommandLine, string parameterName, string description, ref string targetVariable)
        {
            string value;
            if (objParseCommandLine.RetrieveValueForParameter(parameterName, out value))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    ShowErrorMessage("/" + parameterName + " does not have " + description);
                    return false;
                }
                targetVariable = string.Copy(value);
            }
            return true;
        }

        private static void ShowErrorMessage(string strMessage)
        {
            const string strSeparator = "------------------------------------------------------------------------------";

            Console.WriteLine();
            Console.WriteLine(strSeparator);
            Console.WriteLine(strMessage);
            Console.WriteLine(strSeparator);
            Console.WriteLine();

            WriteToErrorStream(strMessage);
        }

        private static void ShowErrorMessage(string strTitle, IEnumerable<string> items)
        {
            const string strSeparator = "------------------------------------------------------------------------------";

            Console.WriteLine();
            Console.WriteLine(strSeparator);
            Console.WriteLine(strTitle);
            var strMessage = strTitle + ":";

            foreach (var item in items)
            {
                Console.WriteLine("   " + item);
                strMessage += " " + item;
            }
            Console.WriteLine(strSeparator);
            Console.WriteLine();

            WriteToErrorStream(strMessage);
        }


        private static void ShowProgramHelp()
        {
            var exeName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            try
            {
                Console.WriteLine();
                Console.WriteLine("This program computes MD5 and SHA-1 checksums for the specified files");
                Console.WriteLine();

                Console.Write("Program syntax #1:" + Environment.NewLine + exeName);
                Console.WriteLine(" FileMask [/S] [/O:OutputFile] [/F] [/Preview]");

                Console.WriteLine();
                Console.Write("Program syntax #2:" + Environment.NewLine + exeName);
                Console.WriteLine(" /I:FileMask [/S] [/O:OutputFile] [/F] [/Preview]");


                Console.WriteLine();
                Console.WriteLine("FileMask specifies the files to compute the checksums, for example, *.raw");
                Console.WriteLine(@"FileMask can optionally include a folder path, e.g. C:\temp\*.raw");
                Console.WriteLine();
                Console.WriteLine("Use /S to process files in all subfolders");
                Console.WriteLine("Use /O to specify the output file path for the checksum file");
                Console.WriteLine("Use /F to write full file paths to the output file");
                Console.WriteLine("Use /Preview to view files that would be processed");
                Console.WriteLine();
                Console.WriteLine("Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2013");
                Console.WriteLine("Version: " + GetAppVersion());
                Console.WriteLine();

                Console.WriteLine("E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com");
                Console.WriteLine("Website: http://panomics.pnnl.gov/ or http://omics.pnl.gov or http://www.sysbio.org/resources/staff/");
                Console.WriteLine();

                // Delay for 750 msec in case the user double clicked this file from within Windows Explorer (or started the program via a shortcut)
                Thread.Sleep(750);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error displaying the program syntax: " + ex.Message);
            }

        }

        private static void WriteToErrorStream(string strErrorMessage)
        {
            try
            {
                using (var swErrorStream = new System.IO.StreamWriter(Console.OpenStandardError()))
                {
                    swErrorStream.WriteLine(strErrorMessage);
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // Ignore errors here
            }
        }

    }
}
