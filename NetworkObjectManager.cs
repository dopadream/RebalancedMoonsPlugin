using HarmonyLib;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace RebalancedMoons
{
    [HarmonyPatch]
    internal class NetworkObjectManager
    {

        static GameObject networkPrefab;

        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
        [HarmonyPostfix]
        public static void Init()
        {
            if (networkPrefab != null)
            {
                return;
            }

            networkPrefab = (GameObject)Plugin.Instance.NetworkBundle.LoadAsset("RBMNetworkHandler");
            networkPrefab.AddComponent<ModNetworkHandler>();

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);

        }


        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandlerHost = Object.Instantiate(NetworkObjectManager.networkPrefab, Vector3.zero, Quaternion.identity);
                Plugin.Logger.LogDebug("Network handler instantiated at " +  networkHandlerHost);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
