using TMPro;
using UnityEngine;

namespace ShadowClone.UI
{
    public class MechanicHudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusLabel;

        public void ShowReady(float duration)
        {
            SetText("READY");
        }

        public void ShowRecordingStarted(float duration)
        {
            SetText("RECORDING");
        }

        public void ShowRecordingFinished(bool success, float duration)
        {
            SetText(success ? "RECORDED" : "FAILED");
        }

        public void ShowReplayStarted(float duration)
        {
            SetText("REPLAYING");
        }

        public void ShowBlocked(string message)
        {
            SetText(message);
        }

        public void ShowReset(string reason)
        {
            SetText("RESET");
        }

        public void ShowCompletion(string message)
        {
            SetText(string.IsNullOrWhiteSpace(message) ? "COMPLETE" : message);
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

            TypographyTheme.ApplyState(statusLabel);
            statusLabel.fontSize = 26f;
            statusLabel.alignment = TextAlignmentOptions.TopRight;
            statusLabel.enableWordWrapping = false;

            RectTransform rect = statusLabel.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(280f, 48f);
        }

        private void SetText(string value)
        {
            if (statusLabel != null)
            {
                statusLabel.text = TypographyTheme.NormalizeToken(value);
            }
        }
    }
}
