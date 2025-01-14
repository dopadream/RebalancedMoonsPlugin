using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using GameNetcodeStuff;
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
using UnityEngine.Rendering.HighDefinition;

namespace RebalancedMoons
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(WEATHER_REGISTRY, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; set; }
        public const string PLUGIN_GUID = "dopadream.lethalcompany.rebalancedmoons", PLUGIN_NAME = "RebalancedMoons", PLUGIN_VERSION = "1.5.7", WEATHER_REGISTRY = "mrov.WeatherRegistry";
        internal static new ManualLogSource Logger;
        internal static ExtendedLevel reRendExtended, reDineExtended, reMarchExtended, reOffenseExtended, reAssuranceExtended, reEmbrionExtended, reTitanExtended, reAdamanceExtended;
        internal static ExtendedMod rebalancedMoonsMod;
        internal static VolumeProfile snowyProfile, embyProfile;
        public AssetBundle NetworkBundle;

        public Plugin()
        {
            Instance = this;
        }


        void Awake()
        {
            Logger = base.Logger;

            ModConfig.Init(Config);

            var dllFolderPath = Path.GetDirectoryName(Info.Location);
            var assetBundleFilePath = Path.Combine(dllFolderPath, "networkprefab");
            NetworkBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            NetcodePatcher();

            AssetBundleLoader.AddOnExtendedModLoadedListener(OnExtendedModRegistered, "dopadream", "RebalancedMoons");

            Harmony harmony = new Harmony(PLUGIN_GUID);

            harmony.PatchAll(typeof(NetworkObjectManager));
            harmony.PatchAll(typeof(RebalancedMoonsPatches));

            if (Chainloader.PluginInfos.ContainsKey(WEATHER_REGISTRY))
            {
                harmony.PatchAll(typeof(WeatherRegistryCompat));
            }




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

            input.SelectableLevel.outsideEnemySpawnChanceThroughDay = output.SelectableLevel.outsideEnemySpawnChanceThroughDay;

            input.SelectableLevel.minScrap = output.SelectableLevel.minScrap;
            input.SelectableLevel.maxScrap = output.SelectableLevel.maxScrap;

            input.SelectableLevel.minTotalScrapValue = output.SelectableLevel.minTotalScrapValue;
            input.SelectableLevel.maxTotalScrapValue = output.SelectableLevel.maxTotalScrapValue;

            input.SelectableLevel.spawnableScrap.Clear();
            input.SelectableLevel.spawnableScrap.AddRange(output.SelectableLevel.spawnableScrap);

            ModNetworkHandler.Instance.WeatherServerRpc();

            input.SelectableLevel.spawnableMapObjects = output.SelectableLevel.spawnableMapObjects;
            input.SelectableLevel.spawnableOutsideObjects = output.SelectableLevel.spawnableOutsideObjects;

            input.SelectableLevel.riskLevel = output.SelectableLevel.riskLevel;

            input.SelectableLevel.videoReel = output.SelectableLevel.videoReel;


            Logger.LogDebug("Rebalances applied for " + input + "!");
        }
        internal static void ApplySky()
        {
            if (ModConfig.configSnowySkies.Value || ModConfig.configEmbrionSky.Value)
            {
                foreach (Volume volume in FindObjectsByType<Volume>(FindObjectsSortMode.None))
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

                        if (volume.name.Equals("Sky and Fog Global Volume"))
                        {
                            if (profile.Equals("SnowyProfile") && ModConfig.configSnowySkies.Value)
                            {
                                volume.sharedProfile = snowyProfile;
                            }
                            else if (profile.Equals("EmbyProfile") && ModConfig.configEmbrionSky.Value)
                            {
                                volume.sharedProfile = embyProfile;
                                if (!ModConfig.configAmbientVariety.Value)
                                {
                                    volume.sharedProfile.TryGet(out Fog fog);
                                    if (fog != null)
                                    {
                                        fog.albedo.overrideState = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



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
                    { "March", level => { ApplyRebalance(level, reMarchExtended); SetScene(level, "ReMarchScene"); } },
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
                if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                    return;

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
                        SendLevelToClients(extendedLevel.SelectableLevel.levelID, "RBMSceneEvent", sceneName);
                }
            }

            [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.Start))]
            [HarmonyPostfix]
            static void SubscribeToHandler()
            {
                ModNetworkHandler.SendLevelEvent += RebalancedMoonsPatches.ReceivedLevelFromServer;
                SetupLobby(StartOfRound.Instance);
                ModNetworkHandler.Instance.InteriorServerRpc();
            }

            [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.LeaveGameConfirm))]
            [HarmonyPostfix]
            static void UnsubscribeFromHandler()
            {
                ModNetworkHandler.SendLevelEvent -= RebalancedMoonsPatches.ReceivedLevelFromServer;
            }



            [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnClientConnect))]
            [HarmonyPostfix]
            static void ClientConnectPostFix(StartOfRound __instance)
            {
                SetupLobby(__instance);
            }

            static void SetupLobby(StartOfRound __instance)
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

            public static void SendLevelToClients(int extendedLevel, string eventName, string sceneName)
            {
                if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                    return;

                ModNetworkHandler.Instance.LevelClientRpc(extendedLevel, eventName, sceneName);
            }

            public static void ReceivedLevelFromServer(int extendedLevel, string eventName, string sceneName)
            {
                foreach (ExtendedLevel patchedLevel in PatchedContent.VanillaExtendedLevels)
                {
                    if (patchedLevel.SelectableLevel.levelID.Equals(extendedLevel))
                    {
                        switch (eventName)
                        {
                            case "RBMSceneEvent":
                                patchedLevel.SelectableLevel.sceneName = sceneName;
                                patchedLevel.SceneSelections.Clear();
                                patchedLevel.SceneSelections.Add(new StringWithRarity(sceneName, 100));
                                break;
                            case "VanillaSceneEvent":
                                var vanillaSceneMapping = new Dictionary<string, string>
                                {
                                    { "ReOffenseScene", "Level7Offense" },
                                    { "ReMarchScene", "Level4March" },
                                    { "ReAdamanceScene", "Level10Adamance" },
                                    { "ReDineScene", "Level6Dine" },
                                    { "ReTitanScene", "Level8Titan" }
                                };
                                if (vanillaSceneMapping.TryGetValue(patchedLevel.SelectableLevel.sceneName, out var vanillaScene))
                                {
                                    patchedLevel.SelectableLevel.sceneName = sceneName;
                                    patchedLevel.SceneSelections.Clear();
                                    patchedLevel.SceneSelections.Add(new StringWithRarity(vanillaScene, 100));
                                }
                                break;
                        }
                    }
                }
            }

