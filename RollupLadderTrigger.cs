using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace RebalancedMoons
{
    public class RollupLadderTrigger : NetworkBehaviour
    {
        public AudioSource thisAudioSource;

        public AudioClip useAudio;

        public void TriggerLadder(PlayerControllerB playerWhoTriggered)
        {
            RBMNetworker.Instance.TriggerLadderServerRpc();
        }

        public void TriggerSound(PlayerControllerB playerWhoTriggered)
        {
            PlayAudioClientRpc();
        }

        [ClientRpc]
        private void PlayAudioClientRpc()
        {

            if (GameNetworkManager.Instance.localPlayerController == null || thisAudioSource == null)
            {
                return;
            }

            AudioClip audioClip = useAudio;

            if (!(audioClip == null))
            {
                thisAudioSource.PlayOneShot(audioClip, 1f);
                WalkieTalkie.TransmitOneShotAudio(thisAudioSource, audioClip);
                RoundManager.Instance.PlayAudibleNoise(thisAudioSource.transform.position, 18f, 0.7f, 0, StartOfRound.Instance.hangarDoorsClosed, 400);
            }
        }
    }
}
