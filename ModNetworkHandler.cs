using System;
using Unity.Netcode;

namespace RebalancedMoons
{
    public class ModNetworkHandler : NetworkBehaviour
    {

        public static ModNetworkHandler Instance { get; private set; }

        public override void OnNetworkSpawn()
        {
            LevelEvent = null;
            SendLevelEvent = null;


            if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) && Instance != null) {
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            }
            Instance = this;
            base.OnNetworkSpawn();
        }

        [ClientRpc]
        public void EventClientRpc(string eventName)
        {
            LevelEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

        [ClientRpc]
        public void LevelClientRpc(int extendedLevel, string eventName, string sceneName)
        {
            SendLevelEvent?.Invoke(extendedLevel, eventName, sceneName);
        }

        public static event Action<String> LevelEvent;

        public static event Action<int, String, String> SendLevelEvent;

    }
}
