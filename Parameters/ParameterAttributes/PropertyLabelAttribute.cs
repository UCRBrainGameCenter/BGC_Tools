using System;

namespace BGC.Parameters
{
    public abstract class PropertyLabelAttribute : Attribute
    {
        public readonly string title;
        public readonly string serializationString;

        public PropertyLabelAttribute(string title, string serializationString = "")
        {
            if (title == "")
            {
                title = "Default";
            }

            if (serializationString == "")
            {
                serializationString = title;
            }

            if (serializationString == "Type" || serializationString == "Keys")
            {
                throw new Exception("Do not name PropertyChoices either \"Type\" or \"Keys\"");
            }

            this.title = title;
            this.serializationString = serializationString;
        }
    }
}
