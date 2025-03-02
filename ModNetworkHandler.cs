using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace RebalancedMoons
{
    public class ModNetworkHandler : NetworkBehaviour
    {

        public static ModNetworkHandler Instance { get; private set; }

        public override void OnNetworkSpawn()
        {
            SendLevelEvent = null;


            if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) && Instance != null)
            {
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            }
            Instance = this;
            base.OnNetworkSpawn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void InteriorServerRpc()
        {
            if (ModConfig.configMarchDungeons.Value)
            {
                InteriorClientRpc("March");
            }
            if (ModConfig.configDineDungeons.Value)
            {
                InteriorClientRpc("Dine");
            }
            if (ModConfig.configTitanDungeons.Value)
            {
                InteriorClientRpc("Titan");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void WeatherServerRpc()
        {
            if (ModConfig.configWeatherOverrides.Value)
            {
                WeatherClientRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void MoonPriceServerRpc()
        {
            if (ModConfig.configMoonPriceOverrides.Value)
            {
                MoonPriceClientRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void MoonPropertiesServerRpc()
        {
            if (ModConfig.configEmbrionBoulders.Value)
            {
                MoonPropertiesClientRpc("EmbrionBoulders");
            }

            if (ModConfig.configEmbrionGambling.Value)
            {
                MoonPropertiesClientRpc("EmbrionGambling");
            }

        }


        [ClientRpc]
        public void MoonPriceClientRpc()
        {
            foreach (ExtendedLevel level in PatchedContent.VanillaExtendedLevels)
            {
                switch (level.NumberlessPlanetName)
                {
                    case "Dine":
                        {
                            level.RoutePrice = 650;
                            break;
                        }
                }
            }
        }


        [ClientRpc]
        public void WeatherClientRpc()
        {
            foreach (ExtendedLevel level in PatchedContent.VanillaExtendedLevels)
            {
                switch (level.NumberlessPlanetName)
                {
                    case "Dine":
                        {
                            RandomWeatherWithVariables rainy = level.SelectableLevel.randomWeathers.FirstOrDefault(weather => weather.weatherType == LevelWeatherType.Rainy);
                            if (rainy != null)
                            {
                                rainy.weatherType = LevelWeatherType.Foggy;
                                rainy.weatherVariable = 4;
                                rainy.weatherVariable2 = 12;
                            }
                            break;
                        }
                    case "March":
                        {
                            level.SelectableLevel.overrideWeather = true;
                            level.SelectableLevel.overrideWeatherType = LevelWeatherType.Rainy;
                            break;
                        }
                }
            }
        }

        [ClientRpc]
        public void MoonPropertiesClientRpc(string configProperty)
        {


            foreach (ExtendedLevel level in PatchedContent.VanillaExtendedLevels)
            {
                if (level.NumberlessPlanetName == "Embrion")
                {
                    switch (configProperty)
                    {

                        case ("EmbrionBoulders"):
                            var curveData = LoadCurve(ref Plugin.embyBoulderCurve, "EmbrionRockCurve");
                            if (curveData == null || curveData.curve == null)
                            {
                                Debug.LogError("Failed to load EmbrionRockCurve!");
                                return;
                            }
                            var embyRockCurve = curveData.curve;

                            string[] boulderNames = { "LargeRock1Embrion", "LargeRock2Embrion", "LargeRock3Embrion", "LargeRock4Embrion" };
                            SpawnableOutsideObject[] objectRefs = { Plugin.embrionBoulder1, Plugin.embrionBoulder2, Plugin.embrionBoulder3, Plugin.embrionBoulder4 };
                            var boulders = new List<SpawnableOutsideObjectWithRarity>();

                            for (int i = 0; i < boulderNames.Length; i++)
                            {
                                var spawnableObj = LoadOutsideObject(ref objectRefs[i], boulderNames[i]);
                                if (spawnableObj == null)
                                {
                                    Debug.LogError($"Failed to load object: {boulderNames[i]}");
                                    continue;
                                }

                                var boulder = new SpawnableOutsideObjectWithRarity
                                {
                                    spawnableObject = spawnableObj,
                                    randomAmount = embyRockCurve
                                };
                                boulders.Add(boulder);
                            }

                            level.SelectableLevel.spawnableOutsideObjects = boulders.ToArray();
                            break;
                        case ("EmbrionGambling"):
                            level.SelectableLevel.spawnableScrap.Clear();
                            foreach (var item in StartOfRound.Instance.allItemsList.itemsList)
                            {
                                if (!item.twoHanded && item.isScrap)
                                {

                                    if (item.isDefensiveWeapon && item.itemName != "Stop sign" && item.itemName != "Yield sign")
                                    {
                                        continue;
                                    }

                                    SpawnableItemWithRarity spawnableItem = new();
                                    spawnableItem.spawnableItem = item;
                                    spawnableItem.rarity = 300;
                                    level.SelectableLevel.spawnableScrap.Add(spawnableItem);
                                }
                            }
                            break;
                    }
                }
            }
        }

        public SpawnableOutsideObject LoadOutsideObject(ref SpawnableOutsideObject mapObject, string assetName)
        {
            if (mapObject == null)
            {
                try
                {
                    mapObject = Plugin.Instance.EmbrionBundle.LoadAsset<SpawnableOutsideObject>(assetName);
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Failed to load asset '{assetName}' from bundle 'embrionboulders': {ex.Message}");
                }
            }

            return mapObject;
        }

        public BundledCurve LoadCurve(ref BundledCurve curveObject, string assetName)
        {
            if (curveObject == null)
            {
                try
                {
                    curveObject = Plugin.Instance.EmbrionBundle.LoadAsset<BundledCurve>(assetName);
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Failed to load asset '{assetName}' from bundle 'embrionboulders': {ex.Message}");
                }
            }

            return curveObject;
        }

        [ClientRpc]
        public void InteriorClientRpc(string name)
        {
            if (Plugin.rebalancedMoonsMod == null)
                return;

            foreach (ExtendedDungeonFlow extendedFlow in PatchedContent.ExtendedDungeonFlows)
            {
                var planetNames = extendedFlow.LevelMatchingProperties.planetNames;

                switch (extendedFlow.DungeonFlow.name)
                {
                    case "Level1Flow3Exits":
                        planetNames.RemoveAll(p => p.Name.Equals("March") && name.Equals("March"));
                        break;

                    case "Level1Flow":
                        UpdateMoonInList(planetNames, "Titan", 140, name);
                        AddMoonIfNotInList(planetNames, "March", 300, name);
                        break;

                    case "Level2Flow":
                        UpdateMoonInList(planetNames, "Titan", 40, name);
                        AddMoonIfNotInList(planetNames, "March", 5, name);
                        break;

                    case "Level3Flow":
                        UpdateMoonInList(planetNames, "Dine", 50, name);
                        UpdateMoonInList(planetNames, "Titan", 300, name);
                        AddMoonIfNotInList(planetNames, "March", 190, name);
                        break;
                }
            }


            static void UpdateMoonInList(List<StringWithRarity> planetNames, string planetName, int newRarity, string configEvent)
            {
                foreach (var planet in planetNames)
                {
                    if (planet.Name.Equals(planetName) && planet.Name.Equals(configEvent))
                    {
                        planet.Rarity = newRarity;
                    }
                }
            }

            static void AddMoonIfNotInList(List<StringWithRarity> planetNames, string planetName, int rarity, string configEvent)
            {
                if (!planetNames.Any(p => p.Name.Equals(planetName) && p.Name.Equals(configEvent)))
                {
                    planetNames.Add(new StringWithRarity(planetName, rarity));
                }
            }
        }



        [ClientRpc]
        public void DeactivateObjectClientRpc(string name)
        {
            var gameObject = GameObject.Find(name);

            if (gameObject != null && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }


        [ClientRpc]
        public void LevelClientRpc(int extendedLevel, string eventName, string sceneName)
        {
            SendLevelEvent?.Invoke(extendedLevel, eventName, sceneName);
        }

        public static event Action<int, String, String> SendLevelEvent;

        [ServerRpc(RequireOwnership = false)]
        public void KillWeedServerRpc(Vector3 weedPos)
        {
            KillWeedClientRpc(weedPos);
        }

        [ClientRpc]
        public void KillWeedClientRpc(Vector3 weedPos)
        {
            if (!base.IsOwner)
            {
                Plugin.MoldSpreadManager.DestroyMoldAtPosition(weedPos, playEffect: false);
                Plugin.Logger.LogDebug($"Destroying weed at {weedPos}");
            }
        }
    }
}
