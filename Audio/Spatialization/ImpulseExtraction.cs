using System;
using System.Threading.Tasks;
using UnityEngine;
using BGC.IO;
using BGC.IO.Compression;
using BGC.MonoUtility;

namespace BGC.Audio.Spatialization
{
    /// <summary>
    /// Extracts the packaged HRTF files, generously provided by
    /// Nirmal Srinivasan, Asst Professor of the Deptartment of 
    /// Speech-Language Pathology & Audiology at Towson University
    /// </summary>
    public class ImpulseExtraction : AsyncInitTask
    {
        [SerializeField]
        private TextAsset impulseZip = null;

        private const string HRTFDirectory = "HRTF";
        private const string ImpulseVersionKey = "ImpulseVersion";
        private const int ImpulseVersion = 3;

        private byte[] rawData;
        private string outputPath;

        protected override bool PrepareRun()
        {
            if (PlayerPrefs.GetInt(ImpulseVersionKey, 0) < ImpulseVersion ||
                !DataManagement.DataDirectoryExists(HRTFDirectory) ||
                !System.IO.File.Exists(DataManagement.PathForDataFile(HRTFDirectory, "0_impulse.wav")))
            {
                rawData = (byte[])impulseZip.bytes.Clone();
                outputPath = DataManagement.PathForDataDirectory(HRTFDirectory);
                return true;
            }

            return false;
        }

        protected override void FinishedRunning(bool runSuccessful)
        {
            if (runSuccessful)
            {
                PlayerPrefs.SetInt(ImpulseVersionKey, ImpulseVersion);
            }
        }

        protected override Task<bool> ExecuteTask() => Task.Run(() =>
            Zip.DecompressMemory(
                compressedMemory: rawData,
                outputPath: outputPath));
    }
}
