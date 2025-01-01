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

        [ClientRpc]
        public void InteriorClientRpc(string name)
        {
            if (Plugin.rebalancedMoonsMod == null)
                return;

            foreach (ExtendedLevel level in PatchedContent.VanillaExtendedLevels)
            {
                switch (name)
                {
                    case "March":
                        level.SelectableLevel.dungeonFlowTypes = Plugin.reMarchExtended.SelectableLevel.dungeonFlowTypes;
                        level.SelectableLevel.factorySizeMultiplier = Plugin.reMarchExtended.SelectableLevel.factorySizeMultiplier;
                        break;
                    case "Dine":
                        level.SelectableLevel.dungeonFlowTypes = Plugin.reDineExtended.SelectableLevel.dungeonFlowTypes;
                        break;
                    case "Titan":
                        level.SelectableLevel.dungeonFlowTypes = Plugin.reTitanExtended.SelectableLevel.dungeonFlowTypes;
                        level.SelectableLevel.factorySizeMultiplier = Plugin.reTitanExtended.SelectableLevel.factorySizeMultiplier;
                        break;
                }
            }
        }

        [ClientRpc]
        public void DeactivateBridgeClientRpc()
        {
            foreach (GameObject bridgeObject in FindObjectsOfType<GameObject>().Where(obj => obj.gameObject.name.StartsWith("DangerousBridge")))
            {
                if (bridgeObject != null)
                {
                    bridgeObject.SetActive(false);
                }
            }
        }

        [ClientRpc]
        public void DeactivateTitanFireClientRpc()
        {
            foreach (GameObject fireExitObject in FindObjectsOfType<GameObject>().Where(obj => obj.gameObject.name.StartsWith("FireExitDoorContainerD")))
            {
                if (fireExitObject != null)
                {
                    fireExitObject.SetActive(false);
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