/*            [HarmonyPatch(typeof(DungeonLoader), nameof(DungeonLoader.PatchFireEscapes))]
            [HarmonyPostfix]
            static void OnPatchFireEscapesPostfix(ref DungeonGenerator dungeonGenerator)
            {
                if (StartOfRound.Instance.currentLevel.PlanetName.Contains("Titan") && !ModConfig.configTitanThirdFireExit.Value && StartOfRound.Instance.currentLevel.sceneName == "ReTitanScene")
                {
                    List<EntranceTeleport> list = (from o in DungeonLoader.GetEntranceTeleports(scene)
                                                   orderby o.entranceId
                                                   select o).ToList();
                    foreach (GlobalPropSettings globalPropSettings in dungeonGenerator.DungeonFlow.GlobalProps)
                    {
                        if (globalPropSettings.ID == 1231)
                        {
                            globalPropSettings.Count = new(2, 2);
                            break;
                        }
                    }
                }
            }*/



            [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SetChallengeFileRandomModifiers))]
            [HarmonyPriority(Priority.First)]
            [HarmonyPrefix]
            static void OnGenerateNewFloorPrefix(RoundManager __instance)
            {
                ModUtil.initSceneOverrides();
                if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                {

                    if (!ModConfig.configTitanThirdFireExit.Value && StartOfRound.Instance.currentLevel != null)
                    {
                        if (StartOfRound.Instance.currentLevel.name.Equals("TitanLevel") && ModConfig.configTitanScene.Value)
                        {
                            Plugin.Logger.LogDebug("Rebalanced Titan loaded, destroying 3rd fire exit...");
                            ModNetworkHandler.Instance.DeactivateObjectClientRpc("Environment/Teleports/FireExitDoorContainerD");
                        }
                    }

                    if (!ModConfig.configMarchBridge.Value && StartOfRound.Instance.currentLevel != null)
                    {
                        if (StartOfRound.Instance.currentLevel.name.Equals("MarchLevel") && ModConfig.configMarchScene.Value)
                        {
                            Plugin.Logger.LogDebug("Rebalanced March loaded, destroying rickety bridge...");
                            ModNetworkHandler.Instance.DeactivateObjectClientRpc("Environment/DangerousBridge");
                        }
                    }
                }
            }


            // titan creepy lights

            [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.RefreshLightsList))]
            [HarmonyPostfix]
            static void RefreshLightsPostFix(ref List<Light> ___allPoweredLights)
            {
                if (ModConfig.configAmbientVariety.Value && StartOfRound.Instance.currentLevel.name.Equals("TitanLevel"))
                {
                    foreach (Light light in ___allPoweredLights)
                    {
                        if (light != null)
                        {
                            light.useColorTemperature = true;
                            light.color = new(0.7F, 0.735F, 0.76F, 1);
                            light.colorTemperature = 6500;
                        }
                    }
                }
            }


            [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayerClientRpc))]
            [HarmonyPostfix]
            static void OnKillPlayerClientRpcPostfix(PlayerControllerB __instance)
            {
                if (ModConfig.configAmbientVariety.Value && StartOfRound.Instance.currentLevel.name.Equals("TitanLevel") && (RoundManager.Instance.LevelRandom.Next(0, 2) != 0))
                {
                    if (__instance.playersManager.connectedPlayersAmount >= 1 && __instance.playersManager.livingPlayers == 1)
                    {
                        RoundManager.Instance.FlickerLights();
                        foreach (Light light in RoundManager.Instance.allPoweredLights)
                        {
                            if (light != null)
                            {
                                light.color = new(0.4F, 0.425F, 0.45F, 1);
                            }
                        }
                    }
                }
            }

            // remove rebalanced moon levels from terminal

            [HarmonyPatch(typeof(TerminalManager), nameof(TerminalManager.GetExtendedLevelGroups))]
            [HarmonyPostfix]
            static void OnGetExtendedLevelGroupsPostfix(ref List<ExtendedLevelGroup> __result)
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