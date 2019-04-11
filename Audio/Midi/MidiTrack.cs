using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using BGC.IO;
using BGC.IO.Extensions;
using BGC.Audio.Midi.Events;
using BGC.Audio.Midi.Synth;
using BGC.Audio.Synthesis;

using HeaderInfo = BGC.Audio.Midi.MidiEncoding.HeaderInfo;
using MidiMessageClass = BGC.Audio.Midi.Events.MidiEvent.MidiMessageClass;
using MidiMetaType = BGC.Audio.Midi.Events.MetaMidiEvent.MidiMetaType;

namespace BGC.Audio.Midi
{
    public class MidiTrack
    {
        public const string TRACK_CHUNK_NAME = "MTrk";

        public readonly List<MidiEvent> events = new List<MidiEvent>();

        private readonly bool retainAll;
        private readonly HeaderInfo headerInfo;

        public string TrackName { get; set; } = "";
        public string InstrumentName { get; set; } = "";
        public short SequenceNumber { get; set; }
        public int Length { get; set; } = int.MaxValue;
        public int Tempo { get; set; } = 0x500000;
        public int StartTime { get; set; } = 0;

        public int SignatureNumerator { get; set; } = 3;
        public int SignatureDenominator { get; set; } = 4;
        public int SignatureClockTicks { get; set; } = 24;
        public int SignatureNoteRate { get; set; } = 8;

        public int SharpFlatCount { get; set; } = 0;
        public bool MajorKey { get; set; } = true;

        public float SamplingRate => 44100f;


        //#region TEST
        //private readonly int _trackNum;
        //private static int trackNum = 0;
        //private static StreamWriter _logger = null;
        //private static StreamWriter Logger => _logger ?? (_logger = CreateNewLogger());

        //private static StreamWriter CreateNewLogger()
        //{
        //    string path = DataManagement.PathForDataFile("Test", $"MidiDump.txt");
        //    return File.AppendText(DataManagement.NextAvailableFilePath(path));
        //}
        //#endregion TEST

        public MidiTrack(
            in HeaderInfo headerInfo,
            short sequenceNumber,
            bool retainAll = true)
        {
            this.headerInfo = headerInfo;
            this.retainAll = retainAll;
            SequenceNumber = sequenceNumber;
        }

        public MidiTrack(
            in HeaderInfo headerInfo,
            short sequenceNumber,
            IEnumerable<MidiEvent> events,
            bool retainAll = true)
            : this(headerInfo, sequenceNumber, retainAll)
        {
            AddRange(events);
        }

        /// <summary> Deserialization Constructor </summary>
        public MidiTrack(
            in HeaderInfo headerInfo,
            short sequenceNumber,
            Stream trackStream,
            bool retainAll = false)
            : this(headerInfo, sequenceNumber, retainAll)
        {
            Deserialize(trackStream);
        }

        private void HandleIntegration(MidiEvent midiEvent, int index)
        {
            if (midiEvent == null)
            {
                return;
            }

            if (index == 0)
            {
                midiEvent.time = midiEvent.deltaTime;
            }
            else
            {
                midiEvent.time = events[index - 1].time + midiEvent.deltaTime;
            }

            switch (midiEvent.MessageClass)
            {
                case MidiMessageClass.SysexEvent:
                case MidiMessageClass.SysCommEvent:
                case MidiMessageClass.SysRTEvent:
                    //Maybe insert the indicated calls?
                    break;

                case MidiMessageClass.MetaEvent:
                    MetaMidiEvent metaEvent = midiEvent as MetaMidiEvent;
                    metaEvent?.Integrate(this);
                    break;

                case MidiMessageClass.ChannelEvent:
                default:
                    //Do nothing
                    break;
            }
        }

        public void Add(MidiEvent midiEvent)
        {
            if (midiEvent == null)
            {
                return;
            }

            if (midiEvent.MessageClass == MidiMessageClass.MAX)
            {
                Debug.Log($"Unrecognized MidiEvent Class {midiEvent.MessageClass}: {midiEvent}");
            }

            if (retainAll || midiEvent.Essential)
            {
                events.Add(midiEvent);
            }

            HandleIntegration(midiEvent, events.Count - 1);
        }

