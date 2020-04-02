using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace BGC.IO.Compression
{
    /// <summary>
    /// A collection of convenience methods to handle compression and
    /// decompression of zip files.
    /// </summary>
    public static class Zip
    {
        public static bool CompressDirectory(
            string inputPath,
            string outputFilePath)
        {
            if (!Directory.Exists(inputPath))
            {
                Debug.LogError($"Input Directory not found: {inputPath}");
                return false;
            }

            try
            {
                ZipFile.CreateFromDirectory(
                    sourceDirectoryName: inputPath,
                    destinationArchiveFileName: outputFilePath,
                    compressionLevel: CompressionLevel.Optimal,
                    includeBaseDirectory: false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Zip Exception Caught during compression: {ex}");
                return false;
            }

            return true;
        }

        public static bool DecompressMemory(
            byte[] compressedMemory,
            string outputPath)
        {
            using (Stream data = new MemoryStream(compressedMemory))
            using (ZipArchive archive = new ZipArchive(data, ZipArchiveMode.Read, false))
            {
                return Decompress(archive, outputPath);
            }
        }

        public static bool DecompressStream(
            Stream compressedStream,
            string outputPath)
        {
            using (ZipArchive archive = new ZipArchive(compressedStream, ZipArchiveMode.Read, false))
            {
                return Decompress(archive, outputPath);
            }
        }

        public static bool DecompressFile(
            string inputFilePath,
            string outputPath)
        {
            if (!File.Exists(inputFilePath))
            {
                Debug.LogError($"Zip Decompress Input File not found: {inputFilePath}");
                return false;
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            try
            {
                ZipFile.ExtractToDirectory(inputFilePath, outputPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Preferred Zip extraction of {inputFilePath} failed with \"{e.Message}\".  Trying fallback extraction to: {outputPath}");
            }

            try
            {
                using (ZipArchive archive = ZipFile.Open(inputFilePath, ZipArchiveMode.Read))
                {
                    DecompressFallback(archive, outputPath);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fallback Zip extraction of {inputFilePath} failed with \"{e.Message}\" to: {outputPath}");
            }

            return false;
        }

        private static bool Decompress(ZipArchive archive, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            try
            {
                DecompressPreferred(archive, outputPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Preferred Zip extraction failed with \"{e.Message}\".  Trying fallback extraction to: {outputPath}");
            }

            try
            {
                DecompressFallback(archive, outputPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fallback Zip extraction failed with \"{e.Message}\" to: {outputPath}");
            }

            return false;
        }

        /// <summary> Simpler and more performant zip extraction </summary>
        private static void DecompressPreferred(ZipArchive archive, string outputPath)
        {
            archive.ExtractToDirectory(outputPath);
        }

        /// <summary> Fallback method for some older android devices </summary>
        private static void DecompressFallback(ZipArchive archive, string outputPath)
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string filepath = Path.Combine(outputPath, entry.FullName);
                if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                {
                    //Create the subdirectory if it doesn't already exist
                    if (!Directory.Exists(filepath))
                    {
                        Directory.CreateDirectory(filepath);
                    }

                    //Skip directories
                    continue;
                }

                //Delete file if it already exists
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }

                //Copy binary data into new file
                using (Stream entryStream = entry.Open())
                using (FileStream fileStream = File.OpenWrite(filepath))
                {
                    entryStream.CopyTo(fileStream);
                }
            }
        }
    }
}
