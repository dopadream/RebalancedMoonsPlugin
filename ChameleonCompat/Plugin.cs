using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace RebalancedMoons.ChameleonCompat
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(CHAMELEON, BepInDependency.DependencyFlags.HardDependency)]

    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "dopadream.lethalcompany.rebalancedmoons.chameleoncompat", PLUGIN_NAME = "RebalancedMoonsChameleonCompat", PLUGIN_VERSION = "0.0.2", CHAMELEON = "butterystancakes.lethalcompany.chameleon";
        internal static new ManualLogSource Logger;

        void Awake()
        {
            Logger = base.Logger;

            new Harmony(PLUGIN_GUID).PatchAll();

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
            if (levelName == "DineLevel")
            {
                Chameleon.Common.currentLevelCosmeticInfo.fancyDoorPos = new(-156.5477f, -15.0669f, 16.7538f);
                Chameleon.Common.currentLevelCosmeticInfo.fancyDoorRot = Quaternion.Euler(270f, -5.7088f, 0f);
            }
        }
    }
}