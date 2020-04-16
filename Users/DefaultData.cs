using System;
using System.IO;
using LightJson;
using BGC.IO;

namespace BGC.Users
{
    /// <summary>
    /// Class to handle the default playerdata that is used when a player isn't logged in.
    /// </summary>
    public class DefaultData : ProfileData
    {
        public override bool IsDefault => true;

        /// <summary> Path of the default datafile </summary>
        protected override string PlayerFilePath => DataManagement.PathForDataFile("System", "DefaultProfile.json");

        public DefaultData()
            : base("Default", "Default")
        {
            if (File.Exists(PlayerFilePath))
            {
                Deserialize();
            }
            else
            {
                //Handle old Default data
                string oldDefaultDataPath = DataManagement.PathForDataFile("SaveData", "Default.json");

                if (File.Exists(oldDefaultDataPath))
                {
                    FileReader.ReadJsonFile(
                        path: oldDefaultDataPath,
                        //If it is parsable, mark it as successfully loaded
                        successCallback: (JsonObject readData) =>
                        {
                            if (readData.ContainsKey("UserDicts"))
                            {
                                foreach (var data in readData["UserDicts"].AsJsonObject)
                                {
                                    SetJsonValue(data.Key, data.Value);
                                }
                            }
                        });

                    File.Delete(oldDefaultDataPath);
                }

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
