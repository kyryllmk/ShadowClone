using ShadowClone.UI;
using UnityEngine;

namespace ShadowClone.Clone
{
    public class CloneMechanicController : MonoBehaviour
    {
        [SerializeField] private RecordingController recordingController;
        [SerializeField] private CloneReplayController cloneReplayController;
        [SerializeField] private MechanicHudController mechanicHudController;
        [SerializeField] private KeyCode recordKey = KeyCode.R;
        [SerializeField] private KeyCode replayKey = KeyCode.E;

        private void Start()
        {
            mechanicHudController?.ConfigureForGameplayStatus();
            mechanicHudController?.Clear();

            if (recordingController != null)
            {
                recordingController.RecordingFinished += HandleRecordingFinished;
            }
        }

        private void OnDestroy()
        {
            if (recordingController != null)
            {
                recordingController.RecordingFinished -= HandleRecordingFinished;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(recordKey))
            {
                ToggleRecording();
            }

            if (Input.GetKeyDown(replayKey))
            {
                TriggerReplay();
            }
        }

        public void ToggleRecording()
        {
            if (recordingController == null)
            {
                return;
            }

            if (recordingController.IsRecording)
            {
                recordingController.StopRecording();
                return;
            }

            cloneReplayController?.ClearClone();
            bool started = recordingController.BeginRecording();

            if (started && mechanicHudController != null)
            {
                mechanicHudController.ShowRecordingStarted(recordingController.MaxRecordingDuration);
            }
        }

        public void TriggerReplay()
        {
            if (recordingController == null || cloneReplayController == null)
            {
                return;
            }

            if (recordingController.IsRecording)
            {
                mechanicHudController?.ShowBlocked("RECORDING");
                return;
            }

            if (!recordingController.HasRecording)
            {
                mechanicHudController?.ShowBlocked("READY");
                return;
            }

            bool replayStarted = cloneReplayController.StartReplay(recordingController.CurrentClip);
            if (mechanicHudController != null)
            {
                if (replayStarted)
                {
                    mechanicHudController.ShowReplayStarted(recordingController.CurrentClip.Duration);
                }
                else
                {
                    mechanicHudController.ShowBlocked("FAILED");
                }
            }
        }

        private void HandleRecordingFinished(RecordingClip clip)
        {
            if (mechanicHudController == null || clip == null)
            {
                return;
            }

            bool capturedClip = clip.FrameCount > 1;
            mechanicHudController.ShowRecordingFinished(capturedClip, clip.Duration);
        }

        public void ResetMechanicState(string reason)
        {
            cloneReplayController?.ClearClone();
            recordingController?.ClearRecording();
            mechanicHudController?.ShowReset(reason);
        }
    }
}
