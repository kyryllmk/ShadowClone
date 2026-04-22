using ShadowClone.Clone;
using ShadowClone.Core;
using ShadowClone.Gameplay;
using ShadowClone.Level;
using ShadowClone.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowClone.UI
{
    public class GameplayUiBootstrap : MonoBehaviour
    {
        private const string BootstrapObjectName = "GameplayUiBootstrap";
        private static GameplayUiBootstrap instance;

        private RoomResetManager roomResetManager;
        private CloneMechanicController cloneMechanicController;
        private PlayerController playerController;
        private GoalZone goalZone;
        private Hazard[] hazards;

        private Canvas overlayCanvas;
        private TextMeshProUGUI objectiveLabel;
        private GameObject pausePanel;
        private GameObject completionPanel;
        private GameObject deathOverlay;
        private TextMeshProUGUI deathLabel;
        private TextMeshProUGUI completionTitle;
        private TextMeshProUGUI completionBody;
        private Button nextLevelButton;

        private bool isPaused;
        private bool isCompleted;
        private float deathOverlayTimer;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapForGameplayScene()
        {
            if (instance != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject(BootstrapObjectName);
            bootstrapObject.AddComponent<GameplayUiBootstrap>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            BuildOverlay();
            SceneManager.sceneLoaded += HandleSceneLoaded;
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void Update()
        {
            if (deathOverlayTimer > 0f)
            {
                deathOverlayTimer -= Time.unscaledDeltaTime;
                if (deathOverlayTimer <= 0f && deathOverlay != null)
                {
                    deathOverlay.SetActive(false);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) && !isCompleted)
            {
                SetPaused(!isPaused);
            }
        }

        private void OnDestroy()
        {
            ReleaseGoalSubscription();
            ReleaseHazardSubscriptions();
            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                instance = null;
            }

            Time.timeScale = 1f;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureOverlayBuilt();
            ReleaseGoalSubscription();
            ReleaseHazardSubscriptions();

            isPaused = false;
            isCompleted = false;
            deathOverlayTimer = 0f;
            Time.timeScale = 1f;

            if (scene.name == SceneRegistry.MainMenu || scene.name == SceneRegistry.LevelSelect)
            {
                if (overlayCanvas != null)
                {
                    overlayCanvas.enabled = false;
                }
                return;
            }

            overlayCanvas.enabled = true;
            EnsureEventSystem();
            RebindSceneDependencies();
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            if (completionPanel != null)
            {
                completionPanel.SetActive(false);
            }

            if (deathOverlay != null)
            {
                deathOverlay.SetActive(false);
            }

            RefreshControlsPrompt();
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

        private void BuildOverlay()
        {
            overlayCanvas = CreateCanvas();
            objectiveLabel = CreateText("ObjectiveLabel", overlayCanvas.transform, 22, TextAlignmentOptions.TopLeft, role: TypographyRole.Hud);
            RectTransform objectiveRect = objectiveLabel.rectTransform;
            objectiveRect.anchorMin = new Vector2(0f, 1f);
            objectiveRect.anchorMax = new Vector2(0f, 1f);
            objectiveRect.pivot = new Vector2(0f, 1f);
            objectiveRect.anchoredPosition = new Vector2(20f, -20f);
            objectiveRect.sizeDelta = new Vector2(340f, 34f);
            TypographyTheme.ApplyHud(objectiveLabel);
            objectiveLabel.enableWordWrapping = false;

            pausePanel = CreatePanel("PausePanel", overlayCanvas.transform, new Color(0.05f, 0.08f, 0.12f, 0.92f));
            pausePanel.SetActive(false);
            CreateText("PauseTitle", pausePanel.transform, 44, TextAlignmentOptions.Center, "PAUSED", TypographyRole.Title);
            CreateText("PauseBody", pausePanel.transform, 24, TextAlignmentOptions.Center,
                "ESC = RESUME\nTAB = RESET", TypographyRole.Hud);

            CreateButton("ResumeButton", pausePanel.transform, "RESUME", new Vector2(0f, -12f), ResumeGameplay);
            CreateButton("RestartButton", pausePanel.transform, "RESTART", new Vector2(0f, -72f), RestartRoom);
            CreateButton("MainMenuButton", pausePanel.transform, "MENU", new Vector2(0f, -132f), ReturnToMenu);

            completionPanel = CreatePanel("CompletionPanel", overlayCanvas.transform, new Color(0.08f, 0.15f, 0.11f, 0.94f));
            completionPanel.SetActive(false);
            completionTitle = CreateText("CompletionTitle", completionPanel.transform, 42, TextAlignmentOptions.Center, "COMPLETE", TypographyRole.Title);
            completionBody = CreateText("CompletionBody", completionPanel.transform, 24, TextAlignmentOptions.Center,
                "ROOM CLEAR", TypographyRole.Hud);

            nextLevelButton = CreateButton("NextLevelButton", completionPanel.transform, "NEXT", new Vector2(0f, -12f), LoadNextLevel);
            CreateButton("ReplayLevelButton", completionPanel.transform, "REPLAY", new Vector2(0f, -72f), RestartRoom);
            CreateButton("CompletionMenuButton", completionPanel.transform, "MENU", new Vector2(0f, -132f), ReturnToMenu);

            deathOverlay = new GameObject("DeathOverlay");
            deathOverlay.transform.SetParent(overlayCanvas.transform, false);
            RectTransform deathRect = deathOverlay.AddComponent<RectTransform>();
            deathRect.anchorMin = new Vector2(0.5f, 0.5f);
            deathRect.anchorMax = new Vector2(0.5f, 0.5f);
            deathRect.pivot = new Vector2(0.5f, 0.5f);
            deathRect.sizeDelta = new Vector2(420f, 120f);
            deathLabel = CreateText("DeathLabel", deathOverlay.transform, 52, TextAlignmentOptions.Center, "FAILED", TypographyRole.Title);
            RectTransform deathLabelRect = deathLabel.rectTransform;
            deathLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            deathLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            deathLabelRect.pivot = new Vector2(0.5f, 0.5f);
            deathLabelRect.anchoredPosition = Vector2.zero;
            deathLabelRect.sizeDelta = new Vector2(420f, 80f);
            deathLabel.color = new Color(1f, 0.42f, 0.42f, 1f);
            deathOverlay.SetActive(false);
        }

        private void EnsureOverlayBuilt()
        {
            bool needsRebuild =
                overlayCanvas == null ||
                objectiveLabel == null ||
                pausePanel == null ||
                completionPanel == null ||
                deathOverlay == null ||
                deathLabel == null ||
                completionTitle == null ||
                completionBody == null ||
                nextLevelButton == null;

            if (!needsRebuild)
            {
                return;
            }

            if (overlayCanvas != null)
            {
                Destroy(overlayCanvas.gameObject);
            }

            BuildOverlay();
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("GameplayUiOverlay");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private void RefreshControlsPrompt()
        {
            if (objectiveLabel == null)
            {
                return;
            }

            string prompt = SceneManager.GetActiveScene().name switch
            {
                SceneRegistry.Tutorial => "LEARN RECORD + REPLAY",
                SceneRegistry.ButtonDoor => "HOLD GATE. TAKE HIGH ROUTE.",
                SceneRegistry.HazardTiming => "PACE HAZARDS. CLIMB CLEAN.",
                SceneRegistry.Final => "LOCK BOTH GATES. EXTRACT.",
                _ => "RECORD / REPLAY / RESET"
            };

            objectiveLabel.text = TypographyTheme.NormalizeToken(prompt);
        }

        private void SetPaused(bool paused)
        {
            isPaused = paused;
            pausePanel.SetActive(paused);
            ApplyInteractiveState(paused);
            PresentationFeedbackBootstrap.PlayPauseToggle();
        }

        private void ApplyInteractiveState(bool locked)
        {
            Time.timeScale = locked ? 0f : 1f;

            if (playerController != null && !isCompleted)
            {
                playerController.SetMovementLocked(locked);
            }

            if (cloneMechanicController != null)
            {
                cloneMechanicController.enabled = !locked;
            }

            if (roomResetManager != null)
            {
                roomResetManager.enabled = !locked;
            }
        }

        private void ResumeGameplay()
        {
            SetPaused(false);
        }

        private void RestartRoom()
        {
            PresentationFeedbackBootstrap.PlayRestartLevel();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ReturnToMenu()
        {
            PresentationFeedbackBootstrap.PlayMenuConfirm();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneRegistry.MainMenu);
        }

        private void HandleLevelCompleted()
        {
            isCompleted = true;
            isPaused = false;
            pausePanel.SetActive(false);
            ApplyInteractiveState(true);

            bool hasNextLevel = SceneRegistry.TryGetNextCampaignScene(SceneManager.GetActiveScene().name, out string nextSceneName);
            completionPanel.SetActive(true);

            if (hasNextLevel)
            {
                completionTitle.text = "COMPLETE";
                completionBody.text = "ROOM CLEAR";
                nextLevelButton.gameObject.SetActive(true);
            }
            else
            {
                completionTitle.text = "PROTOTYPE COMPLETE";
                completionBody.text = "SIMULATION CLEAR";
                nextLevelButton.gameObject.SetActive(false);
            }
        }

        private void LoadNextLevel()
        {
            if (!SceneRegistry.TryGetNextCampaignScene(SceneManager.GetActiveScene().name, out string nextSceneName))
            {
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneName);
        }

        private GameObject CreatePanel(string name, Transform parent, Color panelColor)
        {
            GameObject panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent, false);

            Image image = panelObject.AddComponent<Image>();
            image.color = panelColor;

            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(760f, 420f);

            return panelObject;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, int fontSize, TextAlignmentOptions alignment, string content = "", TypographyRole role = TypographyRole.Hud)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = TypographyTheme.NormalizeToken(content);
            text.enableWordWrapping = true;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = name.Contains("Title") ? new Vector2(0f, -40f) : new Vector2(0f, -108f);
            rect.sizeDelta = new Vector2(640f, 110f);

            switch (role)
            {
                case TypographyRole.Title:
                    TypographyTheme.ApplyTitle(text);
                    break;
                case TypographyRole.Button:
                    TypographyTheme.ApplyButton(text);
                    break;
                default:
                    TypographyTheme.ApplyHud(text);
                    break;
            }

            return text;
        }

        private Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.35f, 0.45f, 1f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(onClick);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(260f, 44f);

            TextMeshProUGUI buttonText = CreateText($"{name}Label", buttonObject.transform, 24, TextAlignmentOptions.Center, label, TypographyRole.Button);
            RectTransform textRect = buttonText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            return button;
        }

        private void RebindSceneDependencies()
        {
            roomResetManager = FindObjectOfType<RoomResetManager>();
            cloneMechanicController = FindObjectOfType<CloneMechanicController>();
            playerController = FindObjectOfType<PlayerController>();
            goalZone = FindObjectOfType<GoalZone>();
            hazards = FindObjectsOfType<Hazard>();

            if (goalZone != null)
            {
                goalZone.Completed += HandleLevelCompleted;
            }

            if (hazards != null)
            {
                for (int i = 0; i < hazards.Length; i++)
                {
                    if (hazards[i] != null)
                    {
                        hazards[i].Triggered += HandleHazardTriggered;
                    }
                }
            }
        }

        private void ReleaseGoalSubscription()
        {
            if (goalZone != null)
            {
                goalZone.Completed -= HandleLevelCompleted;
                goalZone = null;
            }
        }

        private void ReleaseHazardSubscriptions()
        {
            if (hazards == null)
            {
                return;
            }

            for (int i = 0; i < hazards.Length; i++)
            {
                if (hazards[i] != null)
                {
                    hazards[i].Triggered -= HandleHazardTriggered;
                }
            }

            hazards = null;
        }

        private void HandleHazardTriggered()
        {
            if (deathOverlay == null || isCompleted)
            {
                return;
            }

            deathOverlay.SetActive(true);
            deathOverlayTimer = 0.3f;
        }

        private enum TypographyRole
        {
            Hud,
            Title,
            Button
        }
    }
}
