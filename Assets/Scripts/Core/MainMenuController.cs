using ShadowClone.UI;
using ShadowClone.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ShadowClone.Core
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = SceneRegistry.Tutorial;
        [SerializeField] private string levelSelectSceneName = SceneRegistry.LevelSelect;

        private TMP_Text titleText;
        private Button playButton;
        private Button levelsButton;
        private Button quitButton;
        private float titlePulseTime;

        private void Start()
        {
            CacheMenuReferences();
            EnsureLevelsButton();
            StyleMenuTypography();
            LayoutMenu();
        }

        private void Update()
        {
            titlePulseTime += Time.unscaledDeltaTime;

            if (titleText != null)
            {
                float glow = 0.92f + Mathf.Sin(titlePulseTime * 1.8f) * 0.08f;
                titleText.color = new Color(0.88f * glow, 0.98f * glow, 1f, 1f);
                titleText.rectTransform.localScale = Vector3.one * (1f + Mathf.Sin(titlePulseTime * 1.2f) * 0.015f);
            }

            AnimateButton(playButton, 0f);
            AnimateButton(levelsButton, 0.55f);
            AnimateButton(quitButton, 1.1f);
        }

        public void Play()
        {
            PresentationFeedbackBootstrap.PlayMenuConfirm();
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void OpenLevels()
        {
            if (!Application.CanStreamedLevelBeLoaded(levelSelectSceneName))
            {
                Debug.LogError(
                    $"Level Select scene '{levelSelectSceneName}' is missing or not in Build Settings. " +
                    "Create Assets/Scenes/LevelSelect.unity and add it to File > Build Settings.");
                return;
            }

            PresentationFeedbackBootstrap.PlayMenuConfirm();
            SceneManager.LoadScene(levelSelectSceneName);
        }

        public void Quit()
        {
            PresentationFeedbackBootstrap.PlayMenuConfirm();
#if UNITY_WEBGL
            SceneManager.LoadScene(SceneRegistry.MainMenu);
            return;
#else
            if (Application.isEditor)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }

            Application.Quit();
#endif
        }

        private void CacheMenuReferences()
        {
            TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
            Button[] buttons = FindObjectsOfType<Button>(true);

            foreach (TMP_Text text in texts)
            {
                if (text.text.Contains("Shadow Clone"))
                {
                    titleText = text;
                }
            }

            foreach (Button button in buttons)
            {
                switch (button.name)
                {
                    case "PlayButton":
                        playButton = button;
                        break;
                    case "LevelsButton":
                        levelsButton = button;
                        break;
                    case "QuitButton":
                        quitButton = button;
                        break;
                }
            }
        }

        private void EnsureLevelsButton()
        {
            if (levelsButton != null || playButton == null)
            {
                return;
            }

            GameObject clone = Instantiate(playButton.gameObject, playButton.transform.parent);
            clone.name = "LevelsButton";
            levelsButton = clone.GetComponent<Button>();
            levelsButton.onClick.RemoveAllListeners();
            levelsButton.onClick.AddListener(OpenLevels);

            TMP_Text label = clone.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = "LEVELS";
            }
        }

        private void StyleMenuTypography()
        {
            if (titleText != null)
            {
                titleText.text = "SHADOW CLONE";
                TypographyTheme.ApplyTitle(titleText);
                titleText.fontSize = 78f;
                titleText.alignment = TextAlignmentOptions.Center;
            }

            StyleButton(playButton, "START");
            StyleButton(levelsButton, "LEVELS");
            StyleButton(quitButton, "QUIT");
        }

        private void LayoutMenu()
        {
            if (titleText != null)
            {
                RectTransform titleRect = titleText.rectTransform;
                titleRect.anchorMin = new Vector2(0.5f, 0.5f);
                titleRect.anchorMax = new Vector2(0.5f, 0.5f);
                titleRect.pivot = new Vector2(0.5f, 0.5f);
                titleRect.anchoredPosition = new Vector2(0f, 190f);
                titleRect.sizeDelta = new Vector2(900f, 140f);
            }

            LayoutButton(playButton, 70f);
            LayoutButton(levelsButton, -20f);
            LayoutButton(quitButton, -110f);
        }

        private void LayoutButton(Button button, float y)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(320f, 68f);
        }

        private void StyleButton(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.07f, 0.14f, 0.22f, 0.95f);
                image.type = Image.Type.Sliced;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.12f, 0.22f, 0.34f, 1f);
            colors.highlightedColor = new Color(0.2f, 0.42f, 0.56f, 1f);
            colors.pressedColor = new Color(0.14f, 0.72f, 0.96f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.18f, 0.18f, 0.18f, 0.65f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
                TypographyTheme.ApplyButton(text);
                text.fontSize = 30f;
                text.alignment = TextAlignmentOptions.Center;
                text.color = new Color(0.88f, 0.98f, 1f, 1f);
            }
        }

        private void AnimateButton(Button button, float offset)
        {
            if (button == null)
            {
                return;
            }

            float pulse = 0.94f + (Mathf.Sin(titlePulseTime * 2.4f + offset) * 0.06f);
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.09f * pulse, 0.18f * pulse, 0.3f * pulse, 0.96f);
            }
        }
    }
}
