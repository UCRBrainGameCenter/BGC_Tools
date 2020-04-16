using System;
using System.IO;
using BGC.IO;
using UnityEngine;

namespace BGC.Users
{
    /// <summary>
    /// Class to handle the global playerdata that is used when a player isn't logged in.
    /// </summary>
    public class GlobalData : ProfileData
    {
        private const string LockedKey = "IsLocked";
        private const string EverUnlockedKey = "EverUnlocked";
        public override bool IsDefault => true;

        /// <summary> Is the device currently in a Locked mode? </summary>
        public bool IsLocked
        {
            get => GetBool(LockedKey, true);
            set
            {
                SetBool(LockedKey, value);
                if (!value)
                {
                    EverUnlocked = true;
                }
            }
        }

        /// <summary> Has the device ever been unlocked? </summary>
        public bool EverUnlocked
        {
            get => GetBool(EverUnlockedKey, false);
            set => SetBool(EverUnlockedKey, value);
        }

        /// <summary> Path of the global datafile </summary>
        protected override string PlayerFilePath => DataManagement.PathForDataFile("System", "GlobalSettings.json");

        public GlobalData()
            : base("Global", "Global")
        {
            if (File.Exists(PlayerFilePath))
            {
                //Load
                Deserialize();
            }
            else
            {
                //Converting old data

                // Global //

                if (PlayerPrefs.HasKey("LockState"))
                {
                    SetBool(LockedKey, PlayerPrefs.GetInt("LockState") == 0);
                }

                if (PlayerPrefs.HasKey("EverUnlocked"))
                {
                    SetBool(EverUnlockedKey, PlayerPrefs.GetInt("EverUnlocked") != 0);
                }

                //HRTF Extraction
                MigrateInt("ImpulseVersion");
                //MR Extraction
                MigrateInt("MRVersion");
                //Data Reupload
                MigrateInt("ReUploadedData");

                // PART //
                //Safety Limit
                MigrateBool("SafetyLimit");
                //Clipping Response
                MigrateInt("ClippingResponse");

                // Listen //
                //Stimulus extraction
                MigrateInt("StimuliVersion");

                // Recollect //
                //Old User Converter Flag
                MigrateInt("MovedOldData");
                //User Reupload
                MigrateInt("UserReupload");

                // PolyRules //
                //Beta Accept
                MigrateBool("betaMessageConfig");

                // Sightseeing //
                //Phototrigger
                MigrateBool("PhotoTrigger");

                // Great Race 2//
                //Sound Extraction
                MigrateInt("SoundsVersion");

                //Create the data
                Serialize();
            }
        }

        private void MigrateInt(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                SetInt(key, PlayerPrefs.GetInt(key));
            }
        }

        private void MigrateBool(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                SetBool(key, PlayerPrefs.GetInt(key) != 0);
            }
        }

        public override void DeletePlayerData()
        {
            throw new InvalidOperationException("Cannot delete Global data file.");
        }
    }
}
