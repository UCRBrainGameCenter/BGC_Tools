using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace BGC.Audio
{
    /// <summary>
    /// Operations for Loading and Saving WAV files
    /// Some documentation on the WAV format is available here:
    /// http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/WAVE.html
    /// </summary>
    public static class WaveEncoding
    {
        public enum Format : ushort
        {
            UNDEFINED = 0x0000,
            PCM = 0x0001,
            IEEE_Float = 0x0003,
            A_Law = 0x0006,
            u_Law = 0x0007,
            Extensible = 0xFFFE
        }

        const float toInt16Factor = 32767f; //to convert float to Int16
        const float toFloatFactor = 1f / 32767f; //to convert float to Int16

        /// <summary>
        /// Loads a WAV file from the filepath as a SimpleAudioClip, returns success.
        /// </summary>
        public static bool LoadBGCStream(
            string filepath,
            out IBGCStream stream)
        {
            if (ReadFile(
                filepath: filepath,
                dataParser: SimpleReadDATA,
                dataResampler: LinearInterpolation.Resample,
                parsedData: out float[] samples,
                channels: out int channels))
            {
                stream = new SimpleAudioClip(samples, channels);
                return true;
            }

            stream = null;
            return false;
        }

        /// <summary>
        /// Loads a WAV file from the filepath as a SimpleAudioClip, returns success.
        /// </summary>
        public static bool LoadBGCSimple(
            string filepath,
            out SimpleAudioClip simpleAudioClip)
        {
            if (ReadFile(
                filepath: filepath,
                dataParser: SimpleReadDATA,
                dataResampler: LinearInterpolation.Resample,
                parsedData: out float[] samples,
                channels: out int channels))
            {
                simpleAudioClip = new SimpleAudioClip(samples, channels);
                return true;
            }

            simpleAudioClip = null;
            return false;
        }

        /// <summary>
        /// Loads a WAV file from the filepath as a InterlacingAudioClip, returns success.
        /// </summary>
        public static bool LoadBGCStereo(
            string filepath,
            out InterlacingAudioClip interlacingAudioClip)
        {
            if (LoadStereoFile(
                filepath: filepath,
                leftSamples: out float[] leftSamples,
                rightSamples: out float[] rightSamples))
            {
                interlacingAudioClip = new InterlacingAudioClip(leftSamples, rightSamples);
                return true;
            }

            interlacingAudioClip = null;
            return false;
        }


        /// <summary>
        /// Loads a WAV file from the filepath as a float array, returns success.
        /// </summary>
        public static bool LoadFile(
            string filepath,
            out int channels,
            out float[] samples)
        {
            return ReadFile(
                filepath: filepath,
                dataParser: SimpleReadDATA,
                dataResampler: LinearInterpolation.Resample,
                parsedData: out samples,
                channels: out channels);
        }

        /// <summary>
        /// Loads a Stereo WAV file from the filepath as two float arrays, returns success.
        /// </summary>
        public static bool LoadStereoFile(
            string filepath,
            out float[] leftSamples,
            out float[] rightSamples)
        {
            bool success = ReadFile(
                filepath: filepath,
                dataParser: ReadStereoDATA,
                dataResampler: LinearInterpolation.Resample,
                parsedData: out (float[] leftSamples, float[] rightSamples) samples,
                channels: out int channels);

            leftSamples = samples.leftSamples;
            rightSamples = samples.rightSamples;

            return success;
        }

        /// <summary>
        /// Loads a WAV file from the filepath as a PCM short array, returns success.
        /// </summary>
        public static bool LoadPCMFile(
            string filepath,
            out int channels,
            out short[] samples)
        {
            return ReadFile(
                filepath: filepath,
                dataParser: ReadPCMDATA,
                dataResampler: LinearInterpolation.Resample,
                parsedData: out samples,
                channels: out channels);
        }

        /// <summary>
        /// Save the samples passed in as a WAVE file with the specified filepath.  Returns success.
        /// </summary>
        public static bool SaveStream(
            string filepath,
            IBGCStream stream,
            bool overwrite = false)
        {
            return WriteFile(
                filepath: filepath,
                data: stream,
                channels: stream.Channels,
                dataWriter: StreamWriter,
                overwrite: overwrite);
        }

        /// <summary>
        /// Save the samples passed in as a WAVE file with the specified filepath.  Returns success.
        /// </summary>
        public static bool SaveFile(
            string filepath,
            int channels,
            float[] samples,
            bool overwrite = false)
        {
            return WriteFile(
                filepath: filepath,
                data: samples,
                channels: channels,
                dataWriter: SimpleWriter,
                overwrite: overwrite);
        }

        /// <summary>
        /// Save the samples passed in as a WAVE file with the specified filepath.  Returns success.
        /// </summary>
        public static bool SaveFile(
            string filepath,
            int channels,
            short[] samples,
            bool overwrite = false)
        {
            return WriteFile(
                filepath: filepath,
                data: samples,
                channels: channels,
                dataWriter: SimpleWriter,
                overwrite: overwrite);
        }

        /// <summary>
        /// Save the samples passed in as a WAVE file with the specified filepath.  Returns success.
        /// </summary>
        public static bool SaveFile(
            string filepath,
            int channels,
            float[] leftSamples,
            float[] rightSamples,
            bool overwrite = false)
        {
            return WriteFile(
                filepath: filepath,
                data: (leftSamples, rightSamples),
                channels: channels,
                dataWriter: StereoWriter,
                overwrite: overwrite);
        }

        /// <summary>
        /// Internal method to load a WAV file an parse the data
        /// </summary>
        private static bool ReadFile<T>(
            string filepath,
            DataParser<T> dataParser,
            DataResampler<T> dataResampler,
            out T parsedData,
            out int channels)
        {
            parsedData = default;
            channels = 0;

            if (!File.Exists(filepath))
            {
                Debug.LogError($"File {filepath} does not exist.");
                return false;
            }

            using (FileStream fileStream = File.OpenRead(filepath))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                byte[] smallBuffer = new byte[12];

                fileStream.Read(smallBuffer, 0, 12);
                //RIFF
                string header = Encoding.UTF8.GetString(smallBuffer, 0, 4);
                Assert.IsTrue(header == "RIFF", $"Unexpected initial chunkID: {header}");

                //Chunk Size
                int headerSize = BitConverter.ToInt32(smallBuffer, 4);

                //WAVE ID
                string waveID = Encoding.UTF8.GetString(smallBuffer, 8, 4);
                Assert.IsTrue(waveID == "WAVE", $"Unexpected WaveID: {waveID}");

                bool readFormat = false;
                bool readData = false;

                int samplingRate = 44100;
                Format format = Format.UNDEFINED;

                //Now Read Chunks
                //Each chunk starts with an 8 byte header, so we probably reached padding if
                //there is less than that remaining
                while (fileStream.CanRead && (fileStream.Length - fileStream.Position) >= 8)
                {
                    fileStream.Read(smallBuffer, 0, 8);
                    string chunkID = Encoding.UTF8.GetString(smallBuffer, 0, 4).Trim();
                    int chunkSize = BitConverter.ToInt32(smallBuffer, 4);

                    //Read chunk
                    byte[] tempBuffer = new byte[chunkSize];
                    fileStream.Read(tempBuffer, 0, chunkSize);

                    switch (chunkID)
                    {
                        case "fmt":
                            readFormat = ReadFMT(
                                filepath: filepath,
                                buffer: tempBuffer,
                                channels: out channels,
                                samplingRate: out samplingRate,
                                format: out format);
                            break;

                        case "fact":
                            //Fact chunks report the number of samples per channel and are optional
                            //Do nothing with it
                            break;

                        case "id3":
                        case "minf":
                        case "elm1":
                        case "regn":
                        case "umid":
                        case "DGDA":
                        case "JUNK":
                        case "bext":
                        case "cue":
                        case "LIST":
                        case "_PMX":
                            //Some chunks we have seen and identified as unneeded.
                            break;

                        case "data":
                            if (!readFormat)
                            {
                                Debug.LogError($"fmt chunk not found in {filepath} before data.");
                                channels = 0;
                                return false;
                            }

                            if (readData)
                            {
                                Debug.LogError($"Found a second data chunk. Not built to handle that.");
                                parsedData = default;
                                channels = 0;
                                return false;
                            }

                            readData = dataParser(
                                rawData: tempBuffer,
                                format: format,
                                channels: channels,
                                parsedData: out parsedData);

                            if (!readData)
                            {
                               Debug.LogError($"Failed to successfully read data chunk");
                            }
                            break;

                        default:
                            Debug.Log($"Skipping unexpected Chunk in File {filepath}: {chunkID}.");
                            //Do nothing with it
                            break;
                    }
                }

                if (!parsedData.Equals(default(T)) && samplingRate != 44100)
                {
                    parsedData = dataResampler(
                        inputData: parsedData,
                        inputSamplingRate: samplingRate,
                        outputSamplingRate: 44100,
                        channels: channels);
                }

                return readData;
            }
        }

        /// <summary>
        /// Read in Format Chunk.  Returns success
        /// </summary>
        private static bool ReadFMT(
            string filepath,
            byte[] buffer,
            out int channels,
            out int samplingRate,
            out Format format)
        {
            Assert.IsTrue(buffer.Length >= 16, $"Unexpected fmt ChunkSize: {buffer.Length}");

            //Format Tag - 2 bytes (PCM)
            format = (Format)BitConverter.ToUInt16(buffer, 0);

            switch (format)
            {
                case Format.PCM:
                case Format.IEEE_Float:
                case Format.Extensible:
                case Format.u_Law:
                    //Supported
                    break;

                case Format.A_Law:
                    Debug.LogError($"Unsupported Format: ({format}) on file {filepath}");
                    channels = 0;
                    samplingRate = 0;
                    return false;

                default:
                    Debug.LogError($"Unrecognized Format: ({format}) on file {filepath}");
                    channels = 0;
                    samplingRate = 0;
                    return false;
            }


            //Number of Channels - 2 bytes
            channels = BitConverter.ToInt16(buffer, 2);
            Assert.IsTrue(channels == 1 || channels == 2, $"Unsupported Channels: {channels}");

            //Sampling Rate - 4 bytes
            samplingRate = BitConverter.ToInt32(buffer, 4);
            if (samplingRate != 44100)
            {
                Debug.LogWarning($"Sampling rate ({samplingRate}) not 44100.  Resampling {filepath}");
            }

            //Average Bytes Per Second - 4 bytes
            int byteRate = BitConverter.ToInt32(buffer, 8);
            int bytesPerSample = byteRate / (samplingRate * channels);
            Assert.IsTrue(bytesPerSample == 2 || bytesPerSample == 4, $"Unexpected ByteRate: {byteRate}");

            //Block Align - 2 bytes
            short blockAlign = BitConverter.ToInt16(buffer, 12);
            Assert.IsTrue(blockAlign == channels * bytesPerSample, $"Unexpected BlockAlign: {blockAlign}");

            //Bits Per Sample - 2 bytes
            short bps = BitConverter.ToInt16(buffer, 14);
            Assert.IsTrue(bps == 8 * bytesPerSample, $"Unexpected BitsPerSample: {bps}");

            //Extension Size - 2 bytes
            //(We don't care about the extension)
            //short extensionSize = BitConverter.ToInt16(buffer, 16);

            if (format == Format.Extensible)
            {
                short validBitsPerSample = BitConverter.ToInt16(buffer, 18);
                if (validBitsPerSample != 16)
                {
                    Debug.LogError($"Unexpected ValidBitsPerSample: {validBitsPerSample}.  Continuing.");
                }

                //Real format code
                format = (Format)BitConverter.ToUInt16(buffer, 24);

                switch (format)
                {
                    case Format.PCM:
                        //Supported
                        break;

                    case Format.IEEE_Float:
                        //Supported
                        break;

                    case Format.u_Law:
                        //Supported
                        break;

                    case Format.A_Law:
                    case Format.Extensible:
                        Debug.LogError($"Unexpected Data Format Code: ({format}) on file {filepath}");
                        channels = 0;
                        samplingRate = 0;
                        return false;

                    default:
                        Debug.LogError($"Unrecognized Data Format Code: ({format}) on file {filepath}");
                        channels = 0;
                        samplingRate = 0;
                        return false;
                }
            }

            return true;
        }

        private static FileStream CreateFile(string filepath, int samples, int channels = 2)
        {
            const int hz = 44100;

            const int waveIDSize = 4;
            const int chunkHeaderSize = 8;
            const int fmtSize = 18;
            const int factSize = 4;
            int dataSize = 2 * samples;

            FileStream fileStream = new FileStream(filepath, FileMode.Create);

            //The size of the file chunk is the "WAVE" label, plus the sizes of each chunk and each
            //of their headers
            int fileSize = waveIDSize + fmtSize + factSize + dataSize + 3 * chunkHeaderSize;

            fileStream.Seek(0, SeekOrigin.Begin);

            //
            //File Chunk Header
            //
            //Chunk ID - 4 bytes
            fileStream.Write(Encoding.UTF8.GetBytes("RIFF"), 0, 4);
            //Chunk Size - 4 bytes
            fileStream.Write(BitConverter.GetBytes(fileSize), 0, 4);

            //
            //File Chunk
            //
            //Wave ID - 4 bytes
            fileStream.Write(Encoding.UTF8.GetBytes("WAVE"), 0, 4);

            //
            //Format Chunk Header
            //
            //Chunk ID - 4 bytes
            fileStream.Write(Encoding.UTF8.GetBytes("fmt "), 0, 4);
            //Chunk Size - 4 bytes  (18 bytes of data)
            fileStream.Write(BitConverter.GetBytes(fmtSize), 0, 4);

            //
            //Format Chunk
            //
            //Format Tag - 2 bytes (PCM)
            fileStream.Write(BitConverter.GetBytes((short)1), 0, 2);
            //Number of Channels - 2 bytes
            fileStream.Write(BitConverter.GetBytes((short)channels), 0, 2);
            //Sampling Rate - 4 bytes
            fileStream.Write(BitConverter.GetBytes(hz), 0, 4);
            //Average Bytes Per Second - 4 bytes
            fileStream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4);
            //Block Align - 2 bytes
            fileStream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2);
            //Bits Per Sample - 2 bytes
            fileStream.Write(BitConverter.GetBytes((short)16), 0, 2);
            //Extension Size - 2 bytes (0, no extension provided)
            fileStream.Write(BitConverter.GetBytes((short)0), 0, 2);

            //
            //Fact Chunk Header
            //
            //Chunk ID - 4 bytes
            fileStream.Write(Encoding.UTF8.GetBytes("fact"), 0, 4);
            //Chunk Size - 4 bytes
            fileStream.Write(BitConverter.GetBytes(factSize), 0, 4);

            //
            //Fact Chunk
            //
            //Sample Length - 4 bytes
            fileStream.Write(BitConverter.GetBytes(samples / channels), 0, 4);

            //
            //Data Chunk Header
            //
            //Chunk ID - 4 bytes
            fileStream.Write(Encoding.UTF8.GetBytes("data"), 0, 4);
            //Chunk Size - 4 bytes
            fileStream.Write(BitConverter.GetBytes(samples * 2), 0, 4);

            return fileStream;
        }


        #region Data Parsing Methods

        private delegate bool DataParser<T>(
            byte[] rawData,
            Format format,
            int channels,
            out T parsedData);

        private delegate T DataResampler<T>(
            T inputData,
            double inputSamplingRate,
            double outputSamplingRate,
            int channels);

        /// <summary>
        /// Read in DATA chunk.  Returns success.
        /// </summary>
        private static bool ReadPCMDATA(
            byte[] buffer,
            Format format,
            int channels,
            out short[] samples)
        {
            int sampleCount;

            switch (format)
            {
                case Format.PCM:
                    //2 Bytes per sample
                    sampleCount = buffer.Length / 2;
                    samples = new short[sampleCount];
                    for (int i = 0; i < sampleCount; i++)
                    {
                        samples[i] = BitConverter.ToInt16(buffer, 2 * i);
                    }
                    return true;

                case Format.u_Law:
                    //2 Bytes per sample
                    sampleCount = buffer.Length / 2;
                    samples = new short[sampleCount];
                    {
                        int sampleIn;
                        int sign;
                        int mantissa;
                        int exponent;
                        int segment;
                        int step;

                        for (int i = 0; i < sampleCount; i++)
                        {
                            sampleIn = BitConverter.ToInt16(buffer, 2 * i);
                            sign = sampleIn < 0b1000_0000 ? -1 : 1;
                            mantissa = ~sampleIn;
                            exponent = (mantissa >> 4) & 0b0000_0111;
                            segment = exponent + 1;
                            mantissa &= 0b0000_1111;
                            step = 4 << segment;
                            samples[i] = (short)(sign * (((0b1000_0000) << exponent) + step * mantissa + step / 2 - 4 * 33));
                        }

                    }
                    return true;

                case Format.IEEE_Float:
                case Format.UNDEFINED:
                case Format.A_Law:
                case Format.Extensible:
                    Debug.LogError($"Unsupported Format: {format}");
                    break;

                default:
                    Debug.LogError($"Unrecognized Format: {format}");
                    break;
            }


            samples = null;
            return false;
        }

        /// <summary>
        /// Read in DATA chunk.  Returns success.
        /// </summary>
        private static bool SimpleReadDATA(
            byte[] buffer,
            Format format,
            int channels,
            out float[] samples)
        {
            int sampleCount;

            switch (format)
            {
                case Format.PCM:
                    //2 Bytes per sample
                    sampleCount = buffer.Length / 2;
                    samples = new float[sampleCount];
                    for (int i = 0; i < sampleCount; i++)
                    {
                        samples[i] = toFloatFactor * BitConverter.ToInt16(buffer, 2 * i);
                    }
                    return true;

                case Format.IEEE_Float:
                    //4 Bytes per sample
                    sampleCount = buffer.Length / 4;
                    samples = new float[sampleCount];
                    for (int i = 0; i < sampleCount; i++)
                    {
                        samples[i] = BitConverter.ToSingle(buffer, 4 * i);
                    }
                    return true;

                case Format.u_Law:
                    //2 Bytes per sample
                    sampleCount = buffer.Length / 2;
                    samples = new float[sampleCount];
                    {
                        int sampleIn;
                        int sampleOut;
                        int sign;
                        int mantissa;
                        int exponent;
                        int segment;
                        int step;

                        for (int i = 0; i < sampleCount; i++)
                        {
                            sampleIn = BitConverter.ToInt16(buffer, 2 * i);
                            sign = sampleIn < 0b1000_0000 ? -1 : 1;
                            mantissa = ~sampleIn;
                            exponent = (mantissa >> 4) & 0b0000_0111;
                            segment = exponent + 1;
                            mantissa &= 0b0000_1111;
                            step = 4 << segment;
                            sampleOut = (sign * (((0b1000_0000) << exponent) + step * mantissa + step / 2 - 4 * 33));
                            samples[i] = toFloatFactor * sampleOut;
                        }

                    }
                    return true;

                case Format.UNDEFINED:
                case Format.A_Law:
                case Format.Extensible:
                    Debug.LogError($"Unsupported Format: {format}");
                    break;

                default:
                    Debug.LogError($"Unrecognized Format: {format}");
                    break;
            }


            samples = null;
            return false;
        }

        /// <summary>
        /// Read in Stereo DATA chunk.  Returns success.
        /// </summary>
        private static bool ReadStereoDATA(
            byte[] buffer,
            Format format,
            int channels,
            out (float[] leftSamples, float[] rightSamples) samples)
        {
            if (channels == 1)
            {
                Debug.LogError("Expected 2 channels, found 1.");
                samples.leftSamples = null;
                samples.rightSamples = null;
                return false;
            }

            int sampleCount;

            switch (format)
            {
                case Format.PCM:
                    //2 Channels at 2 Bytes per sample
                    sampleCount = buffer.Length / 4;
                    samples.leftSamples = new float[sampleCount];
                    samples.rightSamples = new float[sampleCount];

                    for (int i = 0; i < sampleCount; i++)
                    {
                        samples.leftSamples[i] = toFloatFactor * BitConverter.ToInt16(buffer, 4 * i);
                        samples.rightSamples[i] = toFloatFactor * BitConverter.ToInt16(buffer, 4 * i + 2);
                    }
                    return true;

                case Format.IEEE_Float:
                    //2 Channels at 4 Bytes per sample
                    sampleCount = buffer.Length / 8;
                    samples.leftSamples = new float[sampleCount];
                    samples.rightSamples = new float[sampleCount];

                    for (int i = 0; i < sampleCount; i++)
                    {
                        samples.leftSamples[i] = BitConverter.ToSingle(buffer, 8 * i);
                        samples.rightSamples[i] = BitConverter.ToSingle(buffer, 8 * i + 4);
                    }
                    return true;

                case Format.u_Law:
                    //2 Channels at 2 Bytes per sample
                    sampleCount = buffer.Length / 4;
                    samples.leftSamples = new float[sampleCount];
                    samples.rightSamples = new float[sampleCount];
                    {
                        int sampleIn;
                        int sampleOut;
                        int sign;
                        int mantissa;
                        int exponent;
                        int segment;
                        int step;

                        for (int i = 0; i < sampleCount; i++)
                        {
                            sampleIn = BitConverter.ToInt16(buffer, 4 * i);
                            sign = sampleIn < 0b1000_0000 ? -1 : 1;
                            mantissa = ~sampleIn;
                            exponent = (mantissa >> 4) & 0b0000_0111;
                            segment = exponent + 1;
                            mantissa &= 0b0000_1111;
                            step = 4 << segment;
                            sampleOut = (sign * (((0b1000_0000) << exponent) + step * mantissa + step / 2 - 4 * 33));
                            samples.leftSamples[i] = toFloatFactor * sampleOut;

                            sampleIn = BitConverter.ToInt16(buffer, 4 * i + 2);
                            sign = sampleIn < 0b1000_0000 ? -1 : 1;
                            mantissa = ~sampleIn;
                            exponent = (mantissa >> 4) & 0b0000_0111;
                            segment = exponent + 1;
                            mantissa &= 0b0000_1111;
                            step = 4 << segment;
                            sampleOut = (sign * (((0b1000_0000) << exponent) + step * mantissa + step / 2 - 4 * 33));
                            samples.rightSamples[i] = toFloatFactor * sampleOut;
                        }
                    }
                    return true;




                case Format.UNDEFINED:
                case Format.A_Law:
                case Format.Extensible:
                    Debug.LogError($"Unsupported Format: {format}");
                    break;

                default:
                    Debug.LogError($"Unrecognized Format: {format}");
                    break;
            }

            samples.leftSamples = null;
            samples.rightSamples = null;
            return false;
        }

        /// <summary>
        /// Read in Stereo DATA chunk.  Returns success.
        /// </summary>
        private static bool ReadStereoDATA(
            byte[] buffer,
            Format format,
            int channels,
            out float[] leftSamples,
            out float[] rightSamples)
        {
            int sampleCount;

            switch (format)
            {
                case Format.PCM:
                    //2 Channels at 2 Bytes per sample
                    sampleCount = buffer.Length / 4;
                    leftSamples = new float[sampleCount];
                    rightSamples = new float[sampleCount];

                    for (int i = 0; i < sampleCount; i++)
                    {
                        leftSamples[i] = toFloatFactor * BitConverter.ToInt16(buffer, 4 * i);
                        rightSamples[i] = toFloatFactor * BitConverter.ToInt16(buffer, 4 * i + 2);
                    }
                    return true;

                case Format.IEEE_Float:
                    //2 Channels at 4 Bytes per sample
                    sampleCount = buffer.Length / 8;
                    leftSamples = new float[sampleCount];
                    rightSamples = new float[sampleCount];

                    for (int i = 0; i < sampleCount; i++)
                    {
                        leftSamples[i] = BitConverter.ToSingle(buffer, 8 * i);
                        rightSamples[i] = BitConverter.ToSingle(buffer, 8 * i + 4);
                    }
                    return true;

                case Format.u_Law:
                    //2 Channels at 2 Bytes per sample
                    sampleCount = buffer.Length / 4;
                    leftSamples = new float[sampleCount];
                    rightSamples = new float[sampleCount];
                    {
                        int sampleIn;
                        int sampleOut;
                        int sign;
                        int mantissa;
                        int exponent;
                        int segment;
                        int step;

                        for (int i = 0; i < sampleCount; i++)
                        {
                            sampleIn = BitConverter.ToInt16(buffer, 4 * i);
                            sign = sampleIn < 0b1000_0000 ? -1 : 1;
                            mantissa = ~sampleIn;
                            exponent = (mantissa >> 4) & 0b0000_0111;
                            segment = exponent + 1;
                            mantissa &= 0b0000_1111;
                            step = 4 << segment;
                            sampleOut = (sign * (((0b1000_0000) << exponent) + step * mantissa + step / 2 - 4 * 33));
                            leftSamples[i] = toFloatFactor * sampleOut;

                            sampleIn = BitConverter.ToInt16(buffer, 4 * i + 2);
                            sign = sampleIn < 0b1000_0000 ? -1 : 1;
                            mantissa = ~sampleIn;
                            exponent = (mantissa >> 4) & 0b0000_0111;
                            segment = exponent + 1;
                            mantissa &= 0b0000_1111;
                            step = 4 << segment;
                            sampleOut = (sign * (((0b1000_0000) << exponent) + step * mantissa + step / 2 - 4 * 33));
                            rightSamples[i] = toFloatFactor * sampleOut;
                        }
                    }
                    return true;

                case Format.UNDEFINED:
                case Format.A_Law:
                case Format.Extensible:
                    Debug.LogError($"Unsupported Format: {format}");
                    break;

                default:
                    Debug.LogError($"Unrecognized Format: {format}");
                    break;
            }

            leftSamples = null;
            rightSamples = null;
            return false;
        }

        #endregion Data Parsing Methods
        #region Saving Methods

        private delegate bool DataWriter<T>(
            string filepath,
            T data,
            int channels);

        /// <summary>
        /// Save the samples passed in as a WAVE file with the specified filepath.  Returns success.
        /// </summary>
        private static bool WriteFile<T>(
            string filepath,
            T data,
            int channels,
            DataWriter<T> dataWriter,
            bool overwrite = false)
        {
            try
            {
                if (File.Exists(filepath))
                {
                    if (overwrite)
                    {
                        File.Delete(filepath);
                    }
                    else
                    {
                        filepath = IO.FilePath.NextAvailableFilePath(filepath);
                    }
                }

                return dataWriter(filepath, data, channels);
            }
            catch (IOException excp)
            {
                Debug.LogException(excp);
                return false;
            }
        }

        private static bool SimpleWriter(string filepath, float[] samples, int channels)
        {
            using (FileStream fileStream = CreateFile(filepath, samples.Length, channels))
            {
                //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]
                short[] intData = new short[samples.Length];

                //bytesData array is twice the size of dataSource array because Int16 is 2 bytes.
                byte[] bytesData = new byte[samples.Length * 2];

                for (int i = 0; i < samples.Length; i++)
                {
                    intData[i] = (short)(samples[i] * toInt16Factor);
                }
                Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
                fileStream.Write(bytesData, 0, bytesData.Length);
            }

            return true;
        }

        private static bool StreamWriter(string filepath, IBGCStream stream, int channels)
        {
            using (FileStream fileStream = CreateFile(filepath, stream.TotalSamples, channels))
            {
                stream.Reset();
                const int BUFFER_SIZE = 512;
                float[] buffer = new float[BUFFER_SIZE];

                //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]
                short[] intData = new short[BUFFER_SIZE];
                //bytesData array is twice the size of dataSource array because Int16 is 2 bytes.
                byte[] bytesData = new byte[BUFFER_SIZE * 2];

                int readSamples;

                do
                {
                    readSamples = stream.Read(buffer, 0, BUFFER_SIZE);
                    for (int i = 0; i < readSamples; i++)
                    {
                        intData[i] = (short)(buffer[i] * toInt16Factor);
                    }
                    Buffer.BlockCopy(intData, 0, bytesData, 0, 2 * readSamples);
                    fileStream.Write(bytesData, 0, 2 * readSamples);

                }
                while (readSamples == BUFFER_SIZE);
            }
            stream.Reset();

            return true;
        }

        private static bool SimpleWriter(string filepath, short[] samples, int channels)
        {
            using (FileStream fileStream = CreateFile(filepath, samples.Length, channels))
            {
                //bytesData array is twice the size of dataSource array because Int16 is 2 bytes.
                byte[] bytesData = new byte[samples.Length * 2];
                Buffer.BlockCopy(samples, 0, bytesData, 0, bytesData.Length);
                fileStream.Write(bytesData, 0, bytesData.Length);
            }

            return true;
        }

        private static bool StereoWriter(string filepath, (float[] left, float[] right) samples, int channels)
        {
            if (samples.left.Length != samples.right.Length)
            {
                Debug.LogError("Left and Right channels must be the same length");
                return false;
            }

            using (FileStream fileStream = CreateFile(filepath, 2 * samples.left.Length, channels))
            {
                //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]
                short[] intData = new short[2 * samples.left.Length];

                //bytesData array is twice the size of dataSource array because Int16 is 2 bytes.
                byte[] bytesData = new byte[4 * samples.left.Length];

                for (int i = 0; i < samples.left.Length; i++)
                {
                    intData[2 * i] = (short)(samples.left[i] * toInt16Factor);
                    intData[2 * i + 1] = (short)(samples.right[i] * toInt16Factor);
                }
                Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
                fileStream.Write(bytesData, 0, bytesData.Length);
            }

            return true;
        }

        #endregion Saving Methods
    }
}
