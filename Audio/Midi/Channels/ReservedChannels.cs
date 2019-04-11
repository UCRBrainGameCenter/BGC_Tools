using System;

namespace BGC.Audio.Midi
{
    /// <summary>
    /// Offset to be 0-indexed
    /// </summary>
    public enum ReservedChannels : byte
    {
        Percussion = 9
    }

    /// <summary>
    /// Offset to be 0-indexed
    /// Source:
    ///     Complete MIDI Specs
    ///     General MIDI System Level 1 - Pg 5 - Table 2
    /// </summary>
    public enum ReservedSoundSet : byte
    {
        //Piano 1-8
        AcousticGrandPiano = 0,
        BrightAcousticPiano,
        ElectricGrandPiano,
        HonkyTonkPiano,
        ElectricPiano1,
        ElectricPiano2,
        Harpsichord,
        Clavi,

        //Chromatic Percussion 9-16
        Celesta,
        Glockenspiel,
        MusicBox,
        Vibraphone,
        Marimba,
        Xylophone,
        TubularBells,
        Dulcimer,

        //Organ 17-24
        DrawbarOrgan,
        PercussiveOrgan,
        RockOrgan,
        CrutchOrgan,
        ReedOrgan,
        Accordion,
        Harmonica,
        TangoAccordion,

        //Guitar 25-32
        AcousticGuitar_Nylon,
        AcousticGuitar_Steel,
        ElectricGuitar_Jazz,
        ElectricGuitar_Clean,
        ElectricGuitar_Muted,
        OverdrivenGuitar,
        DistortionGuitar,
        GuitarHarmonics,

        //Bass 33-40
        AcousticBass,
        ElectricBass_Finger,
        ElectricBasS_Pick,
        FretlessBass,
        SlapBass1,
        SlapBass2,
        SynthBass1,
        SynthBass2,

        //Strings 41-48
        Violin,
        Viola,
        Cello,
        Contrabass,
        TremoloStrings,
        PizzicatoStrings,
        OrchestralHarp,
        Timpani,

        //Ensemble 49-56
        StringEnsemble1,
        StringEnsemble2,
        SynthStrings1,
        SynthStrings2,
        ChoirAahs,
        VoiceOohs,
        SynthVoice,
        OrchestraHit,

        //Brass 57-64
        Trumpet,
        Trombone,
        Tuba,
        MutedTrumped,
        FrenchHorn,
        BrassSection,
        SynthBrass1,
        SynthBrass2,

        //Reed 65-72
        SopranoSax,
        AltoSax,
        TenorSax,
        BaritoneSax,
        Oboe,
        EnglishHorn,
        Bassoon,
        Clarinet,

        //Pipe 73-80
        Piccolo,
        Flute,
        Recorder,
        PanFlute,
        BlownBottle,
        Shakuhachi,
        Whistle,
        Ocarina,

        //Synth Lead 81-88
        Lead1_Square,
        Lead2_Sawtooth,
        Lead3_Calliope,
        Lead4_Chiff,
        Lead5_Charang,
        Lead6_Voice,
        Lead7_Fifths,
        Lead8_BassAndLead,

        //Synth Pad 89-96
        Pad1_NewAge,
        Pad2_Warm,
        Pad3_Polysynth,
        Pad4_Choir,
        Pad5_Bowed,
        Pad6_Metallic,
        Pad7_Halo,
        Pad8_Sweep,

        //Synth Effects 97-104
        FX1_Rain,
        FX2_Soundtrack,
        FX3_Crystal,
        FX4_Atmosphere,
        FX5_Brightness,
        FX6_Goblins,
        FX7_Echoes,
        FX8_Scifi,

        //Ethnic 105-112 
        Sitar,
        Banjo,
        Shamisen,
        Koto,
        Kalimba,
        BagPipe,
        Fiddle,
        Shanai,

        //Percussive 113-120
        TinkleBell,
        Agogo,
        SteelDrums,
        Woodblock,
        TaikoDrum,
        MelodicTom,
        SynthDrum,
        ReverseCymbal,
        GuitarFretNoise,

        //Sound Effects 121-128
        BreathNoise,
        Seashore,
        BirdTweet,
        TelephoneRing,
        Helicopter,
        Applause,
        Gunshot
    }

    /// <summary>
    /// Already correctly maps to Frequency
    /// Source:
    ///     Complete MIDI Specs
    ///     General MIDI System Level 1 - Pg 6 - Table 3
    /// </summary>
    public enum PercussionMap
    {
        AcousticBassDrum = 35,
        BassDrum1,
        SideStick,
        AcousticSnare,
        HandClap,
        ElectricSnare,

        LowFloorTom,
        ClosedHiHat,
        HighFloorTom,
        PedalHiHat,
        LowTom,
        OpenHiHat,
        LowMidTom,
        HiMidTom,
        CrashCymbal1,
        HighTom,

        RideCymbal1,
        ChineseCymbal,
        RideBell,
        Tambourine,
        SplashCymbal,
        Cowbell,
        CrashCymbal2,
        Vibraslap,
        RideCymbal2,
        HiBongo,

        LowBongo,
        MuteHiConga,
        OpenHiConga,
        LowConga,
        HighTimbale,
        LowTimbale,
        HighAgogo,
        LowAgogo,
        Cabasa,
        Maracas,

        ShortWhistle,
        LongWhistle,
        ShortGuiro,
        LongGuiro,
        Claves,
        HiWoodBlock,
        LowWoodBlock,
        MuteCuica,
        OpenCuica,
        MuteTriangle,

        OpenTriangle
    }
}
