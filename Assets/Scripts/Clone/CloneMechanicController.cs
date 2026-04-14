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
            if (mechanicHudController != null && recordingController != null)
            {
                mechanicHudController.ShowReady(recordingController.MaxRecordingDuration);
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
                bool capturedClip = recordingController.StopRecording();
                if (mechanicHudController != null)
                {
                    mechanicHudController.ShowRecordingFinished(capturedClip, recordingController.CurrentClip.Duration);
                }

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
                mechanicHudController?.ShowBlocked("Stop recording before replaying.");
                return;
            }

            if (!recordingController.HasRecording)
            {
                mechanicHudController?.ShowBlocked("Record a path before replaying.");
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
                    mechanicHudController.ShowBlocked("Clone prefab missing or clip invalid.");
                }
            }
        }

        public void ResetMechanicState(string reason)
        {
            cloneReplayController?.ClearClone();
            recordingController?.ClearRecording();
            mechanicHudController?.ShowReset(reason);
        }
    }
}
