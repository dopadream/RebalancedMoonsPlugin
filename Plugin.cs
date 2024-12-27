using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Chameleon.Info;
using HarmonyLib;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using WeatherRegistry;

namespace RebalancedMoons
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(WEATHER_REGISTRY, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CHAMELEON, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "dopadream.lethalcompany.rebalancedmoons", PLUGIN_NAME = "RebalancedMoons", PLUGIN_VERSION = "1.4.9", WEATHER_REGISTRY = "mrov.WeatherRegistry", CHAMELEON = "butterystancakes.lethalcompany.chameleon";
        internal static new ManualLogSource Logger;
        internal static ExtendedLevel reRendExtended, reDineExtended, reMarchExtended, reOffenseExtended, reAssuranceExtended, reEmbrionExtended, reTitanExtended, reAdamanceExtended;
        internal static ExtendedMod rebalancedMoonsMod;
        internal static VolumeProfile snowyProfile, embyProfile;
        internal static ConfigEntry<bool> configHDRISkies;
        internal static ConfigEntry<bool> configOffenseScene, configAdamanceScene, configMarchScene, configDineScene, configTitanScene;

        void Awake()
        {
            Logger = base.Logger;

            // -client settings-

            configHDRISkies = Config.Bind("Client", "New HDRI Skies", true,
                new ConfigDescription("Adds new HDRI sky volumes to Embrion and snowy moons."));


            // -server settings-

            configOffenseScene = Config.Bind("Server", "Offense Scene Overrides", true,
                new ConfigDescription("Replaces Offense with a new scene using LLL."));

            configAdamanceScene = Config.Bind("Server", "Adamance Scene Overrides", true,
                new ConfigDescription("Replaces Adamance with a new scene using LLL."));

            configMarchScene = Config.Bind("Server", "March Scene Overrides", true,
                new ConfigDescription("Replaces March with a new scene using LLL."));

            configDineScene = Config.Bind("Server", "Dine Scene Overrides", true,
                new ConfigDescription("Replaces Dine with a new scene using LLL."));

            configTitanScene = Config.Bind("Server", "Titan Scene Overrides", true,
                new ConfigDescription("Replaces Titan with a new scene using LLL."));

            // -----------------

            AssetBundleLoader.AddOnExtendedModLoadedListener(OnExtendedModRegistered, "dopadream", "RebalancedMoons");

            new Harmony(PLUGIN_GUID).PatchAll();

            SceneManager.sceneLoaded += delegate
            {
                ApplySky();
            };


            Logger.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded");


        }

        internal static void OnExtendedModRegistered(ExtendedMod extendedMod)
        {
            if (extendedMod == null) return;
            rebalancedMoonsMod = extendedMod;
            reAdamanceExtended = rebalancedMoonsMod.ExtendedLevels[0];
            reAssuranceExtended = rebalancedMoonsMod.ExtendedLevels[1];
            reDineExtended = rebalancedMoonsMod.ExtendedLevels[2];
            reEmbrionExtended = rebalancedMoonsMod.ExtendedLevels[3];
            reMarchExtended = rebalancedMoonsMod.ExtendedLevels[4];
            reOffenseExtended = rebalancedMoonsMod.ExtendedLevels[5];
            reRendExtended = rebalancedMoonsMod.ExtendedLevels[6];
            reTitanExtended = rebalancedMoonsMod.ExtendedLevels[7];

        }

        internal static void ApplyRebalance(ExtendedLevel input, ExtendedLevel output)
        {

            input.RoutePrice = output.RoutePrice;

            input.SelectableLevel.Enemies = output.SelectableLevel.Enemies;
            input.SelectableLevel.OutsideEnemies = output.SelectableLevel.OutsideEnemies;
            input.SelectableLevel.DaytimeEnemies = output.SelectableLevel.DaytimeEnemies;

            input.SelectableLevel.maxEnemyPowerCount = output.SelectableLevel.maxEnemyPowerCount;
            input.SelectableLevel.maxOutsideEnemyPowerCount = output.SelectableLevel.maxOutsideEnemyPowerCount;
            input.SelectableLevel.maxDaytimeEnemyPowerCount = output.SelectableLevel.maxDaytimeEnemyPowerCount;

            input.SelectableLevel.spawnProbabilityRange = output.SelectableLevel.spawnProbabilityRange;
            input.SelectableLevel.enemySpawnChanceThroughoutDay = output.SelectableLevel.enemySpawnChanceThroughoutDay;

            input.SelectableLevel.daytimeEnemiesProbabilityRange = output.SelectableLevel.daytimeEnemiesProbabilityRange;
            input.SelectableLevel.daytimeEnemySpawnChanceThroughDay = output.SelectableLevel.daytimeEnemySpawnChanceThroughDay;

            input.SelectableLevel.daytimeEnemiesProbabilityRange = output.SelectableLevel.daytimeEnemiesProbabilityRange;
            input.SelectableLevel.daytimeEnemySpawnChanceThroughDay = output.SelectableLevel.daytimeEnemySpawnChanceThroughDay;

            input.SelectableLevel.outsideEnemySpawnChanceThroughDay = output.SelectableLevel.outsideEnemySpawnChanceThroughDay;

            input.SelectableLevel.minScrap = output.SelectableLevel.minScrap;
            input.SelectableLevel.maxScrap = output.SelectableLevel.maxScrap;

            input.SelectableLevel.minTotalScrapValue = output.SelectableLevel.minTotalScrapValue;
            input.SelectableLevel.maxTotalScrapValue = output.SelectableLevel.maxTotalScrapValue;

            input.SelectableLevel.spawnableScrap.Clear();
            input.SelectableLevel.spawnableScrap.AddRange(output.SelectableLevel.spawnableScrap);

            input.SelectableLevel.spawnableMapObjects = output.SelectableLevel.spawnableMapObjects;
            input.SelectableLevel.spawnableOutsideObjects = output.SelectableLevel.spawnableOutsideObjects;

            input.SelectableLevel.riskLevel = output.SelectableLevel.riskLevel;

            input.SelectableLevel.sceneName = output.SelectableLevel.sceneName;

            input.SelectableLevel.videoReel = output.SelectableLevel.videoReel;


            Logger.LogDebug("Rebalances applied for " + input + "!");
        }

        internal static void ApplySky()
        {
            if (configHDRISkies.Value)
            {
                foreach (Volume volume in FindObjectsOfType<Volume>())
                {
                    if (snowyProfile == null || embyProfile == null)
                    {
                        if (snowyProfile == null || embyProfile == null)
                        {
                            try
                            {
                                AssetBundle hdriSkies = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "hdri_skies"));
                                snowyProfile = hdriSkies.LoadAsset<VolumeProfile>("ReSnowyFog");
                                embyProfile = hdriSkies.LoadAsset<VolumeProfile>("EmbrionSky");
                            }
                            catch
                            {
                                Plugin.Logger.LogError("Encountered some error loading assets from bundle \"hdri_skies\". Did you install the plugin correctly?");
                                return;
                            }
                        }
                    }

                    if (volume == null || volume.sharedProfile == null)
                    {
                        continue;
                    }

                    string profile = null;

                    if (snowyProfile != null)
                    {
                        if (volume.sharedProfile.name.Contains("SnowyFog") &&
                            StartOfRound.Instance?.currentLevel?.PlanetName?.Contains("Artifice") == false)
                        {
                            profile = "SnowyProfile";
                        }
                    }
                    if (embyProfile != null)
                    {
                        if (StartOfRound.Instance?.currentLevel?.PlanetName?.Contains("Embrion") == true &&
                            volume.sharedProfile.name.Contains("Sky and Fog Settings Profile"))
                        {
                            profile = "EmbyProfile";
                        }
                    }

                    if (!string.IsNullOrEmpty(profile))
                    {
                        Plugin.Logger.LogInfo($"Applying profile '{profile}' to Volume {volume.name}.");

                        if (profile.Equals("SnowyProfile"))
                            volume.sharedProfile = snowyProfile;
                        else if (profile.Equals("EmbyProfile"))
                            volume.sharedProfile = embyProfile;
                    }
                }
            }
        }


        [HarmonyPatch]
        class RebalancedMoonsPatches
        {


            static void InitMoons()
            {
                if (!LethalLevelLoader.Plugin.IsSetupComplete)
                {
                    return;
                }

                foreach (ExtendedLevel extendedLevel in PatchedContent.VanillaExtendedLevels)
                {
                    RebalanceMoon(extendedLevel);
                }
            }

            static void RebalanceMoon(ExtendedLevel extendedLevel)
            {
                var planetActions = new Dictionary<string, Action<ExtendedLevel>>
            {
                { "Assurance", level => ApplyRebalance(level, reAssuranceExtended) },
                { "Offense", level => { ApplyRebalance(level, reOffenseExtended); SetScene(level, "ReOffenseScene"); } },
                { "March", level => { ApplyRebalance(level, reMarchExtended); SetScene(level, "ReMarchLevel"); } },
                { "Adamance", level => { ApplyRebalance(level, reAdamanceExtended); SetScene(level, "ReAdamanceScene"); } },
                { "Rend", level => ApplyRebalance(level, reRendExtended) },
                { "Dine", level => { ApplyRebalance(level, reDineExtended); SetScene(level, "ReDineScene"); } },
                { "Titan", level => { ApplyRebalance(level, reTitanExtended); SetScene(level, "ReTitanScene"); } },
                { "Artifice", level => level.SelectableLevel.riskLevel = "S+" },
                { "Embrion", level => ApplyRebalance(level, reEmbrionExtended) }
            };

                if (planetActions.TryGetValue(extendedLevel.NumberlessPlanetName, out var action))
                {
                    action(extendedLevel);
                }
            }

            static void SetScene(ExtendedLevel extendedLevel, string sceneName)
            {
                var planetSceneMapping = new Dictionary<string, bool>
                {
                    { "Offense", configOffenseScene.Value },
                    { "March", configMarchScene.Value },
                    { "Adamance", configAdamanceScene.Value },
                    { "Dine", configDineScene.Value },
                    { "Titan", configTitanScene.Value }
                };

                if (planetSceneMapping.TryGetValue(extendedLevel.NumberlessPlanetName, out bool configValue) && configValue)
                {
                    extendedLevel.SceneSelections.Clear();
                    extendedLevel.SceneSelections.Add(new StringWithRarity(sceneName, 100));
                }

            }


            [HarmonyPatch(typeof(StartOfRound), "Start")]
            [HarmonyPostfix]
            static void StartOfRoundPostFix(StartOfRound __instance)
            {

                foreach (SelectableLevel level in __instance.levels)
                {
                    switch (level.PlanetName)
                    {
                        case string a when a.Contains("Titan"):
                            level.canSpawnMold = false;
                            break;
                        case string b when b.Contains("Embrion"):
                            level.canSpawnMold = false;
                            break;
                    }
                }

                InitMoons();
                __instance.screenLevelVideoReel.Play();

            }


            [HarmonyPatch(typeof(ExtendedLevel), "SetExtendedDungeonFlowMatches")]
            [HarmonyPostfix]
            [HarmonyPriority(Priority.First)]
            static void ExtendedLevelFlowsPostFix(ExtendedLevel __instance)
            {
                foreach (ExtendedDungeonFlow extendedFlow in PatchedContent.ExtendedDungeonFlows)
                {
                    var planetNames = extendedFlow.LevelMatchingProperties.planetNames;

                    switch (extendedFlow.DungeonFlow.name)
                    {
                        case "Level1Flow3Exits":
                            planetNames.RemoveAll(p => p.Name.Equals("March"));
                            break;

                        case "Level1Flow":
                            UpdateMoonInList(planetNames, "Titan", 140);
                            AddMoonIfNotInList(planetNames, "March", 300);
                            break;

                        case "Level2Flow":
                            UpdateMoonInList(planetNames, "Titan", 40);
                            AddMoonIfNotInList(planetNames, "March", 5);
                            break;

                        case "Level3Flow":
                            UpdateMoonInList(planetNames, "Dine", 50);
                            UpdateMoonInList(planetNames, "Titan", 300);
                            AddMoonIfNotInList(planetNames, "March", 190);
                            break;
                    }
                }
            }


            static void UpdateMoonInList(List<StringWithRarity> planetNames, string planetName, int newRarity)
            {
                foreach (var planet in planetNames)
                {
                    if (planet.Name.Equals(planetName))
                    {
                        planet.Rarity = newRarity;
                    }
                }
            }

            static void AddMoonIfNotInList(List<StringWithRarity> planetNames, string planetName, int rarity)
            {
                if (!planetNames.Any(p => p.Name.Equals(planetName)))
                {
                    planetNames.Add(new StringWithRarity(planetName, rarity));
                }
            }

            [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
            [HarmonyPostfix]
            static void startGamePostfix(RoundManager __instance)
            {
                if (Chainloader.PluginInfos.ContainsKey(WEATHER_REGISTRY))
                {
                    initWeatherTweaksCompat(__instance);
                }
            }

            static void initWeatherTweaksCompat(RoundManager instance)
            {
                if (instance.currentLevel.name.Equals("TitanLevel") && WeatherManager.GetCurrentLevelWeather().Name.Equals("Blackout"))
                {
                    Logger.LogDebug("Titan Blackout detected, turning the evil fire exit off...");
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

            [HarmonyPatch(typeof(Chameleon.SceneOverrides), nameof(Chameleon.SceneOverrides.SetUpFancyEntranceDoors))]
            [HarmonyPrefix]
            static void onApplyCosmeticInfoPostfix(ref LevelCosmeticInfo levelCosmeticInfo)
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

            [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
            [HarmonyPriority(250)]
            [HarmonyPrefix]
            static void onGenerateNewFloorPrefix(RoundManager __instance)
            {
                foreach (ExtendedLevel levels in PatchedContent.ExtendedLevels)
                {
                    switch (levels.NumberlessPlanetName)
                    {
                        case "March":
                            levels.SelectableLevel.factorySizeMultiplier = reMarchExtended.SelectableLevel.factorySizeMultiplier;
                            break;
                        case "Titan":
                            levels.SelectableLevel.factorySizeMultiplier = reTitanExtended.SelectableLevel.factorySizeMultiplier;
                            break;
                    }
                }
            }
             
            [HarmonyPatch(typeof(TerminalManager), nameof(TerminalManager.GetExtendedLevelGroups))]
            [HarmonyPostfix]
            static void onGetExtendedLevelGroupsPostfix(ref List<ExtendedLevelGroup> __result)
            {
                List<ExtendedLevelGroup> newList = new List<ExtendedLevelGroup>();

                foreach (ExtendedLevelGroup group in __result)
                {
                    if (!group.extendedLevelsList.Any(x => rebalancedMoonsMod.ExtendedLevels.Any(y => y == x)))
                    {
                        newList.Add(group);
                    }
                }

                __result = newList;
            }
        }
    }
}