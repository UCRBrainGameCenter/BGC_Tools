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
                using (ZipArchive archive = ZipFile.Open(inputFilePath, ZipArchiveMode.Read))
                {
                    DecompressFallback(archive, outputPath);
                }

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

        /// <summary>Decompresses a zip archive onto disk.</summary>
        /// <param name="inputFilePath">The absolute file path of the zip archive on local storage.</param>
        /// <param name="outputPath">The absolute path to where the archive should be output to on local storage.</param>
        /// <param name="progressReporter">Optional progress reporter.</param>
        /// <param name="abortToken">Optional cancellation token.</param>
        /// <returns>TRUE if successful. FALSE otherwise.</returns>
        public static Task<bool> DecompressFileAsync(
        string inputFilePath,
        string outputPath,
        IProgress<float> progressReporter = null,
        CancellationToken abortToken = default)
        {
            if (!File.Exists(inputFilePath))
            {
                Debug.LogError($"Zip Decompress Input File not found: {inputFilePath}");
                return Task.FromResult(false);
            }

            Directory.CreateDirectory(outputPath);

            // Do the extraction work on a background thread, but keep it single-threaded.
            return Task.Run(() =>
            {
                try
                {
                    using var archive = ZipFile.OpenRead(inputFilePath);

                    var fileEntries = archive.Entries
                        .Where(e => !string.IsNullOrEmpty(e.Name)) // directories have empty Name
                        .ToList();

                    int total = fileEntries.Count;
                    if (total == 0)
                    {
                        progressReporter?.Report(1f);
                        return true;
                    }

                    int done = 0;

                    foreach (var entry in fileEntries)
                    {
                        abortToken.ThrowIfCancellationRequested();

                        string destinationPath = GetSafeDestinationPath(outputPath, entry.FullName);

                        string destinationDir = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(destinationDir))
                        {
                            Directory.CreateDirectory(destinationDir);
                        }

                        // Extract
                        using var inStream = entry.Open();
                        using var outStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                        inStream.CopyTo(outStream);

                        done++;
                        progressReporter?.Report((float)done / total);
                    }

                    return true;
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
                catch (InvalidDataException ide)
                {
                    // Optional: fallback to ExtractToDirectory if your runtime supports it.
                    // Note: progress is not available here.
                    Debug.LogWarning($"Zip extraction hit InvalidDataException: {ide.Message}. Trying ExtractToDirectory fallback.");

                    try
                    {
                        abortToken.ThrowIfCancellationRequested();
                        ZipFile.ExtractToDirectory(inputFilePath, outputPath);
                        progressReporter?.Report(1f);
                        return true;
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Fallback ExtractToDirectory failed: {e.Message}");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Zip extraction failed: {e.Message}");
                    return false;
                }
            }, abortToken);
        }

        private static string GetSafeDestinationPath(string outputRoot, string entryFullName)
        {
            // Normalize separators to current platform
            string relativePath = entryFullName.Replace('\\', Path.DirectorySeparatorChar)
                                               .Replace('/', Path.DirectorySeparatorChar);

            // Prevent rooted paths from ignoring outputRoot
            while (relativePath.Length > 0 && (relativePath[0] == Path.DirectorySeparatorChar))
            {
                relativePath = relativePath.Substring(1);
            }

            string fullOutputRoot = Path.GetFullPath(outputRoot);
            string combined = Path.GetFullPath(Path.Combine(fullOutputRoot, relativePath));

            // Ensure the final path stays within outputRoot
            if (!combined.StartsWith(fullOutputRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(combined, fullOutputRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Blocked zip entry path traversal: {entryFullName}");
            }

            return combined;
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
