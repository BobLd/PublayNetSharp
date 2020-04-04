namespace PublayNetSharp
{
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// 
    /// </summary>
    public static class TarGz
    {
        const string tempFolder = "temp";
        const string logFile = "log.txt";

        /// <summary>
        /// Extract all compressed pdf documents.
        /// </summary>
        /// <param name="tarGzFolderPdf">The folder containing the tar.gz compressed pdf documents.</param>
        /// <param name="outputFolder">The output folder containing the extracted pdf documents.</param>
        public static void ExtractAll(string tarGzFolderPdf, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            string logFilePath = Path.Combine(outputFolder, logFile);

            DirectoryInfo d = new DirectoryInfo(tarGzFolderPdf);
            var zippedFiles = d.GetFiles("*.tar.gz", SearchOption.AllDirectories);

            zippedFiles = zippedFiles.OrderBy(f => f.Length).ToArray(); // order by size

            var todo = zippedFiles.Length;
            var done = 0;

            foreach (var tarGzFile in zippedFiles)
            {
                if (!Extract(tarGzFile, outputFolder))
                {
                    Console.WriteLine("Error with file '" + tarGzFile.Name + "'.");
                    File.AppendAllText(logFilePath, tarGzFile.FullName + Environment.NewLine);
                }
                else
                {
                    done++;
                    if (done % 100 == 0) Console.WriteLine(done + "/" + todo);
                }
            }
        }

        /// <summary>
        /// Extract the pdf document.
        /// </summary>
        /// <param name="tarGzFile">The path to the tar.gz compressed pdf document.</param>
        /// <param name="outputFolder">The output folder containing the extracted pdf document.</param>
        /// <returns>True if extracted.</returns>
        public static bool Extract(FileInfo tarGzFile, string outputFolder)
        {
            var tempPath = Path.Combine(outputFolder, tempFolder);
            Directory.CreateDirectory(tempPath);
            while (!Directory.Exists(tempPath))
            {
                // wait for creation
                Thread.Sleep(50);
            }

            var rootFileName = tarGzFile.Name.Replace(".tar.gz", "");

            string tempFolderPath = Path.Combine(tempPath, rootFileName);

            try
            {
                using (Stream sourceStream = new GZipInputStream(tarGzFile.OpenRead()))
                using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(sourceStream, TarBuffer.DefaultBlockFactor))
                {
                    tarArchive.ExtractContents(tempPath);
                }

                // check if something was extracted
                if (!Directory.Exists(tempFolderPath)) throw new DirectoryNotFoundException();
            }
            catch (Exception)
            {
                // delete all file in temp
                Directory.Delete(tempPath, true);
                return false;
            }

            // find nxml file to get the pdf file name as several pdf documents can exist in the tar.gz file
            var nxmlFiles = Directory.GetFiles(tempFolderPath, "*.nxml", SearchOption.TopDirectoryOnly);
            if (nxmlFiles.Length != 1)
            {
                // delete all file in temp
                Directory.Delete(tempPath, true);
                return false;
            }

            var pdfFilePath = Path.ChangeExtension(nxmlFiles[0], "pdf");
            if (!File.Exists(pdfFilePath))
            {
                // delete all file in temp
                Directory.Delete(tempPath, true);
                return false;
            }

            try
            {
                // move and rename pdf file
                File.Move(pdfFilePath, Path.Combine(outputFolder, rootFileName + ".pdf"));
            }
            catch (Exception)
            {           
                // delete all file in temp
                Directory.Delete(tempPath, true);
                return false;
            }

            // delete all file in temp
            Directory.Delete(tempPath, true);

            while (Directory.Exists(tempPath))
            {
                // wait for deletion
                Thread.Sleep(50);
            }

            return true;
        }
    }
}
