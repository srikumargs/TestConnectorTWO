using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Globalization;

namespace Unzip
{
    class Program
    {
        public static void Main(string[] args)
        {
            Environment.ExitCode = 0;

            if (args.Count() != 2)
            {
                Console.WriteLine("Usage: Unzip [zip file] [destination directory]");
                Environment.ExitCode = 1;
            }
            else
            {
                String zipFileName = args[0];
                String destinationDir = args[1];

                if (!File.Exists(zipFileName))
                {
                    Console.WriteLine("Zip file does not exist: {0}", zipFileName);
                    Environment.ExitCode = 1;
                }
                else
                {
                    EnsureDirectoryExists(Directory.GetParent(destinationDir).FullName, "DestinationDir");

                    ZipFile zipFile = new ZipFile(File.OpenRead(zipFileName));
                    byte[] buffer = new byte[4096];
                    String fileName = String.Empty;

                    try
                    {
                        for (Int32 i = 0; i < zipFile.Count; i++)
                        {
                            ZipEntry zipEntry = zipFile[i];

                            fileName = Path.Combine(destinationDir, zipEntry.Name);
                            fileName = fileName.Replace('/', '\\');

                            try
                            {

                                FileDelete(fileName);

                                EnsureDirectoryExists(Path.GetDirectoryName(fileName));

                                Int32 size = 0;
                                using (var zipStream = zipFile.GetInputStream(zipEntry))
                                {
                                    FileStream fileStream = File.Create(fileName);

                                    do
                                    {
                                        size = zipStream.Read(buffer, 0, buffer.Length);

                                        if (size > 0)
                                        {
                                            fileStream.Write(buffer, 0, size);
                                        }
                                    } while (size > 0);
                                    fileStream.Close();
                                }
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                Console.WriteLine(String.Format("Unzip : error : Action UNZ0000 : Exception encountered during replacement of '{0}'\r\n[\r\n{1}\r\n]", fileName, ex.ToString()));

                                Environment.ExitCode = 1;
                            }
                        }

                        Environment.ExitCode = 0;
                    }
                    finally
                    {
                        zipFile.Close();
                    }
                }
            }
        }

        private static void FileDelete(String fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            if (fileInfo.Exists)
            {
                if (fileInfo.Attributes != FileAttributes.Normal)
                {
                    System.IO.File.SetAttributes(fileInfo.FullName, FileAttributes.Normal);
                }

                System.IO.File.Delete(fileName);
            }
        }

        private static void EnsureDirectoryExists(String directoryPath)
        {
            EnsureDirectoryExists(directoryPath, String.Empty);
        }

        private static void EnsureDirectoryExists(String directoryPath, String directoryDescription)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory does not exist;  creating.");
                Console.WriteLine(String.Format(CultureInfo.CurrentCulture, "{0}:   '{1}'", String.IsNullOrEmpty(directoryDescription) ? "Directory" : directoryDescription, directoryPath));
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}
