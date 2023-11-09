using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

                if (Utility.IsDiskFullException(e))
                {
                    throw;
                }
            }

            try
            {
                using ZipArchive archive = ZipFile.Open(inputFilePath, ZipArchiveMode.Read);
                DecompressFallback(archive, outputPath);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Fallback Zip extraction of {inputFilePath} failed with \"{e.Message}\" to: {outputPath}");
                
                if (Utility.IsDiskFullException(e))
                {
                    throw;
                }
            }

            return false;
        }
        
        /// <summary>Decompresses a zip archive onto disc.</summary>
        /// <param name="inputFilePath">The absolute file path of the zip archive on local storage.</param>
        /// <param name="outputPath">The absolute path to where the archive should be output to on local storage.</param>
        /// <param name="progressReporter">Optional progress reporter.</param>
        /// <param name="abortToken">Optional cancellation token.</param>
        /// <returns>TRUE if successful. FALSE otherwise.</returns>
        public static async Task<bool> DecompressFileAsync(
            string inputFilePath,
            string outputPath,
            IProgress<float> progressReporter = null,
            CancellationToken abortToken = default)
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
                using ZipArchive archive = ZipFile.OpenRead(inputFilePath);

                float newCurrentProgress = 0f;
                int totalFiles = archive.Entries.Count;

                if (abortToken.IsCancellationRequested)
                {
                    return false;
                }

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fullPath = Path.Combine(outputPath, entry.FullName);

                    string dirName = Path.GetDirectoryName(entry.FullName);

                    if (!string.IsNullOrEmpty(dirName))
                    {
                        string dirPath = Path.Combine(outputPath, dirName);
                        Directory.CreateDirectory(dirPath);
                    }

                    if (entry.FullName.Last() == '/')
                    {
                        // If we're here, skip because this entry is a directory
                        continue;
                    }

                    entry.ExtractToFile(fullPath);
                    // await WriteArchiveEntry(
                    //     entry,
                    //     fullPath,
                    //     abortToken);
                    
                    newCurrentProgress += 1f / totalFiles;
                    progressReporter?.Report(newCurrentProgress);

                    if (abortToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    // await Task.Yield();
                }

                return true;
            }
            catch (InvalidDataException)
            {
                // We might be dealing with an incompatible zip64 file, as per MS issue report:
                // https://github.com/dotnet/runtime/issues/49580
                
                // in this case, fallback to try and use the known working method. Unfortunately, we can't report
                // progress with this.

                try
                {
                    Task writeTask = Task.Run(() => ZipFile.ExtractToDirectory(inputFilePath, outputPath), abortToken);

                    float prog = 0f;
                    while (!writeTask.IsCompleted)
                    {
                        // fake the progress
                        await Task.Yield();
                        // await Task.Delay(1, abortToken);
                        prog += 0.001f;
                        progressReporter?.Report(prog);
                    }
                    
                    progressReporter?.Report(1f);
                
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Preferred Zip extraction of {inputFilePath} failed with \"{e.Message}\".  Trying fallback extraction to: {outputPath}");

                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Preferred Zip extraction of {inputFilePath} failed with \"{e.Message}\".  Trying fallback extraction to: {outputPath}");

                return false;
            }
        }

        private static async Task WriteArchiveEntry(
            ZipArchiveEntry entry,
            string filepath,
            CancellationToken abortToken)
        { 
            abortToken.ThrowIfCancellationRequested();
            if (Path.HasExtension(filepath))
            {
                // we may have entries that represent a directory. Rule those out and only extract files.

                //Copy binary data into new file
                await using FileStream fileStream = new FileStream(
                    filepath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
            true);
                
                await using Stream entryStream = entry.Open();
                await entryStream.CopyToAsync(fileStream, 81920, abortToken);
            }
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
        
        /// <summary> Fallback method for some older android devices </summary>
        private static async Task DecompressFallbackAsync(ZipArchive archive, string outputPath)
        {
            List<Task> tasks = new List<Task>();
            
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                tasks.Add(Task.Run(() =>
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
                        return;
                    }

                    //Delete file if it already exists
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }

                    //Copy binary data into new file
                    using Stream entryStream = entry.Open();
                    using FileStream fileStream = File.OpenWrite(filepath);
                    entryStream.CopyTo(fileStream);
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}
