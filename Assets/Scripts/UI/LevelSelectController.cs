using System;
using ShadowClone.Core;
using ShadowClone.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowClone.UI
{
    public class LevelSelectController : MonoBehaviour
    {
        [Serializable]
        public class LevelButtonBinding
        {
            public int levelNumber;
            public string sceneName;
            public Button button;
            public TMP_Text levelLabel;
            public TMP_Text statusLabel;
            public TMP_Text metaLabel;
        }

        [Header("Scene Flow")]
        [SerializeField] private string mainMenuSceneName = SceneRegistry.MainMenu;

        [Header("Optional Existing UI")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private Button backButton;
        [SerializeField] private LevelButtonBinding[] levelButtons = Array.Empty<LevelButtonBinding>();

        [Header("Visual State")]
        [SerializeField] private Color unlockedColor = new Color(0.1f, 0.22f, 0.36f, 0.96f);
        [SerializeField] private Color currentColor = new Color(0.16f, 0.46f, 0.68f, 1f);
        [SerializeField] private Color lockedColor = new Color(0.16f, 0.16f, 0.18f, 0.78f);
        [SerializeField] private Color unlockedTextColor = new Color(0.9f, 0.98f, 1f, 1f);
        [SerializeField] private Color lockedTextColor = new Color(0.58f, 0.62f, 0.68f, 1f);

        private void Awake()
        {
            EnsureEventSystem();
            EnsureUiExists();
        }

        private void Start()
        {
            RefreshView();
        }

        public void RefreshView()
        {
            if (titleLabel != null)
            {
                titleLabel.text = "SELECT LEVEL";
                TypographyTheme.ApplyTitle(titleLabel);
                titleLabel.fontSize = 58f;
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(GoBack);
                StyleAuxiliaryButton(backButton, "BACK");
            }

            int highestUnlocked = LevelProgressManager.GetHighestUnlockedLevel();
            int currentPlayable = Mathf.Clamp(highestUnlocked, LevelProgressManager.FirstLevel, LevelProgressManager.MaxLevel);

            foreach (LevelButtonBinding binding in levelButtons)
            {
                if (binding == null || binding.button == null)
                {
                    continue;
                }

                int levelNumber = Mathf.Clamp(binding.levelNumber, LevelProgressManager.FirstLevel, LevelProgressManager.MaxLevel);
                string preferredSceneName = string.IsNullOrWhiteSpace(binding.sceneName)
                    ? LevelProgressManager.GetDefaultSceneNameForLevel(levelNumber)
                    : binding.sceneName;
                string sceneName = LevelProgressManager.ResolvePlayableSceneName(levelNumber, preferredSceneName);

                bool unlocked = LevelProgressManager.IsLevelUnlocked(levelNumber);
                bool isCurrent = unlocked && levelNumber == currentPlayable;

                binding.button.onClick.RemoveAllListeners();
                binding.button.interactable = unlocked;
                if (unlocked)
                {
                    binding.button.onClick.AddListener(() => OpenLevel(sceneName));
                }

                StyleLevelButton(binding, levelNumber, unlocked, isCurrent);
            }
        }

        public void GoBack()
        {
            PresentationFeedbackBootstrap.PlayMenuConfirm();
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void OpenLevel(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            PresentationFeedbackBootstrap.PlayMenuConfirm();
            SceneManager.LoadScene(sceneName);
        }

        private void EnsureUiExists()
        {
            if (rootCanvas == null)
            {
                Canvas[] canvases = FindObjectsOfType<Canvas>(true);
                foreach (Canvas candidate in canvases)
                {
                    if (candidate != null && candidate.gameObject.scene == gameObject.scene)
                    {
                        rootCanvas = candidate;
                        break;
                    }
                }
            }

            if (rootCanvas == null)
            {
                GameObject canvasObject = new GameObject("Canvas");
                rootCanvas = canvasObject.AddComponent<Canvas>();
                rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);

                canvasObject.AddComponent<GraphicRaycaster>();
            }

            if (titleLabel != null && backButton != null && levelButtons != null && levelButtons.Length == LevelProgressManager.MaxLevel)
            {
                return;
            }

            BuildRuntimeLayout();
        }

        private void BuildRuntimeLayout()
        {
            foreach (Transform child in rootCanvas.transform)
            {
                Destroy(child.gameObject);
            }

            GameObject background = CreateImage("Background", rootCanvas.transform, new Color(0.02f, 0.04f, 0.06f, 0.28f));
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            GameObject titleObject = new GameObject("Title");
            titleObject.transform.SetParent(rootCanvas.transform, false);
            titleLabel = titleObject.AddComponent<TextMeshProUGUI>();
            titleLabel.alignment = TextAlignmentOptions.Center;
            titleLabel.color = Color.white;
            RectTransform titleRect = titleLabel.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -76f);
            titleRect.sizeDelta = new Vector2(900f, 96f);

            levelButtons = new LevelButtonBinding[LevelProgressManager.MaxLevel];
            for (int i = 0; i < LevelProgressManager.MaxLevel; i++)
            {
                int levelNumber = i + 1;
                levelButtons[i] = CreateLevelButton(levelNumber, new Vector2(0f, 180f - (i * 120f)));
            }

            backButton = CreateActionButton("BackButton", "BACK", new Vector2(0f, -340f), GoBack);
        }

        private LevelButtonBinding CreateLevelButton(int levelNumber, Vector2 anchoredPosition)
        {
            Button button = CreateActionButton($"Level{levelNumber}Button", $"LEVEL {levelNumber}", anchoredPosition, null);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(420f, 86f);

            TMP_Text title = button.GetComponentInChildren<TMP_Text>(true);
            title.alignment = TextAlignmentOptions.Top;
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -12f);
            titleRect.sizeDelta = new Vector2(340f, 34f);

            TMP_Text status = CreateChildText("StatusLabel", button.transform, new Vector2(0f, -40f), new Vector2(340f, 24f));
            TMP_Text meta = CreateChildText("MetaLabel", button.transform, new Vector2(0f, -62f), new Vector2(340f, 20f));

            return new LevelButtonBinding
            {
                levelNumber = levelNumber,
                sceneName = LevelProgressManager.GetDefaultSceneNameForLevel(levelNumber),
                button = button,
                levelLabel = title,
                statusLabel = status,
                metaLabel = meta
            };
        }

        private Button CreateActionButton(string name, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = CreateImage(name, rootCanvas.transform, unlockedColor);
            Button button = buttonObject.AddComponent<Button>();
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(300f, 68f);

            TMP_Text buttonLabel = CreateChildText("Label", buttonObject.transform, new Vector2(0f, -8f), new Vector2(320f, 34f));
            buttonLabel.text = label;
            TypographyTheme.ApplyButton(buttonLabel);

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            return button;
        }

        private TMP_Text CreateChildText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.color = unlockedTextColor;
            text.alignment = TextAlignmentOptions.Center;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TypographyTheme.ApplyHud(text);
            return text;
        }

        private GameObject CreateImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(name);
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            return imageObject;
        }

        private void StyleAuxiliaryButton(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
                TypographyTheme.ApplyButton(text);
                text.fontSize = 24f;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.09f, 0.16f, 0.24f, 0.94f);
            }
        }

        private void StyleLevelButton(LevelButtonBinding binding, int levelNumber, bool unlocked, bool isCurrent)
        {
            Image image = binding.button.GetComponent<Image>();
            if (image != null)
            {
                image.color = unlocked ? (isCurrent ? currentColor : unlockedColor) : lockedColor;
            }

            if (binding.levelLabel != null)
            {
                binding.levelLabel.text = $"LEVEL {levelNumber}";
                TypographyTheme.ApplyButton(binding.levelLabel);
                binding.levelLabel.color = unlocked ? unlockedTextColor : lockedTextColor;
                binding.levelLabel.fontSize = 28f;
            }

            if (binding.statusLabel != null)
            {
                binding.statusLabel.text = unlocked ? (isCurrent ? "CURRENT UNLOCK" : "UNLOCKED") : "LOCKED";
                TypographyTheme.ApplyHud(binding.statusLabel);
                binding.statusLabel.color = unlocked ? unlockedTextColor : lockedTextColor;
                binding.statusLabel.fontSize = 18f;
            }

            if (binding.metaLabel != null)
            {
                int stars = LevelProgressManager.GetStars(levelNumber);
                int bestScore = LevelProgressManager.GetBestScore(levelNumber);
                binding.metaLabel.text = unlocked
                    ? $"STARS {stars}/3    BEST {bestScore}"
                    : "COMPLETE PREVIOUS LEVEL";
                TypographyTheme.ApplyHud(binding.metaLabel);
                binding.metaLabel.color = unlocked ? new Color(0.72f, 0.88f, 0.98f, 1f) : lockedTextColor;
                binding.metaLabel.fontSize = 16f;
            }
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }
}
