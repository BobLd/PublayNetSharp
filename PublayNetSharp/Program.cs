using System;
using System.IO;

namespace PublayNetSharp
{
    class Program
    {
        const string outputFolder = @"D:\Datasets\Document Layout Analysis\PubLayNet\extracted_v2\val";
        const string tarGzFolder = @"D:\Datasets\Document Layout Analysis\PubLayNet\publaynet\val\pdf";
        const string jsonPath = @"D:\Datasets\Document Layout Analysis\PubLayNet\publaynet\val.json";

        static void Main(string[] args)
        {
            TarGz.ExtractAll(tarGzFolder, outputFolder);
            CocoPageXml.Convert(jsonPath, outputFolder);
        }
    }
}
