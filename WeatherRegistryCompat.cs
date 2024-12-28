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
                foreach (AudioSource audioSource in SearchInLatestScene<AudioSource>().Where(aud => aud.gameObject.name == "BrokenLight"))
                {
                    audioSource.mute = true;
                }
            }
        }

        internal static List<T> SearchInLatestScene<T>() where T : UnityEngine.Object
        {
            List<T> returnList = new List<T>();
            foreach (GameObject sceneObject in SceneManager.GetSceneAt(SceneManager.sceneCount - 1).GetRootGameObjects())
                foreach (T component in sceneObject.GetComponentsInChildren<T>())
                    returnList.Add(component);
            return (returnList);
        }
    }
}
