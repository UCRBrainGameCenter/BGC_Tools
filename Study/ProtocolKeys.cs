using System;

namespace BGC.Study
{
    public static class ProtocolKeys
    {
        //Attributes
        public const string Version = "Version";
        public const string LastModifiedVersion = "LastModifiedVersion";
        public const string LastCompatibleAppBuildNumber = "LastCompatibleAppBuildNumber";

        //Arrays
        public const string Protocols = "Protocols";
        public const string Sessions = "Sessions";
        public const string SessionElements = "SessionElements";

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
    }
}
