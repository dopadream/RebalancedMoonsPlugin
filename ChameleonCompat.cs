using Chameleon.Info;
using HarmonyLib;
using UnityEngine;

namespace RebalancedMoons
{
    [HarmonyPatch]
    internal class ChameleonCompat
    {
        [HarmonyPatch(typeof(Chameleon.SceneOverrides), nameof(Chameleon.SceneOverrides.SetUpFancyEntranceDoors))]
        [HarmonyPrefix]
        static void OnApplyCosmeticInfoPostfix(ref LevelCosmeticInfo levelCosmeticInfo)
        {
            string levelName = StartOfRound.Instance.currentLevel.name;
            if (levelName == "DineLevel")
            {
                LevelCosmeticInfo newInfo =

                levelCosmeticInfo = new()
                {
                    fancyDoorPos = new(-156.5477f, -15.0669f, 16.7538f),
                    fancyDoorRot = Quaternion.Euler(270f, 174.2912f, 0f),
                    doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND,
                    windowMatName = "FakeWindowView3"
                };

                levelCosmeticInfo = newInfo;
            }
        }
    }
}
