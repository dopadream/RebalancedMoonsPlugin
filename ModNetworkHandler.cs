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


        [ClientRpc]
        public void WeatherClientRpc()
        {
            foreach (ExtendedLevel level in PatchedContent.VanillaExtendedLevels)
            {
                switch (level.NumberlessPlanetName)
                {
                    case "Dine": 
                        {
                            level.SelectableLevel.randomWeathers = Plugin.reDineExtended.SelectableLevel.randomWeathers;
                            break;
                        }
                }
            }
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

    }
}
