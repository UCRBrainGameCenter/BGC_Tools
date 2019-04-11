using System;

namespace BGC.IO
{
    public class ParsingException : Exception
    {
        public string correctiveAction;

        public ParsingException(string message, string correctiveAction = "")
            : base(message)
        {
            this.correctiveAction = correctiveAction;
        }
    }
}