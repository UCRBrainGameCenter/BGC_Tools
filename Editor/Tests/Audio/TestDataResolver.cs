using System.IO;
using System.Linq;
using BGC.IO;

namespace BGC.Tests
{
    /// <summary>
    /// Locates audio fixtures for the generator / DSP smoke tests.
    ///
    /// Historically these tests read inputs from a flat "Test/" folder under the project
    /// root. That input data now ships through the PART Downloader (extracted under
    /// <c>DownloadManagerCache/&lt;localId&gt;/&lt;hash&gt;.zip/</c>) and is also committed under
    /// <c>DownloadableAssets/</c>. This resolver checks all of those, so the tests run for
    /// real when the corpus is present and callers can skip cleanly when it is genuinely
    /// absent (a checkout without the corpus, or unpulled Git-LFS pointers).
    /// </summary>
    public static class TestDataResolver
    {
        // Real corpus clips are kilobytes. An unpulled Git-LFS pointer for a committed
        // .wav is ~130 bytes, so anything below this threshold is treated as "not really
        // present" (File.Exists would otherwise return true for a pointer stub).
        private const long MinRealFileBytes = 1024;

        // Searched in priority order, relative to DataManagement.RootDirectory.
        private static readonly string[] SearchRoots =
        {
            // Downloaded + extracted module cache — always real bytes when present.
            "DownloadManagerCache",
            // Committed corpus source — may be a Git-LFS pointer if LFS was not pulled.
            "DownloadableAssets",
        };

        /// <summary>
        /// Try to locate <paramref name="fileName"/>. Any <paramref name="preferredRelativePaths"/>
        /// (relative to the data root, e.g. "Test/000000.wav") are checked first; otherwise the
        /// known corpus roots are searched recursively by file name. Returns false when no real
        /// (non-pointer) copy can be found, so the caller can Assert.Ignore.
        /// </summary>
        public static bool TryResolve(string fileName, out string fullPath, params string[] preferredRelativePaths)
        {
            foreach (string relativePath in preferredRelativePaths)
            {
                string candidate = Path.Combine(DataManagement.RootDirectory, relativePath);
                if (IsRealFile(candidate))
                {
                    fullPath = candidate;
                    return true;
                }
            }

            foreach (string root in SearchRoots)
            {
                string rootPath = Path.Combine(DataManagement.RootDirectory, root);
                if (!Directory.Exists(rootPath))
                {
                    continue;
                }

                // Deterministic first match (e.g. CRM 000000.wav exists for every talker).
                string match = Directory
                    .EnumerateFiles(rootPath, fileName, SearchOption.AllDirectories)
                    .OrderBy(path => path, System.StringComparer.Ordinal)
                    .FirstOrDefault(IsRealFile);

                if (match != null)
                {
                    fullPath = match;
                    return true;
                }
            }

            fullPath = null;
            return false;
        }

        private static bool IsRealFile(string path) =>
            File.Exists(path) && new FileInfo(path).Length >= MinRealFileBytes;
    }
}