        public void Insert(MidiEvent midiEvent, int time)
        {
            if (retainAll || midiEvent.Essential)
            {
                (int index, int deltaT) = FindTimeIndex(time + midiEvent.deltaTime);

                midiEvent.deltaTime = deltaT;
                events.Insert(index, midiEvent);

                if (index + 1 < events.Count)
                {
                    events[index + 1].deltaTime -= deltaT;
                }

                HandleIntegration(midiEvent, index);
            }
        }

        private (int index, int deltaT) FindTimeIndex(int time)
        {
            int currentTime = 0;
            for (int i = 0; i < events.Count; i++)
            {
                if (currentTime + events[i].deltaTime > time)
                {
                    return (i, time - currentTime);
                }

                currentTime += events[i].deltaTime;
            }

            return (events.Count, time - currentTime);
        }

        public void AddRange(IEnumerable<MidiEvent> midiEvents)
        {
            foreach (MidiEvent midiEvent in midiEvents)
            {
                Add(midiEvent);
            }
        }

        private byte lastEventCode = 0;

        public MidiEvent ParseEvent(Stream inputStream)
        {
            int deltaTime = inputStream.ReadVarQuantity();

            byte nextByte = (byte)inputStream.ReadByte();

            //If the nextByte is an EventCode, update the last event code
            if (nextByte >= 0b1000_0000)
            {
                lastEventCode = nextByte;
                nextByte = (byte)inputStream.ReadByte();
            }

            MidiEvent midiEvent = MidiEvent.ParseEvent(
                inputStream: inputStream,
                deltaTime: deltaTime,
                eventCode: lastEventCode,
                nextByte: nextByte);


            if (midiEvent != null)
            {
                //Logger.WriteLine($"{_trackNum.ToString(),-2} {eventNum.ToString(),-3} {midiEvent.ToString()}");
                if (midiEvent.MessageClass == MidiMessageClass.MAX)
                {
                    midiEvent = null;
                }
            }

            return midiEvent;
        }

        public void Deserialize(Stream trackStream) => AddRange(trackStream.ParseAll(ParseEvent));

        public void Serialize(Stream outputStream)
        {
            int eventLengths = 0;
            byte lastEventCode = 0;

            foreach (MidiEvent midiEvent in events)
            {
                switch (midiEvent.MessageClass)
                {
                    case MidiMessageClass.ChannelEvent:
                        if (midiEvent.eventCode == lastEventCode)
                        {
                            eventLengths += midiEvent.Length - 1;
                        }
                        else
                        {
                            eventLengths += midiEvent.Length;
                        }
                        lastEventCode = midiEvent.eventCode;
                        break;

                    case MidiMessageClass.SysexEvent:
                    case MidiMessageClass.SysCommEvent:
                    case MidiMessageClass.SysRTEvent:
                    case MidiMessageClass.MetaEvent:
                    default:
                        eventLengths += midiEvent.Length;
                        lastEventCode = 0;
                        break;
                }
            }

            outputStream.Write(Encoding.UTF8.GetBytes(TRACK_CHUNK_NAME), 0, 4);
            outputStream.WriteByte((byte)(eventLengths >> 24));
            outputStream.WriteByte((byte)((eventLengths >> 16) & 0b1111_1111));
            outputStream.WriteByte((byte)((eventLengths >> 8) & 0b1111_1111));
            outputStream.WriteByte((byte)(eventLengths & 0b1111_1111));

            foreach (MidiEvent midiEvent in events)
            {
                switch (midiEvent.MessageClass)
                {
                    case MidiMessageClass.ChannelEvent:
                        midiEvent.Serialize(outputStream, midiEvent.eventCode == lastEventCode);
                        lastEventCode = midiEvent.eventCode;
                        break;

                    case MidiMessageClass.SysexEvent:
                    case MidiMessageClass.MetaEvent:
                    case MidiMessageClass.SysCommEvent:
                    case MidiMessageClass.SysRTEvent:
                    default:
                        midiEvent.Serialize(outputStream, false);
                        lastEventCode = 0;
                        break;
                }
            }
        }

