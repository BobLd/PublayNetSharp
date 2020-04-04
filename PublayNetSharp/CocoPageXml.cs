using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PublayNetSharp
{
    public static class CocoPageXml
    {
        const string logFile = "log_convert.txt";
        const string logDeleteFile = "log_delete.txt";

        /// <summary>
        /// Convert the coco formated json file to PageXml format.
        /// <para>Will check for the existence of the pdf document before converting the corresponding xml.</para>
        /// </summary>
        /// <param name="jsonPath">The path to the coco formated json file.</param>
        /// <param name="pdfFolderPath">The folder containing the previously extracted pdf documents.</param>
        public static void Convert(string jsonPath, string pdfFolderPath)
        {
            string logFilePath = Path.Combine(pdfFolderPath, logFile);

            using (FileStream s = File.Open(jsonPath, FileMode.Open))
            using (StreamReader sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();
                var cocoFile = serializer.Deserialize<CocoFile>(reader);

                Dictionary<int, string> categories = cocoFile.categories.ToDictionary(k => k.id, k => k.name);
                PageXmlTextExporter exporter = new PageXmlTextExporter(categories);

                var todo = cocoFile.images.Count;
                var done = 0;

                for (int i = 0; i < cocoFile.images.Count; i++)
                {
                    var image = cocoFile.images[i];
                    try
                    {
                        string pdfFilePath = Path.Combine(pdfFolderPath, GetPdfName(image.file_name));
                        if (File.Exists(pdfFilePath))
                        {
                            string outputFilePath = Path.ChangeExtension(Path.Combine(pdfFolderPath, image.file_name), "xml");
                            if (File.Exists(outputFilePath)) continue;

                            var entry = new CocoEntry(image.id, image.file_name, image.height, image.width);

                            var annotations = cocoFile.annotations.Where(a => a.image_id == image.id);
                            foreach (var annotation in annotations)
                            {
                                entry.Annotations.Add(annotation);
                            }

                            var pageXml = exporter.Get(entry);
                            File.WriteAllText(outputFilePath, pageXml);

                            done++;
                            if (done % 100 == 0) Console.WriteLine(done + "/" + todo);
                        }
                        else
                        {
                            todo--;
                        }
                    }
                    catch (Exception)
                    {
                        File.AppendAllText(logFilePath, image.file_name + Environment.NewLine);
                    }
                }
            }
        }

        public static void CheckAndDelete(string pdfFolderPat)
        {
            List<FileInfo> pdfToDelete = new List<FileInfo>();

            DirectoryInfo d = new DirectoryInfo(pdfFolderPat);
            var pdfFiles = d.GetFiles("*.pdf", SearchOption.TopDirectoryOnly);

            var todo = pdfFiles.Length;
            var done = 0;

            foreach (var pdfFile in pdfFiles)
            {
                var xmlFiles = d.GetFiles(pdfFile.Name.Replace(".pdf", "") + "_*.xml", SearchOption.TopDirectoryOnly);
                if (xmlFiles.Length == 0)
                {
                    pdfToDelete.Add(pdfFile);
                    File.AppendAllText(logDeleteFile, pdfFile.FullName + Environment.NewLine);
                }
                done++;
                if (done % 100 == 0) Console.WriteLine(done + "/" + todo);
            }

            // delete

        }

        public static string GetPdfName(string imageName)
        {
            return imageName.Split('_')[0] + ".pdf";
        }
    }
}
