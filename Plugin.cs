using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace RebalancedMoons
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(WEATHER_REGISTRY, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(LOBBY_COMPATIBILITY, BepInDependency.DependencyFlags.SoftDependency)]

    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; set; }
        public const string PLUGIN_GUID = "dopadream.lethalcompany.rebalancedmoons", PLUGIN_NAME = "RebalancedMoons", PLUGIN_VERSION = "1.7.6", WEATHER_REGISTRY = "mrov.WeatherRegistry", LOBBY_COMPATIBILITY = "BMX.LobbyCompatibility";
        internal static new ManualLogSource Logger;
        internal static ExtendedMod rebalancedMoonsMod;
        internal static SpawnableOutsideObject embrionBoulder1, embrionBoulder2, embrionBoulder3, embrionBoulder4;
        internal static BundledCurve embyBoulderCurve;
        public AssetBundle NetworkBundle, EmbrionBundle;
        static MoldSpreadManager _moldSpreadManager;
        internal static MoldSpreadManager MoldSpreadManager
        {
            get
            {
                if (_moldSpreadManager == null)
                    _moldSpreadManager = UnityEngine.Object.FindAnyObjectByType<MoldSpreadManager>();

                return _moldSpreadManager;
            }
        }

        public Plugin()
        {
            Instance = this;
        }


        void Awake()
        {
            Logger = base.Logger;

            if (Chainloader.PluginInfos.ContainsKey(LOBBY_COMPATIBILITY))
            {
                Plugin.Logger.LogInfo("CROSS-COMPATIBILITY - Lobby Compatibility detected");
                LobbyCompatibility.Init();
            }

            ModConfig.Init(Config);

            var dllFolderPath = Path.GetDirectoryName(Info.Location);

            var networkBundleFilePath = Path.Combine(dllFolderPath, "networkprefab");
            NetworkBundle = AssetBundle.LoadFromFile(networkBundleFilePath);

            var embrionBundleFilePath = Path.Combine(dllFolderPath, "embrionboulders");
            EmbrionBundle = AssetBundle.LoadFromFile(embrionBundleFilePath);

            NetcodePatcher();

            AssetBundleLoader.AddOnExtendedModLoadedListener(OnExtendedModRegistered, "dopadream", "RebalancedMoons");

            Harmony harmony = new Harmony(PLUGIN_GUID);

            harmony.PatchAll(typeof(NetworkObjectManager));
            harmony.PatchAll(typeof(RebalancedMoonsPatches));
            harmony.PatchAll(typeof(MoldBlockerLogic));

            if (Chainloader.PluginInfos.ContainsKey(WEATHER_REGISTRY))
            {
                Plugin.Logger.LogInfo("CROSS-COMPATIBILITY - Weather Registry detected");
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
                    SwapScene(extendedLevel);
                }
            }

            static void SwapScene(ExtendedLevel extendedLevel)
            {
                var planetActions = new Dictionary<string, Action<ExtendedLevel>>
              {
                    { "Experimentation", level => { SetScene(level, "ReExperimentationScene"); } },
                    { "Assurance", level => { SetScene(level, "ReAssuranceScene"); } },
                    { "Offense", level => { SetScene(level, "ReOffenseScene"); } },
                    { "Vow", level => { SetScene(level, "ReVowScene"); } },
                    { "March", level => { SetScene(level, "ReMarchScene"); } },
                    { "Adamance", level => { SetScene(level, "ReAdamanceScene"); } },
                    { "Rend", level => { SetScene(level, "ReRendScene"); } },
                    { "Dine", level => { SetScene(level, "ReDineScene"); } },
                    { "Titan", level => { SetScene(level, "ReTitanScene"); } },
                    { "Artifice", level => { SetScene(level, "ReArtificeScene"); } },
                    { "Embrion", level => { SetScene(level, "ReEmbrionScene"); } }
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
                    { "Experimentation", ModConfig.configExperimentationScene.Value },
                    { "Assurance", ModConfig.configAssuranceScene.Value },
                    { "Offense", ModConfig.configOffenseScene.Value },
                    { "Vow", ModConfig.configVowScene.Value },
                    { "March", ModConfig.configMarchScene.Value },
                    { "Adamance", ModConfig.configAdamanceScene.Value },
                    { "Rend", ModConfig.configRendScene.Value },
                    { "Dine", ModConfig.configDineScene.Value },
                    { "Titan", ModConfig.configTitanScene.Value },
                    { "Embrion", ModConfig.configEmbrionScene.Value },
                    { "Artifice", ModConfig.configArtificeScene.Value }
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
                ModNetworkHandler.Instance.MoonPriceServerRpc();
                ModNetworkHandler.Instance.WeatherServerRpc();
                ModNetworkHandler.Instance.MoonPropertiesServerRpc();
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

                // yeah i know this is weird ok just leaving this method here in case i want to add anything extra 

                InitMoons();
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
                                    { "ReExperimentationScene", "Level1Experimentation"},
                                    { "ReAssuranceScene", "Level2Assurance" },
                                    { "ReVowScene", "Level3Vow" },
                                    { "ReMarchScene", "Level4March" },
                                    { "ReRendScene", "Level5Rend" },
                                    { "ReDineScene", "Level6Dine" },
                                    { "ReOffenseScene", "Level7Offense" },
                                    { "ReTitanScene", "Level8Titan" },
                                    { "ReArtificeScene", "Level9Artifice" },
                                    { "ReAdamanceScene", "Level10Adamance" }
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

            [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SetChallengeFileRandomModifiers))]
            [HarmonyPriority(Priority.First)]
            [HarmonyPrefix]
            static void OnGenerateNewFloorPrefix(RoundManager __instance)
            {
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
                if (ModConfig.configTitanLighting.Value && StartOfRound.Instance.currentLevel.name.Equals("TitanLevel"))
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

                if (ModConfig.configIncreasedFog.Value)
                {
                    VolumeProfile globalProfile = GameObject.Find("/Systems/Rendering/VolumeMain").GetComponent<Volume>().sharedProfile;

                    if (globalProfile != null)
                    {
                        foreach (VolumeComponent component in globalProfile.components)
                        {
                            if (component.GetType() == typeof(Fog))
                            {
                                Fog componentFog = (Fog)component;
                                componentFog.depthExtent.overrideState = true;
                                componentFog.depthExtent.value = 256;
                                Logger.LogDebug("Global volumetric fog distance increased");
                                return;
                            }
                        }
                    }
                    else
                    {
                        Logger.LogDebug("Global volume is null for some reason");
                    }
                }


                [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayerClientRpc))]
                [HarmonyPostfix]
                static void OnKillPlayerClientRpcPostfix(PlayerControllerB __instance)
                {
                    if (ModConfig.configTitanLighting.Value && StartOfRound.Instance.currentLevel.name.Equals("TitanLevel") && (RoundManager.Instance.LevelRandom.Next(0, 2) != 0))
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
            }
        }
    }
}
