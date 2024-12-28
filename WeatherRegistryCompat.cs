using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeatherRegistry;

namespace RebalancedMoons
{
    [HarmonyPatch]
    internal class WeatherRegistryCompat
    {
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPostfix]
        static void startGamePostfix(RoundManager __instance)
        {
            initWeatherTweaksCompat(__instance);
        }

        static void initWeatherTweaksCompat(RoundManager instance)
        {
            if (instance.currentLevel.name.Equals("TitanLevel") && WeatherManager.GetCurrentLevelWeather().Name.Equals("Blackout"))
            {
                Plugin.Logger.LogDebug("Titan Blackout detected, turning the evil fire exit off...");
                foreach (AudioSource audioSource in ModUtil.SearchInLatestScene<AudioSource>().Where(aud => aud.gameObject.name == "BrokenLight"))
                {
                    audioSource.mute = true;
                }
            }
        }
    }
}
