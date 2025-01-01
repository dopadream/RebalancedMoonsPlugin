using LethalLevelLoader;
using System;
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


            if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) && Instance != null) {
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            }
            Instance = this;
            base.OnNetworkSpawn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void InteriorServerRpc()
        {
            if (ModConfig.configMarchDungeons.Value)
                InteriorClientRpc("March");
            if (ModConfig.configDineDungeons.Value)
                InteriorClientRpc("Dine");
            if (ModConfig.configTitanDungeons.Value)
                InteriorClientRpc("Titan");
        }

        [ClientRpc]
        public void InteriorClientRpc(string moonName)
        {
            if (Plugin.rebalancedMoonsMod == null)
                return;

            Plugin.RebalancedMoonsPatches.initInteriors(moonName);
        }

        [ClientRpc]
        public void DeactivateObjectClientRpc(string name)
        {
            foreach (GameObject gameObject in FindObjectsByType<GameObject>(sortMode: FindObjectsSortMode.None).Where(obj => obj.gameObject.name.StartsWith(name)))
            {
                if (gameObject != null)
                {
                    gameObject.SetActive(false);
                }
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
