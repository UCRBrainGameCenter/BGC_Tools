using System;

namespace BGC.Study
{
    public static class ProtocolKeys
    {
        //Attributes
        public const string Version = "Version";

        //Arrays
        public const string Protocols = "Protocols";
        public const string Sessions = "Sessions";
        public const string Lockouts = "Lockouts";
        public const string SessionElements = "SessionElements";
        public const string LockoutElements = "LockoutElements";

        //
        //Elements
        //

        public static class SessionElement
        {
            //Attributes
            public const string Id = "Id";
            public const string EnvironmentValues = "Env";

            //Dictionary
            public const string ElementType = "Type";
        }

        public static class LockoutElement
        {
            //Attributes
            public const string Id = "Id";
            public const string EnvironmentValues = "Env";
            
            //Dictionary
            public const string Type = "Type";
            public const string Time = "Time";
            public const string BypassPassword = "BypassPassword";
            public const string Password = "Password";
            public const string WindowTime = "WindowTime";
            public const string MinTime = "MinTime";
            public const string MaxSessions = "MaxSessions";
        }

        public static class SequenceElement
        {
            // Attributes
            public const string Id = "Id";
            public const string Type = "Type";

            // Type values
            public const string SessionType = "Session";
            public const string LockoutType = "Lockout";

            /// <summary>
            /// Determines if a sequence element type can be the final element in a protocol sequence.
            /// This mirrors the CanBeFinalElement property on IProtocolSequenceMember but works with
            /// type strings before instantiation.
            /// </summary>
            public static bool CanTypeBeFinal(string type)
            {
                // Sessions can be final, lockouts cannot (they require a subsequent session to complete)
                return type == SessionType;
            }
        }
    }
}
