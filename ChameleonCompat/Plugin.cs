using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace RebalancedMoons.ChameleonCompat
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(CHAMELEON, BepInDependency.DependencyFlags.SoftDependency)]

    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "dopadream.lethalcompany.rebalancedmoons.chameleoncompat", PLUGIN_NAME = "RebalancedMoonsChameleonCompat", PLUGIN_VERSION = "0.0.5", CHAMELEON = "butterystancakes.lethalcompany.chameleon";
        internal static new ManualLogSource Logger;

        void Awake()
        {
            Logger = base.Logger;


            if (Chainloader.PluginInfos.ContainsKey("butterystancakes.lethalcompany.chameleon"))
            {
                Plugin.Logger.LogInfo("CROSS-COMPATIBILITY - Chameleon detected");
                new Harmony(PLUGIN_GUID).PatchAll();
            } else
            {
                Plugin.Logger.LogWarning("CROSS-COMPATIBILITY - Chameleon could not be detected");
                return;
            }

            Logger.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded");
        }
    }

    [HarmonyPatch]  
    internal class ChameleonCompatPatches
    {
        [HarmonyPatch(typeof(Chameleon.Overrides.EntranceDoorFancifier), nameof(Chameleon.Overrides.EntranceDoorFancifier.Apply))]
        [HarmonyPrefix]
        static void OnApplyCosmeticInfoPostfix()
        {
            string levelName = StartOfRound.Instance.currentLevel.name;

            switch (levelName)
            {
                case "DineLevel":
                    Chameleon.Common.currentLevelCosmeticInfo.fancyDoorPos = new(-156.5477f, -15.0669f, 16.7538f);
                    Chameleon.Common.currentLevelCosmeticInfo.fancyDoorRot = Quaternion.Euler(270f, -5.709f, 0f);
                    break;
                case "EmbrionLevel":
                    if (Chainloader.PluginInfos.ContainsKey("MapImprovements"))  
                    {
                        if (GameObject.Find("Embrion_A(Clone)"))
                        {
                            break;
                        }
                    }
                    Chameleon.Common.currentLevelCosmeticInfo.fancyDoorPos = new(-170.063f, 7.176f, -32.569f);
                    Chameleon.Common.currentLevelCosmeticInfo.fancyDoorRot = Quaternion.Euler(270, 220.146805f, 0);
                    break;
                case "ArtificeLevel":
                    Chameleon.Common.currentLevelCosmeticInfo.fancyDoorPos = new(36.552f, 0.021f, -160.742f);
                    Chameleon.Common.currentLevelCosmeticInfo.fancyDoorRot = Quaternion.Euler(-90, 180f, 90.374f);
                    break;
            }
        }
    }
}