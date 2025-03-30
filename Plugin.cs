using BepInEx;
using HarmonyLib;
using UnityEngine;
using BoplFixedMath;
using BepInEx.Configuration;
using System.IO;
using System.Reflection;

namespace death
{
    [BepInPlugin("com.pirre.deathtimer", "death", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        // ConfigEntry to store the seconds value
        private static int timeBeforeSuddenDeathValue;

        private void Awake()
        {
            Logger.LogInfo($"Plugin death is loaded!");

            // Get the directory where the plugin is located
            string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Path to the config file
            string configFilePath = Path.Combine(pluginDirectory, "SetDeathTimer.cfg");

            // Load the config file once during mod load
            if (File.Exists(configFilePath))
            {
                Logger.LogInfo($"Loading configuration from {configFilePath}");
                // Read the value from the config file once
                var configFile = new ConfigFile(configFilePath, true);
                timeBeforeSuddenDeathValue = configFile.Bind<int>(
                    "Settings", 
                    "TimeBeforeSuddenDeath", 
                    60, // Default value if config is invalid
                    "Time before sudden death in seconds"
                ).Value;
            }
            else
            {
                // Use default value if no config file is found
                timeBeforeSuddenDeathValue = 60;
                Logger.LogInfo($"No config file found, using default value of {timeBeforeSuddenDeathValue} seconds.");
            }

            var harmony = new Harmony("com.pirre.deathtimer");
            harmony.PatchAll();
        }

        // Public accessor for the config value
        public static int GetTimeBeforeSuddenDeath()
        {
            return timeBeforeSuddenDeathValue;
        }
    }

    [HarmonyPatch(typeof(GameSessionHandler))]
    public class GameSessionHandlerPatch
    {
        private static FieldInfo timeBeforeSuddenDeathField = AccessTools.Field(typeof(GameSessionHandler), "TimeBeforeSuddenDeath");

        [HarmonyPostfix]
        [HarmonyPatch("UpdateSim")]
        public static void AfterUpdateSim(GameSessionHandler __instance)
        {
            if (timeBeforeSuddenDeathField != null)
            {
                // Get the current value of TimeBeforeSuddenDeath
                var currentValue = (Fix)timeBeforeSuddenDeathField.GetValue(__instance);

                // Check if the value is 120 (default value)
                if (currentValue == new Fix(120))
                {
                    // Get the value from the loaded config
                    int configValue = Plugin.GetTimeBeforeSuddenDeath();

                    // Set it to the config value
                    timeBeforeSuddenDeathField.SetValue(__instance, new Fix(configValue));
                    Debug.Log($"TimeBeforeSuddenDeath has been set to {configValue} seconds!");
                }
            }
        }
    }
}
