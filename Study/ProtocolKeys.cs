namespace BGC.Study
{
    public static class ProtocolKeys
    {
        //Attributes
        public const string Version = "Version";

        //Arrays
        public const string Protocols = "Protocols";
        public const string Sessions = "Sessions";
        public const string SessionElements = "SessionElements";

        //
        //Elements
        //
        public static class Protocol
        {
            //Attributes
            public const string Id = "Id";
            public const string Name = "Name";
            public const string SessionIDs = "Sessions";

            //Dictionary
            public const string EnvironmentValues = "Env";
        }

        public static class Session
        {
            //Attributes
            public const string Id = "Id";
            public const string SessionElementIDs = "Elements";

            //Dictionary
            public const string EnvironmentValues = "Env";
        }

        public static class SessionElement
        {
            //Attributes
            public const string Id = "Id";
            public const string EnvironmentValues = "Env";

            //Dictionary
            public const string ElementType = "Type";
        }
    }
}
