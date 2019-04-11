using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using BGC.IO;
using BGC.IO.Compression;
using NUnit.Framework;

namespace BGC.Tests
{
    public class ZipTests
    {
        [Test]
        public void TestZip()
        {
            string compressedFilePath = DataManagement.PathForDataFile("Test", "ziptest.zip");
            string compressionPath = DataManagement.PathForDataSubDirectory("Test", "ZipTest");
            //Clear prior tests
            Directory.Delete(compressionPath, true);

            //
            //Create files with random contents
            //

            string[] fileNames = new string[]
            {
                "Baron.txt",
                "Silly test",
                "Oh_Noes).asdf",
                "Something is going to happen with this one maybe.int",
                "What.bgc"
            };

            string[] subpaths = new string[]
            {
                DataManagement.PathForDataSubDirectory("Test", "ZipTest"),
                DataManagement.PathForDataSubDirectory("Test", "ZipTest", "SubA"),
                DataManagement.PathForDataSubDirectory("Test", "ZipTest", "SubB")
            };

            string[,] fileContents = new string[subpaths.Length, 5];

            for (int p = 0; p < subpaths.Length; p++)
            {
                string datapath = subpaths[p];
                for (int i = 0; i < 5; i++)
                {
                    fileContents[p, i] = $"{UnityEngine.Random.value}\n{UnityEngine.Random.value}";
                    File.WriteAllText(
                        Path.Combine(datapath, fileNames[i]),
                        fileContents[p, i]);
                }
            }

            //
            //Compress contents
            //
            Zip.CompressDirectory(
                inputPath: compressionPath,
                outputFilePath: compressedFilePath);

            //
            //Delete Original Files
            //
            Directory.Delete(compressionPath, true);

            //
            //Decompress contents
            //
            Zip.DecompressFile(
                inputFilePath: compressedFilePath,
                outputPath: compressionPath);

            //
            //Verify Output
            //
            for (int p = 0; p < subpaths.Length; p++)
            {
                string datapath = subpaths[p];
                for (int i = 0; i < 5; i++)
                {
                    Assert.IsTrue(File.Exists(Path.Combine(datapath, fileNames[i])));
                    string fileText = File.ReadAllText(Path.Combine(datapath, fileNames[i]));
                    Assert.IsTrue(fileContents[p, i] == fileText);
                }
            }

            //
            //Delete Test Files
            //
            if (Directory.Exists(compressionPath))
            {
                Directory.Delete(compressionPath, true);
            }

            //
            //Delete Zip File
            //
            if (File.Exists(compressedFilePath))
            {
                File.Delete(compressedFilePath);
            }
        }
    }
}