        #region Playback

        private double samplesPerTick;

        private float[] sampleBuffer = new float[1];
        private int bufferCount = 0;
        private int bufferIndex = 0;
        private readonly ActiveNoteStream noteStream = new ActiveNoteStream();
        private MidiFileStream midiStream = null;


        private MidiEvent NextEvent => eventIndex < events.Count ? events[eventIndex] : null;

        private int eventIndex = 0;
        private int currentTick;
        private double currentSample = 0.0;

        public void Initialize(MidiFileStream midiStream)
        {
            this.midiStream = midiStream;
        }

        public void SetTempo(int tempo)
        {
            Tempo = tempo;

            samplesPerTick = 1E-6 * Tempo * SamplingRate / headerInfo.ticksPerQuarter;

            int bufferSize = (int)Math.Ceiling(samplesPerTick);

            if (bufferSize > sampleBuffer.Length)
            {
                float[] newSampleBuffer = new float[bufferSize];

                Array.Copy(
                    sourceArray: sampleBuffer,
                    destinationArray: newSampleBuffer,
                    length: sampleBuffer.Length);

                sampleBuffer = newSampleBuffer;
            }
        }

        public int SampleEstimate()
        {
            double tempSamplesPerTick = 1E-6 * Tempo * SamplingRate / headerInfo.ticksPerQuarter;

            return (int)(Length * tempSamplesPerTick);
        }

        public void ExecuteRunningEvent(PitchBendMidiEvent pitchBendEvent)
        {

        }

        public void ExecuteRunningEvent(ChannelPressureMidiEvent channelPressureEvent)
        {

        }

        public void ExecuteRunningEvent(ProgramMidiEvent programEvent)
        {
            midiStream.ExecuteRunningEvent(programEvent);
        }

        public void ExecuteRunningEvent(NoteMidiEvent noteEvent)
        {
            if (noteEvent.noteEventType == NoteMidiEvent.NoteEventType.NoteOff ||
                noteEvent.noteEventType == NoteMidiEvent.NoteEventType.NoteOn && noteEvent.param == 0)
            {
                noteStream.Release(noteEvent.note | noteEvent.channel << 8);
            }
            else
            {
                noteStream.AddStream(
                    key: noteEvent.note | noteEvent.channel << 8,
                    stream: InstrumentLookup.GetNote(
                        noteEvent: noteEvent,
                        set: midiStream.GetChannelProgram(noteEvent.channel)));
            }
        }

