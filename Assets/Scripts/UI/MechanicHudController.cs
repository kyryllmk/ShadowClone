using TMPro;
using UnityEngine;

namespace ShadowClone.UI
{
    public class MechanicHudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusLabel;

        public void ShowReady(float duration)
        {
            SetText($"Ready to record. Max {duration:0.0}s.");
        }

        public void ShowRecordingStarted(float duration)
        {
            SetText($"Recording... {duration:0.0}s max");
        }

        public void ShowRecordingFinished(bool success, float duration)
        {
            SetText(success
                ? $"Recorded. Press E to replay. ({duration:0.0}s)"
                : "Recording too short. Try again.");
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

        public void Clear()
        {
            SetText(string.Empty);
        }

        public void ConfigureForGameplayStatus()
        {
            if (statusLabel == null)
            {
                return;
            }

            statusLabel.fontSize = 24f;
            statusLabel.alignment = TextAlignmentOptions.TopRight;

            RectTransform rect = statusLabel.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(600f, 120f);
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
