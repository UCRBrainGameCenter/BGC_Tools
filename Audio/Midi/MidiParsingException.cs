using System;

namespace BGC.Audio.Midi
{
    public class MidiParsingException : Exception
    {
        public MidiParsingException(string message)
            : base(message)
        {
        }

        public MidiParsingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
