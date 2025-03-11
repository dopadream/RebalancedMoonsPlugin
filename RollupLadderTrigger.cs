using GameNetcodeStuff;
using UnityEngine;

namespace RebalancedMoons
{
    public class RollupLadderTrigger : MonoBehaviour
    {
        public AudioSource thisAudioSource;

        public AudioClip useAudio;

        public void TriggerLadder(PlayerControllerB playerWhoTriggered)
        {
            ModNetworkHandler.Instance.TriggerLadderServerRpc();
            PlayAudio();
        }

        private void PlayAudio()
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
