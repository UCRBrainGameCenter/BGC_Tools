using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Assertions;
using UnityEngine;
using BGC.Web;
using BGC.IO;
using LightJson;

namespace BGC.Utility
{
    /// <summary>
    /// A static cache of registered log-upgraders, as well as convenience methods for managing said.
    /// </summary>
    public static class LogUpgradeUtility
    {
        private static readonly List<LogUpgradeStep> logUpgraders = new List<LogUpgradeStep>();

        public static void Clear()
        {
            if (logUpgraders.Count != 0)
            {
                Debug.LogWarning($"Clearing {logUpgraders.Count} Log Upgraders");
                logUpgraders.Clear();
            }
        }

        public static void RegisterUpgrader(LogUpgradeStep upgrader)
        {
            logUpgraders.Add(upgrader);
        }

        public static BGCRemapHelper UpgradeMetaData(
            string filePath,
            JsonObject metaData)
        {
            BGCRemapHelper remapperHelper = new BGCRemapHelper(filePath);

            foreach (LogUpgradeStep upgrader in logUpgraders)
            {
                upgrader.TestAndApply(
                    metaData: metaData,
                    remapperHelper: remapperHelper);
            }

            if (remapperHelper.IsEmpty())
            {
                return null;
            }

            return remapperHelper;
        }
    }


}