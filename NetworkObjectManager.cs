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

        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPostfix]
        public static void Init()
        {
            if (networkPrefab != null)
                return;

            try
            {
                AssetBundle networkObject = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "networkprefab"));
                networkPrefab = networkObject.LoadAsset<GameObject>("RBMNetworkHandler");
            }
            catch
            {
                Plugin.Logger.LogError("Encountered some error loading assets from bundle \"networkprefab\". Did you install the plugin correctly?");
                return;
            }

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
