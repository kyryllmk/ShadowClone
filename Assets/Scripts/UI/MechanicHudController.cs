using TMPro;
using UnityEngine;

namespace ShadowClone.UI
{
    public class MechanicHudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusLabel;

        public void ShowReady(float duration)
        {
            SetText($"Ready. R = record ({duration:0.0}s), E = replay, Tab = reset");
        }

        public void ShowRecordingStarted(float duration)
        {
            SetText($"Recording... max {duration:0.0}s");
        }

        public void ShowRecordingFinished(bool success, float duration)
        {
            SetText(success
                ? $"Recording saved ({duration:0.0}s). Press E to replay."
                : "Recording too short. Try another path.");
        }

        public void ShowReplayStarted(float duration)
        {
            SetText($"Replay started ({duration:0.0}s).");
        }

        public void ShowBlocked(string message)
        {
            SetText(message);
        }

        public void ShowReset(string reason)
        {
            SetText($"{reason}. Room reset.");
        }

        public void ShowCompletion(string message)
        {
            SetText(message);
        }

        private void SetText(string value)
        {
            if (statusLabel != null)
            {
                statusLabel.text = value;
            }
        }
    }
}
