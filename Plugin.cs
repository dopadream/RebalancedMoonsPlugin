using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace RebalancedMoons
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(WEATHER_REGISTRY, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CHAMELEON, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "dopadream.lethalcompany.rebalancedmoons", PLUGIN_NAME = "RebalancedMoons", PLUGIN_VERSION = "1.4.10", WEATHER_REGISTRY = "mrov.WeatherRegistry", CHAMELEON = "butterystancakes.lethalcompany.chameleon";
        internal static new ManualLogSource Logger;
        internal static ExtendedLevel reRendExtended, reDineExtended, reMarchExtended, reOffenseExtended, reAssuranceExtended, reEmbrionExtended, reTitanExtended, reAdamanceExtended;
        internal static ExtendedMod rebalancedMoonsMod;
        internal static VolumeProfile snowyProfile, embyProfile;


        void Awake()
        {
            Logger = base.Logger;

            ModConfig.Init(Config);

            NetcodePatcher();

            AssetBundleLoader.AddOnExtendedModLoadedListener(OnExtendedModRegistered, "dopadream", "RebalancedMoons");

            Harmony harmony = new Harmony(PLUGIN_GUID);

            harmony.PatchAll(typeof(NetworkObjectManager));
            harmony.PatchAll(typeof(RebalancedMoonsPatches));

            if (Chainloader.PluginInfos.ContainsKey(CHAMELEON))
            {
                harmony.PatchAll(typeof(ChameleonCompat));
            }

            if (Chainloader.PluginInfos.ContainsKey(WEATHER_REGISTRY))
            {
                harmony.PatchAll(typeof(WeatherRegistryCompat));
            }

            SceneManager.sceneLoaded += delegate
            {
                ModNetworkHandler.LevelEvent += RebalancedMoonsPatches.ReceivedEventFromServer;
                ApplySky();
            };




            Logger.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded");

        }

        private static void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
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
            LevelManager.GlobalLevelEvents.onLevelLoaded.AddListener(OnLoadLevel);
        }

        internal static void OnLoadLevel()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (!ModConfig.configTitanThirdFireExit.Value && StartOfRound.Instance.currentLevel != null)
                {
                    if (StartOfRound.Instance.currentLevel.name.Equals("TitanLevel") && ModConfig.configTitanScene.Value)
                    {
                        Plugin.Logger.LogDebug("Rebalanced Titan loaded, destroying 3rd fire exit...");
                        RebalancedMoonsPatches.SendEventToClients("ExitEvent");

                    }
                }

                if (!ModConfig.configMarchBridge.Value && StartOfRound.Instance.currentLevel != null)
                {
                    if (StartOfRound.Instance.currentLevel.name.Equals("MarchLevel") && ModConfig.configMarchScene.Value)
                    {
                        Plugin.Logger.LogDebug("Rebalanced March loaded, destroying rickety bridge...");
                        RebalancedMoonsPatches.SendEventToClients("BridgeEvent");

                    }
                }
            }
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

            input.SelectableLevel.videoReel = output.SelectableLevel.videoReel;


            Logger.LogDebug("Rebalances applied for " + input + "!");
        }

        internal static void ApplySky()
        {
            if (ModConfig.configHDRISkies.Value)
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
        internal class RebalancedMoonsPatches
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
                    { "Offense", ModConfig.configOffenseScene.Value },
                    { "March", ModConfig.configMarchScene.Value },
                    { "Adamance", ModConfig.configAdamanceScene.Value },
                    { "Dine", ModConfig.configDineScene.Value },
                    { "Titan", ModConfig.configTitanScene.Value }
                };

                if (planetSceneMapping.TryGetValue(extendedLevel.NumberlessPlanetName, out bool configValue))
                {
                    if (configValue)
                    {
                        extendedLevel.SceneSelections.Clear();
                        extendedLevel.SceneSelections.Add(new StringWithRarity(sceneName, 100));
                    }
                    else
                    {
                        var vanillaSceneMapping = new Dictionary<string, string>
                        {
                            { "Offense", "Level7Offense" },
                            { "March", "Level4March" },
                            { "Adamance", "Level10Adamance" },
                            { "Dine", "Level6Dine" },
                            { "Titan", "Level8Titan" }
                        };

                        if (vanillaSceneMapping.TryGetValue(extendedLevel.NumberlessPlanetName, out var vanillaScene))
                        {
                            extendedLevel.SceneSelections.Clear();
                            extendedLevel.SceneSelections.Add(new StringWithRarity(vanillaScene, 100));
                        }
                    }
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

            [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
            [HarmonyPostfix]
            static void UnsubscribeFromHandler()
            {
                ModNetworkHandler.LevelEvent -= ReceivedEventFromServer;
            }

            public static void ReceivedEventFromServer(string eventName)
            {
                if (eventName.Equals("ExitEvent"))
                {
                    foreach (GameObject fireExitObject in FindObjectsOfType<GameObject>().Where(obj => obj.gameObject.name.StartsWith("FireExitDoorContainerD")))
                    {
                        if (fireExitObject != null)
                        {
                            fireExitObject.SetActive(false);
                        }
                    }
                }

                if (eventName.Equals("BridgeEvent"))
                {
                    foreach (GameObject bridgeObject in FindObjectsOfType<GameObject>().Where(obj => obj.gameObject.name.StartsWith("DangerousBridge")))
                    {
                        if (bridgeObject != null)
                        {
                            bridgeObject.SetActive(false);

                        }
                    }
                }
                // Event Code Here
            }

            public static void SendEventToClients(string eventName)
            {
                if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                    return;

                ModNetworkHandler.Instance.EventClientRpc(eventName);
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
                if (rebalancedMoonsMod != null)
                {
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
}