        public void ExecuteRunningEvent(ControllerMidiEvent controllerEvent)
        {
            Debug.Log($"Executing ControllerEvent: {controllerEvent}");

            switch (controllerEvent.ModeMessageType)
            {
                case ControllerMidiEvent.ChannelModeMessageTypes.BankSelect_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.BankSelect_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.Modulation_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.Modulation_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.BreathController_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.BreathController_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.FootController_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.FootController_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.PortamentoTime_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.PortamentoTime_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.DataEntry_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.DataEntry_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.ChannelVolume_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.ChannelVolume_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.Balance_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.Balance_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.Pan_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.Pan_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.ExpressionController_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.ExpressionController_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.EffectControl_01_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.EffectControl_01_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.EffectControl_02_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.EffectControl_02_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_01_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_01_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_02_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_02_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_03_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_03_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_04_MSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_04_LSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_05:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_06:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_07:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.GeneralPurposeController_08:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.SustainPedal:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.PortamentoToggle:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.Sostenuto:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.SoftPedal:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.LegatoFootswitch:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.Hold_02:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.SoundController_01:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.SoundController_02:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.SoundController_03:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.SoundController_04:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.SoundController_05:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.SoundController_06:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.SoundController_07:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.SoundController_08:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.SoundController_09:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.SoundController_0A:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.PortamentoControl:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.Effects_01_Depth:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.Effects_02_Depth:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.Effects_03_Depth:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.Effects_04_Depth:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.Effects_05_Depth:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.DataIncrement:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.DataDecrement:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.NonRegisteredParameterNumber_LSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.NonRegisteredParameterNumber_MSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.RegisteredParameterNumber_LSB:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.RegisteredParameterNumber_MSB:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.AllSoundOff:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.ResetAll:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.LocalControl:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.AllNotesOff:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.OmniModeOff:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.OmniModeOn:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.MonoModeOn:
                    break;
                case ControllerMidiEvent.ChannelModeMessageTypes.PolyModeOn:
                    break;

                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_01_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_02_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_03_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_04_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_05_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_06_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_07_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_08_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_09_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0A_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0B_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0C_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0D_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0E_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0F_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_10_MSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_01_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_02_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_03_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_04_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_05_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_06_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_07_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_08_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_09_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0A_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0B_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0C_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0D_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0E_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_0F_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_10_LSB:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_1A:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_1B:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_1C:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_1D:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_1E:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_1F:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_20:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_21:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_22:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_23:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_24:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_25:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_26:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_27:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_28:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_29:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_2A:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_2B:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_2C:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_2D:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_2E:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_2F:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_30:
                case ControllerMidiEvent.ChannelModeMessageTypes.Undefined_31:
                    Debug.LogError($"Undefined MidiChannelEvent: {controllerEvent.ModeMessageType}");
                    return;

                default:
                    Debug.LogError($"Unknown MidiChannelEvent: {controllerEvent.ModeMessageType}");
                    return;
            }
        }

        public int Read(float[] data, int offset, int count)
        {
            //When a read comes in, advance the system to the next whole tick, leaving
            //buffered samples

            int samplesRemaining = count;

            int bodySamplesRead = ReadBuffered(data, offset, count);
            samplesRemaining -= bodySamplesRead;
            offset += bodySamplesRead;

            while (samplesRemaining > 0)
            {
                //Execute any Events at the current tick
                while (NextEvent != null && NextEvent.time == currentTick)
                {
                    NextEvent.ExecuteEvent(this);
                    eventIndex++;
                }

                if (NextEvent == null)
                {
                    break;
                }

                int tickAdvance = Math.Min(
                    (int)Math.Ceiling(samplesRemaining / samplesPerTick),
                    NextEvent.time - currentTick);
                double newSample = currentSample + tickAdvance * samplesPerTick;
                int samplesInTick = (int)(newSample - currentSample);
                int samplesToRead = Math.Min(samplesInTick, samplesRemaining);

                //NoteStream always returns requested samples
                noteStream.Read(data, offset, samplesToRead);

                currentSample = newSample;
                offset += samplesToRead;
                samplesRemaining -= samplesToRead;
                currentTick += tickAdvance;

                //Advance stream to the end of the tick if we still had tick samples left
                if (samplesInTick > samplesToRead)
                {
                    bufferCount = samplesInTick - samplesToRead;

                    noteStream.Read(sampleBuffer, 0, bufferCount);
                    bufferIndex = 0;
                }
            }

            return count - samplesRemaining;
        }

        public int ReadBuffered(float[] data, int offset, int count)
        {
            int samplesWritten = Math.Max(0, Math.Min(count, bufferCount - bufferIndex));

            Array.Copy(
                sourceArray: sampleBuffer,
                sourceIndex: bufferIndex,
                destinationArray: data,
                destinationIndex: offset,
                length: samplesWritten);

            bufferIndex += samplesWritten;

            return samplesWritten;
        }

        public void Reset()
        {
            eventIndex = 0;
            currentTick = 0;
            currentSample = 0.0;
            bufferIndex = 0;
            bufferCount = 0;
            noteStream.Clear();
        }

        public void Seek(int position) => throw new NotSupportedException();

        #endregion Playback
    }
}
