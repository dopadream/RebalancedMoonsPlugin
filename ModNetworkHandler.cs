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
                        planetNames.RemoveAll(p => p.Name.Equals("March"));
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

            /*            foreach (ExtendedLevel level in PatchedContent.ExtendedLevels)
                        {
                            switch (name)
                            {
                                case "March":
                                    if (level.NumberlessPlanetName == name || level.SelectableLevel.sceneName == "ReMarchLevel")
                                    {
                                        for (int i = 0; i < level.SelectableLevel.dungeonFlowTypes.Length; i++)
                                        {
                                            if (level.SelectableLevel.dungeonFlowTypes[i].id == 3)
                                                level.SelectableLevel.dungeonFlowTypes[i].id = 0;

                                        }
                                        level.SelectableLevel.dungeonFlowTypes = level.SelectableLevel.dungeonFlowTypes.Union(Plugin.reMarchExtended.SelectableLevel.dungeonFlowTypes).ToArray();
                                        level.SelectableLevel.factorySizeMultiplier = 1.8f;
                                    }
                                    break;
                                case "Dine":
                                    if (level.NumberlessPlanetName == name || level.SelectableLevel.sceneName == "ReDineScene")
                                    {
                                        for (int i = 0; i < level.SelectableLevel.dungeonFlowTypes.Length; i++)
                                        {
                                            if (level.SelectableLevel.dungeonFlowTypes[i].id == 4)
                                                level.SelectableLevel.dungeonFlowTypes[i].rarity = 50;

                                        }
                                    }
                                    break;
                                case "Titan":
                                    if (level.NumberlessPlanetName == name || level.SelectableLevel.sceneName == "ReTitanScene")
                                    {
                                        for (int i = 0; i < level.SelectableLevel.dungeonFlowTypes.Length; i++)
                                        {
                                            if (level.SelectableLevel.dungeonFlowTypes[i].id == 0)
                                                level.SelectableLevel.dungeonFlowTypes[i].rarity = 115;
                                            if (level.SelectableLevel.dungeonFlowTypes[i].id == 4)
                                                level.SelectableLevel.dungeonFlowTypes[i].rarity = 300;

                                        }
                                        level.SelectableLevel.factorySizeMultiplier = 2;
                                    }
                                    break;
                            }*/
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
