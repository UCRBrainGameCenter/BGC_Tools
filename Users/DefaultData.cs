using System;
using System.IO;

namespace BGC.Users
{
    /// <summary>
    /// Class to handle the default playerdata that is used when a player isn't logged in.
    /// </summary>
    public class DefaultData : ProfileData
    {
        public override bool IsDefault => true;

        public DefaultData()
            : base("Default")
        {
            if (File.Exists(PlayerFilePath))
            {
                Deserialize();
            }
            else
            {
                //Create the data
                Serialize();
            }
        }

        public override void DeletePlayerData()
        {
            throw new InvalidOperationException("Cannot delete default data files.");
        }
    }
}
