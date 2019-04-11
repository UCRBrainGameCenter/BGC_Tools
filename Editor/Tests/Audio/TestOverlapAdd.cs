using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using BGC.Audio;
using BGC.IO;
using BGC.Mathematics;

namespace BGC.Tests
{
    public class TestOverlapAdd
    {
        [Test]
        public void TestPhaseVocoding()
        {
            //string baseFile = "Boston_HitchARide";
            string baseFile = "000000";

            WaveEncoding.LoadBGCSimple(
                filepath: DataManagement.PathForDataFile("Test", $"{baseFile}.wav"),
                simpleAudioClip: out SimpleAudioClip song);

            song = song.Window(10f).Cache();

            //First, write unmodified
            WaveEncoding.SaveFile(
                filepath: DataManagement.PathForDataFile("Test", $"{baseFile}_Unmodified.wav"),
                channels: song.Channels,
                samples: song.Samples,
                overwrite: true);

            //Next, Slow it down 5%
            {
                SimpleAudioClip slowed_05 = song.PhaseVocode(0.95f).Cache();

                //Next, write it slowed 5%
                WaveEncoding.SaveFile(
                    filepath: DataManagement.PathForDataFile("Test", $"{baseFile}_Slowed_05.wav"),
                    channels: slowed_05.Channels,
                    samples: slowed_05.Samples,
                    overwrite: true);
            }

            //Next, Slow it down 25%
            {
                SimpleAudioClip slowed_25 = song.PhaseVocode(0.75f).Cache();

                //Next, write it slowed 5%
                WaveEncoding.SaveFile(
                    filepath: DataManagement.PathForDataFile("Test", $"{baseFile}_Slowed_25.wav"),
                    channels: slowed_25.Channels,
                    samples: slowed_25.Samples,
                    overwrite: true);
            }

            //Next, Slow it down 50%
            {
                SimpleAudioClip slowed_50 = song.PhaseVocode(0.5f).Cache();

                //Next, write it slowed 5%
                WaveEncoding.SaveFile(
                    filepath: DataManagement.PathForDataFile("Test", $"{baseFile}_Slowed_50.wav"),
                    channels: slowed_50.Channels,
                    samples: slowed_50.Samples,
                    overwrite: true);
            }

        }

        [Test]
        public void TestCarlileShuffler()
        {
            string baseFile = "000000";

            WaveEncoding.LoadBGCSimple(
                filepath: DataManagement.PathForDataFile("Test", $"{baseFile}.wav"),
                simpleAudioClip: out SimpleAudioClip song);

            Debug.Log($"Pre  RMS: {Mathf.Sqrt(song.Samples.Sum(x => x * x) / song.Samples.Length)}   N:{song.Samples.Length}");

            song = song.CarlileShuffle().Cache();

            Debug.Log($"Post RMS: {Mathf.Sqrt(song.Samples.Sum(x => x * x) / song.Samples.Length)}   N:{song.Samples.Length}");

            //Write to File
            WaveEncoding.SaveFile(
                filepath: DataManagement.PathForDataFile("Test", $"{baseFile}_Carlile.wav"),
                channels: song.Channels,
                samples: song.Samples,
                overwrite: true);

        }

        [Test]
        public void TestNewConvolution()
        {
            string baseFile = "000000";

            WaveEncoding.LoadBGCStream(
                filepath: DataManagement.PathForDataFile("Test", $"{baseFile}.wav"),
                stream: out IBGCStream stream);

            Debug.Log($"Pre  RMS: {string.Join(", ", stream.CalculateRMS().Select(x => x.ToString()).ToArray())}");

            {

                float[] filter1 = new float[150];
                filter1[25] = 1f;


                float[] filter2 = new float[150];

                for (int i = 0; i < 150; i++)
                {
                    filter2[i] = 1f / Mathf.Sqrt(150);
                }

                IBGCStream convolved = stream.MultiConvolve(filter1, filter2);


                string rms = string.Join(", ", convolved.CalculateRMS().Select(x => x.ToString()).ToArray());

                Debug.Log($"Post RMS: {rms}");

                //Write to File
                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"{baseFile}_Convolved.wav"),
                    stream: convolved,
                    overwrite: true);
            }

            {
                float[] filter1 = new float[150];
                filter1[25] = 1f / Mathf.Sqrt(2);
                filter1[26] = 1f / Mathf.Sqrt(2);


                float[] filter2 = new float[150];

                for (int i = 0; i < 150; i++)
                {
                    filter2[i] = 1f / 150f;
                }

                IBGCStream convolved = stream.MultiConvolve(filter1, filter2);


                string rms = string.Join(", ", convolved.CalculateRMS().Select(x => x.ToString()).ToArray());

                Debug.Log($"Post RMS: {rms}");

                //Write to File
                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"{baseFile}_Convolved2.wav"),
                    stream: convolved,
                    overwrite: true);
            }

        }

        [Test]
        public void TestNewSpatialization()
        {
            string baseFile = "000000";

            WaveEncoding.LoadBGCStream(
                filepath: DataManagement.PathForDataFile("Test", $"{baseFile}.wav"),
                stream: out IBGCStream stream);

            Debug.Log($"Pre  RMS: {string.Join(", ", stream.CalculateRMS().Select(x => x.ToString()).ToArray())}");

            {
                IBGCStream spatialized = stream.Spatialize(0f);

                string rms = string.Join(", ", spatialized.CalculateRMS().Select(x => x.ToString()).ToArray());

                Debug.Log($"Post RMS: {rms}");

                //Write to File
                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"{baseFile}_Spatialized_0.wav"),
                    stream: spatialized,
                    overwrite: true);
            }

            {
                IBGCStream spatialized = stream.Spatialize(25f);

                string rms = string.Join(", ", spatialized.CalculateRMS().Select(x => x.ToString()).ToArray());

                Debug.Log($"Post RMS: {rms}");

                //Write to File
                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"{baseFile}_Spatialized_25.wav"),
                    stream: spatialized,
                    overwrite: true);
            }

            {
                IBGCStream spatialized = stream.Spatialize(-25f);

                string rms = string.Join(", ", spatialized.CalculateRMS().Select(x => x.ToString()).ToArray());

                Debug.Log($"Post RMS: {rms}");

                //Write to File
                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"{baseFile}_Spatialized_n25.wav"),
                    stream: spatialized,
                    overwrite: true);
            }

        }

        [Test]
        public void TestNewFFTs()
        {
            Complex32[] samples = new Complex32[1000];

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = 1f;
            }

            Fourier.Forward(samples);

            for (int i = 0; i < 10; i++)
            {
                Debug.Log($"{i}: {samples[i]}");
            }

            Fourier.Inverse(samples);

            for (int i = 0; i < 10; i++)
            {
                Debug.Log($"{i}: {samples[i]}");
            }
        }


        private void DumpData(float[] data, string name)
        {
            using (StreamWriter writer = new StreamWriter($"Stimuli/{name}.csv"))
            {
                foreach (float sample in data)
                {
                    writer.WriteLine(sample.ToString());
                }
            }
        }

    }

}