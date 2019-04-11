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
                Debug.LogError($"Exception Caught during processing: {ex}");
                return false;
            }

            return true;
        }

        public static bool DecompressMemory(
            byte[] compressedMemory,
            string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            try
            {
                using (Stream data = new MemoryStream(compressedMemory))
                using (ZipArchive archive = new ZipArchive(data, ZipArchiveMode.Read, false))
                {
                    archive.ExtractToDirectory(outputPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception Caught during processing of Zip Memory: {ex}");
                return false;
            }

            return true;
        }

        public static bool DecompressStream(
            Stream compressedStream,
            string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            try
            {
                using (ZipArchive archive = new ZipArchive(compressedStream, ZipArchiveMode.Read, false))
                {
                    archive.ExtractToDirectory(outputPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception Caught during processing of Zip Stream: {ex}");
                return false;
            }

            return true;
        }

        public static bool DecompressFile(
            string inputFilePath,
            string outputPath)
        {
            if (!File.Exists(inputFilePath))
            {
                Debug.LogError($"Input File not found: {inputFilePath}");
                return false;
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            try
            {
                ZipFile.ExtractToDirectory(inputFilePath, outputPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception Caught during processing of ZipFile: {ex}");
                return false;
            }

            return true;
        }
    }
}
