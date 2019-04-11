using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    /// <summary>
    /// A Midi event that is sent when a controller other than a key (e.g. a pedal, wheel, etc)
    /// is moved in order to modify the sound of a note (e.g. modulation, sustain, etc)
    /// Midi 1.0 Detailed Specification 4.2 - p.11
    /// </summary>
    public class ControllerMidiEvent : ChannelMidiEvent
    {
        /// <summary>
        /// Documented in Midi 1.0 Detailed Specification 4.2, Table III
        /// </summary>
        public enum ChannelModeMessageTypes
        {
            //
            // MSB of most continuous Controller data
            // [0x00, 0x1F]
            //
            BankSelect_MSB = 0x00,
            Modulation_MSB = 0x01,
            BreathController_MSB = 0x02,
            Undefined_01_MSB = 0x03,
            FootController_MSB = 0x04,
            PortamentoTime_MSB = 0x05,
            DataEntry_MSB = 0x06,
            ChannelVolume_MSB = 0x07,
            Balance_MSB = 0x08,
            Undefined_02_MSB = 0x09,
            Pan_MSB = 0x0A,
            ExpressionController_MSB = 0x0B,
            EffectControl_01_MSB = 0x0C,
            EffectControl_02_MSB = 0x0D,
            Undefined_03_MSB = 0x0E,
            Undefined_04_MSB = 0x0F,
            GeneralPurposeController_01_MSB = 0x10,
            GeneralPurposeController_02_MSB = 0x11,
            GeneralPurposeController_03_MSB = 0x12,
            GeneralPurposeController_04_MSB = 0x13,
            Undefined_05_MSB = 0x14,
            Undefined_06_MSB = 0x15,
            Undefined_07_MSB = 0x16,
            Undefined_08_MSB = 0x17,
            Undefined_09_MSB = 0x18,
            Undefined_0A_MSB = 0x19,
            Undefined_0B_MSB = 0x1A,
            Undefined_0C_MSB = 0x1B,
            Undefined_0D_MSB = 0x1C,
            Undefined_0E_MSB = 0x1D,
            Undefined_0F_MSB = 0x1E,
            Undefined_10_MSB = 0x1F,

            //
            // LSB for controllers 0 through 31
            // [0x20, 0x3F]
            //
            BankSelect_LSB = 0x20,
            Modulation_LSB = 0x21,
            BreathController_LSB = 0x22,
            Undefined_01_LSB = 0x23,
            FootController_LSB = 0x24,
            PortamentoTime_LSB = 0x25,
            DataEntry_LSB = 0x26,
            ChannelVolume_LSB = 0x27,
            Balance_LSB = 0x28,
            Undefined_02_LSB = 0x29,
            Pan_LSB = 0x2A,
            ExpressionController_LSB = 0x2B,
            EffectControl_01_LSB = 0x2C,
            EffectControl_02_LSB = 0x2D,
            Undefined_03_LSB = 0x2E,
            Undefined_04_LSB = 0x2F,
            GeneralPurposeController_01_LSB = 0x30,
            GeneralPurposeController_02_LSB = 0x31,
            GeneralPurposeController_03_LSB = 0x32,
            GeneralPurposeController_04_LSB = 0x33,
            Undefined_05_LSB = 0x34,
            Undefined_06_LSB = 0x35,
            Undefined_07_LSB = 0x36,
            Undefined_08_LSB = 0x37,
            Undefined_09_LSB = 0x38,
            Undefined_0A_LSB = 0x39,
            Undefined_0B_LSB = 0x3A,
            Undefined_0C_LSB = 0x3B,
            Undefined_0D_LSB = 0x3C,
            Undefined_0E_LSB = 0x3D,
            Undefined_0F_LSB = 0x3E,
            Undefined_10_LSB = 0x3F,

            //
            // Single-byte controllers
            // [0x40, 0x5F]
            //
            SustainPedal = 0x40,
            PortamentoToggle = 0x41,
            Sostenuto = 0x42,
            SoftPedal = 0x43,
            LegatoFootswitch = 0x44,
            Hold_02 = 0x45,
            SoundController_01 = 0x46, //Default: SoundVariation
            SoundController_02 = 0x47, //Default: Timbre
            SoundController_03 = 0x48, //Default: ReleaseTime
            SoundController_04 = 0x49, //Default: AttackTime
            SoundController_05 = 0x4A, //Default: Brightness
            SoundController_06 = 0x4B, //Default: None
            SoundController_07 = 0x4C, //Default: None
            SoundController_08 = 0x4D, //Default: None
            SoundController_09 = 0x4E, //Default: None
            SoundController_0A = 0x4F, //Default: None
            GeneralPurposeController_05 = 0x50,
            GeneralPurposeController_06 = 0x51,
            GeneralPurposeController_07 = 0x52,
            GeneralPurposeController_08 = 0x53,
            PortamentoControl = 0x54,
            Undefined_1A = 0x55,
            Undefined_1B = 0x56,
            Undefined_1C = 0x57,
            Undefined_1D = 0x58,
            Undefined_1E = 0x59,
            Undefined_1F = 0x5A,
            Effects_01_Depth = 0x5B, //Formerly: External Effects Depth
            Effects_02_Depth = 0x5C, //Formerly: Tremolo Depth
            Effects_03_Depth = 0x5D, //Formerly: Chorus Depth
            Effects_04_Depth = 0x5E, //Formerly: Detune Depth
            Effects_05_Depth = 0x5F, //Formerly: Phaser Depth

            //
            // Increment/Decrement and Parameter Numbers
            // [0x60, 0x65]
            //
            DataIncrement = 0x60,
            DataDecrement = 0x61,
            NonRegisteredParameterNumber_LSB = 0x62,
            NonRegisteredParameterNumber_MSB = 0x63,
            RegisteredParameterNumber_LSB = 0x64,
            RegisteredParameterNumber_MSB = 0x65,


            //
            // Undefined single-byte controllers
            // [0x66, 0x77]
            //
            Undefined_20 = 0x66,
            Undefined_21 = 0x67,
            Undefined_22 = 0x68,
            Undefined_23 = 0x69,
            Undefined_24 = 0x6A,
            Undefined_25 = 0x6B,
            Undefined_26 = 0x6C,
            Undefined_27 = 0x6D,
            Undefined_28 = 0x6E,
            Undefined_29 = 0x6F,
            Undefined_2A = 0x70,
            Undefined_2B = 0x71,
            Undefined_2C = 0x72,
            Undefined_2D = 0x73,
            Undefined_2E = 0x74,
            Undefined_2F = 0x75,
            Undefined_30 = 0x76,
            Undefined_31 = 0x77,

            //
            // Channel Mode Messages
            // [0x78, 0x7F]
            //
            AllSoundOff = 0x78,
            ResetAll = 0x79,
            LocalControl = 0x7A,
            AllNotesOff = 0x7B,
            OmniModeOff = 0x7C,
            OmniModeOn = 0x7D,
            MonoModeOn = 0x7E,
            PolyModeOn = 0x7F
        }

        /// <summary>
        /// Combination of MSB and LSB from RegisteredParameterNumber message
        /// Documented in Midi 1.0 Detailed Specification 4.2, Table IIIa
        /// </summary>
        public enum RegisteredParameters
        {
            PitchBendSensitivity = 0x0000,
            FineTuning = 0x0001,
            CoarseTuning = 0x0002,
            TuningProgramSelect = 0x0003,
            TuningBankSelect = 0x0004

        }

        public override string EventName => $"Control({ModeMessageType})";
        public override ChannelEventType EventType => ChannelEventType.Controller;
        public override int Length => base.Length + 2;

        public readonly byte channel;
        public readonly byte dataA;
        public readonly byte dataB;

        public ChannelModeMessageTypes ModeMessageType => (ChannelModeMessageTypes)dataA;

        public ControllerMidiEvent(
            int deltaTime,
            byte channel,
            byte controllerNumber,
            byte controllerValue)
            : base(deltaTime, (byte)(0xB0 | channel))
        {
            this.channel = channel;
            dataA = controllerNumber;
            dataB = controllerValue;
        }

        public ControllerMidiEvent(
            int deltaTime,
            ChannelModeMessageTypes channelModeMessageType,
            byte channel,
            byte value = 0x00)
            : base(deltaTime, (byte)(0xB0 | channel))
        {
            this.channel = channel;
            dataA = (byte)channelModeMessageType;
            dataB = value;
        }

        public static ControllerMidiEvent ParseControllerMidiEvent(
            int deltaTime,
            byte typeCode,
            byte dataA,
            byte dataB)
        {
            return new ControllerMidiEvent(deltaTime, (byte)(typeCode & 0b1111), dataA, dataB);
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WriteByte(dataA);
            outputStream.WriteByte(dataB);
        }

        public override string ToString() => $"{base.ToString()} 0x{dataA:X2} 0x{dataB:X2}";

        public override void ExecuteEvent(MidiTrack track) => track.ExecuteRunningEvent(this);
    }
}
