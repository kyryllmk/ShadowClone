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
        private const float DeathEffectDuration = 0.18f;
        private const float SuccessEffectDuration = 0.28f;
        private static readonly Vector2[] DeathShardBasePositions =
        {
            new Vector2(-180f, 54f),
            new Vector2(152f, -28f),
            new Vector2(-24f, 122f),
            new Vector2(84f, -118f)
        };

        private static GameplayUiBootstrap instance;

        private RoomResetManager roomResetManager;
        private CloneMechanicController cloneMechanicController;
        private PlayerController playerController;
        private GoalZone goalZone;
        private Hazard[] hazards;
        private StarCollectionManager starCollectionManager;

        private Canvas overlayCanvas;
        private TextMeshProUGUI objectiveLabel;
        private TextMeshProUGUI starLabel;
        private GameObject onboardingPanel;
        private TextMeshProUGUI onboardingLabel;
        private GameObject pausePanel;
        private GameObject completionPanel;
        private GameObject deathOverlay;
        private Image deathFlashImage;
        private Image deathBurstImage;
        private Image[] deathShardImages = System.Array.Empty<Image>();
        private Image successFlashImage;
        private TextMeshProUGUI completionTitle;
        private TextMeshProUGUI completionBody;
        private Button nextLevelButton;

        private bool isPaused;
        private bool isCompleted;
        private float deathOverlayTimer;
        private float successOverlayTimer;
        private float onboardingTimer;
        private float starBindRetryTimer;

        public static void ShowOnboardingPrompt(string message, float duration = 3.2f)
        {
            instance?.ShowOnboarding(message, duration);
        }

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
                UpdateDeathEffect();

                if (deathOverlayTimer <= 0f && deathOverlay != null)
                {
                    deathOverlay.SetActive(false);
                }
            }

            if (successOverlayTimer > 0f)
            {
                successOverlayTimer -= Time.unscaledDeltaTime;
                UpdateSuccessEffect();
            }
            else
            {
                UpdateRhythmUiPulse();
            }

            if (onboardingTimer > 0f)
            {
                onboardingTimer -= Time.unscaledDeltaTime;
                UpdateOnboardingPrompt();
            }

            TryBindStarCollectionManager();

            if (Input.GetKeyDown(KeyCode.Escape) && !isCompleted)
            {
                SetPaused(!isPaused);
            }
        }

        private void OnDestroy()
        {
            ReleaseResetSubscription();
            ReleaseGoalSubscription();
            ReleaseHazardSubscriptions();
            ReleaseStarSubscription();
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
            ReleaseResetSubscription();
            ReleaseGoalSubscription();
            ReleaseHazardSubscriptions();
            ReleaseStarSubscription();

            isPaused = false;
            isCompleted = false;
            deathOverlayTimer = 0f;
            successOverlayTimer = 0f;
            onboardingTimer = 0f;
            starBindRetryTimer = 0f;
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

            if (successFlashImage != null)
            {
                successFlashImage.gameObject.SetActive(false);
            }

            if (onboardingPanel != null)
            {
                onboardingPanel.SetActive(false);
            }

            RefreshControlsPrompt();
            RefreshStarCounter(0, 3);
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

            starLabel = CreateText("StarCounterLabel", overlayCanvas.transform, 20, TextAlignmentOptions.TopLeft, "STAR 0/3", TypographyRole.Hud);
            RectTransform starRect = starLabel.rectTransform;
            starRect.anchorMin = new Vector2(0f, 1f);
            starRect.anchorMax = new Vector2(0f, 1f);
            starRect.pivot = new Vector2(0f, 1f);
            starRect.anchoredPosition = new Vector2(20f, -54f);
            starRect.sizeDelta = new Vector2(220f, 30f);
            TypographyTheme.ApplyHud(starLabel);
            starLabel.enableWordWrapping = false;

            onboardingPanel = CreatePanel("OnboardingPanel", overlayCanvas.transform, new Color(0.02f, 0.06f, 0.09f, 0.7f));
            RectTransform onboardingRect = onboardingPanel.GetComponent<RectTransform>();
            onboardingRect.anchorMin = new Vector2(0.5f, 1f);
            onboardingRect.anchorMax = new Vector2(0.5f, 1f);
            onboardingRect.pivot = new Vector2(0.5f, 1f);
            onboardingRect.anchoredPosition = new Vector2(0f, -80f);
            onboardingRect.sizeDelta = new Vector2(760f, 72f);
            onboardingLabel = CreateText("OnboardingLabel", onboardingPanel.transform, 28, TextAlignmentOptions.Center, string.Empty, TypographyRole.Hud);
            RectTransform onboardingLabelRect = onboardingLabel.rectTransform;
            onboardingLabelRect.anchorMin = Vector2.zero;
            onboardingLabelRect.anchorMax = Vector2.one;
            onboardingLabelRect.pivot = new Vector2(0.5f, 0.5f);
            onboardingLabelRect.anchoredPosition = Vector2.zero;
            onboardingLabelRect.sizeDelta = Vector2.zero;
            onboardingPanel.SetActive(false);

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
            deathRect.anchorMin = Vector2.zero;
            deathRect.anchorMax = Vector2.one;
            deathRect.pivot = new Vector2(0.5f, 0.5f);
            deathRect.offsetMin = Vector2.zero;
            deathRect.offsetMax = Vector2.zero;

            deathFlashImage = CreateOverlayImage("DeathWhiteFlash", deathOverlay.transform, Color.white);
            deathBurstImage = CreateOverlayImage("DeathRedBurst", deathOverlay.transform, new Color(1f, 0.05f, 0.02f, 0.85f));
            RectTransform burstRect = deathBurstImage.rectTransform;
            burstRect.anchorMin = new Vector2(0.5f, 0.5f);
            burstRect.anchorMax = new Vector2(0.5f, 0.5f);
            burstRect.pivot = new Vector2(0.5f, 0.5f);
            burstRect.sizeDelta = new Vector2(340f, 340f);
            deathShardImages = new[]
            {
                CreateDeathShard("DeathShardA", DeathShardBasePositions[0], new Vector2(420f, 16f), 14f),
                CreateDeathShard("DeathShardB", DeathShardBasePositions[1], new Vector2(360f, 12f), -10f),
                CreateDeathShard("DeathShardC", DeathShardBasePositions[2], new Vector2(300f, 10f), -23f),
                CreateDeathShard("DeathShardD", DeathShardBasePositions[3], new Vector2(500f, 14f), 7f)
            };
            deathOverlay.SetActive(false);

            successFlashImage = CreateOverlayImage("SuccessFlash", overlayCanvas.transform, new Color(0.64f, 1f, 0.86f, 0f));
            successFlashImage.gameObject.SetActive(false);
        }

        private void EnsureOverlayBuilt()
        {
            bool needsRebuild =
                overlayCanvas == null ||
                objectiveLabel == null ||
                starLabel == null ||
                onboardingPanel == null ||
                onboardingLabel == null ||
                pausePanel == null ||
                completionPanel == null ||
                deathOverlay == null ||
                deathFlashImage == null ||
                deathBurstImage == null ||
                deathShardImages == null ||
                deathShardImages.Length == 0 ||
                successFlashImage == null ||
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

        private Image CreateOverlayImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(name);
            imageObject.transform.SetParent(parent, false);

            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return image;
        }

        private Image CreateDeathShard(string name, Vector2 anchoredPosition, Vector2 size, float rotation)
        {
            Image image = CreateOverlayImage(name, deathOverlay.transform, new Color(1f, 0.12f, 0.08f, 0.55f));
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
            return image;
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
                completionPanel.transform.localScale = Vector3.one * 1.08f;
                successOverlayTimer = SuccessEffectDuration;
                UpdateSuccessEffect();

            if (hasNextLevel)
            {
                completionTitle.text = "COMPLETE";
                completionBody.text = GetCompletionBodyText("ROOM CLEAR");
                nextLevelButton.gameObject.SetActive(true);
            }
            else
            {
                completionTitle.text = "PROTOTYPE COMPLETE";
                completionBody.text = GetCompletionBodyText("SIMULATION CLEAR");
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
            TryBindStarCollectionManager(force: true);

            if (goalZone != null)
            {
                goalZone.Completed += HandleLevelCompleted;
            }

            if (roomResetManager != null)
            {
                roomResetManager.ResetCompleted += HandleResetCompleted;
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

        private void ReleaseResetSubscription()
        {
            if (roomResetManager != null)
            {
                roomResetManager.ResetCompleted -= HandleResetCompleted;
                roomResetManager = null;
            }
        }

        private void TryBindStarCollectionManager(bool force = false)
        {
            if (starCollectionManager != null || !force && starBindRetryTimer > 0f)
            {
                if (!force)
                {
                    starBindRetryTimer -= Time.unscaledDeltaTime;
                }
                return;
            }

            StarCollectionManager manager = StarCollectionManager.Active;
            if (manager == null)
            {
                starBindRetryTimer = 0.2f;
                return;
            }

            starCollectionManager = manager;
            starCollectionManager.StarsChanged += HandleStarsChanged;
            RefreshStarCounter(starCollectionManager.CollectedThisRun, starCollectionManager.TotalStars);
        }

        private void ReleaseStarSubscription()
        {
            if (starCollectionManager != null)
            {
                starCollectionManager.StarsChanged -= HandleStarsChanged;
                starCollectionManager = null;
            }
        }

        private void HandleStarsChanged(int collected, int total)
        {
            RefreshStarCounter(collected, total);
        }

        private void RefreshStarCounter(int collected, int total)
        {
            if (starLabel == null)
            {
                return;
            }

            starLabel.text = $"STARS {Mathf.Clamp(collected, 0, total)}/{Mathf.Max(1, total)}";
        }

        private void HandleHazardTriggered()
        {
            if (deathOverlay == null || isCompleted)
            {
                return;
            }

            deathOverlay.SetActive(true);
            deathOverlayTimer = DeathEffectDuration;
            UpdateDeathEffect();
        }

        private void HandleResetCompleted(string reason)
        {
            if (reason != "Fall reset" && reason != "Out of bounds")
            {
                return;
            }

            HandleHazardTriggered();
        }

        private void UpdateDeathEffect()
        {
            if (deathFlashImage == null || deathBurstImage == null)
            {
                return;
            }

            float progress = 1f - Mathf.Clamp01(deathOverlayTimer / DeathEffectDuration);
            float flashAlpha = Mathf.Lerp(0.72f, 0f, progress);
            float burstAlpha = Mathf.Lerp(0.55f, 0f, progress);

            deathFlashImage.color = new Color(1f, 1f, 1f, flashAlpha);
            deathBurstImage.color = new Color(1f, 0.04f, 0.02f, burstAlpha);
            deathBurstImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.45f, 2.6f, progress);

            if (deathShardImages == null)
            {
                return;
            }

            for (int i = 0; i < deathShardImages.Length; i++)
            {
                Image shard = deathShardImages[i];
                if (shard == null)
                {
                    continue;
                }

                float direction = i % 2 == 0 ? -1f : 1f;
                float alpha = Mathf.Lerp(0.62f, 0f, progress);
                shard.color = new Color(1f, 0.1f, 0.06f, alpha);
                shard.rectTransform.anchoredPosition = DeathShardBasePositions[i] + new Vector2(direction * 22f, (i - 1.5f) * 6f) * progress;
                shard.rectTransform.localScale = new Vector3(Mathf.Lerp(0.72f, 1.18f, progress), 1f, 1f);
            }
        }

        private void UpdateSuccessEffect()
        {
            float progress = 1f - Mathf.Clamp01(successOverlayTimer / SuccessEffectDuration);

            if (successFlashImage != null)
            {
                successFlashImage.gameObject.SetActive(successOverlayTimer > 0f);
                float alpha = Mathf.Lerp(0.48f, 0f, progress);
                successFlashImage.color = new Color(0.62f, 1f, 0.86f, alpha);
            }

            if (completionPanel != null && completionPanel.activeSelf)
            {
                float pulse = Mathf.Sin(progress * Mathf.PI) * 0.08f;
                completionPanel.transform.localScale = Vector3.one * (1f + pulse);
            }
        }

        private void UpdateRhythmUiPulse()
        {
            float beatPulse = PresentationFeedbackBootstrap.BeatPulse01;

            if (objectiveLabel != null && overlayCanvas != null && overlayCanvas.enabled)
            {
                objectiveLabel.rectTransform.localScale = Vector3.one * (1f + beatPulse * 0.018f);
                Color color = objectiveLabel.color;
                color.a = Mathf.Clamp01(0.82f + beatPulse * 0.16f);
                objectiveLabel.color = color;
            }

            if (completionPanel != null && completionPanel.activeSelf)
            {
                completionPanel.transform.localScale = Vector3.one * (1f + beatPulse * 0.026f);
            }
        }

        private void ShowOnboarding(string message, float duration)
        {
            if (onboardingPanel == null || onboardingLabel == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            onboardingLabel.text = TypographyTheme.NormalizeToken(message);
            onboardingTimer = Mathf.Max(0.5f, duration);
            onboardingPanel.SetActive(true);
            UpdateOnboardingPrompt();
        }

        private void UpdateOnboardingPrompt()
        {
            if (onboardingPanel == null || onboardingLabel == null)
            {
                return;
            }

            if (onboardingTimer <= 0f)
            {
                onboardingPanel.SetActive(false);
                return;
            }

            float alpha = Mathf.Clamp01(onboardingTimer / 0.35f);
            Image panelImage = onboardingPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.02f, 0.06f, 0.09f, 0.7f * alpha);
            }

            onboardingLabel.color = new Color(1f, 1f, 1f, alpha);
        }

        private string GetCompletionBodyText(string fallback)
        {
            int collected = starCollectionManager != null ? starCollectionManager.CollectedThisRun : 0;
            int total = starCollectionManager != null ? starCollectionManager.TotalStars : 3;
            return $"{fallback}\nSTARS {collected}/{total}";
        }

        private enum TypographyRole
        {
            Hud,
            Title,
            Button
        }
    }
}
