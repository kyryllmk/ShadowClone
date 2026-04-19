using ShadowClone.Clone;
using ShadowClone.Core;
using ShadowClone.Gameplay;
using ShadowClone.Level;
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

        private Canvas overlayCanvas;
        private Text controlsLabel;
        private GameObject pausePanel;
        private GameObject completionPanel;
        private Text completionTitle;
        private Text completionBody;
        private Button nextLevelButton;

        private bool isPaused;
        private bool isCompleted;
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
            if (Input.GetKeyDown(KeyCode.Escape) && !isCompleted)
            {
                SetPaused(!isPaused);
            }
        }

        private void OnDestroy()
        {
            if (goalZone != null)
            {
                goalZone.Completed -= HandleLevelCompleted;
            }

            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                instance = null;
            }

            Time.timeScale = 1f;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ReleaseGoalSubscription();

            isPaused = false;
            isCompleted = false;
            Time.timeScale = 1f;

            if (scene.name == SceneRegistry.MainMenu)
            {
                overlayCanvas.enabled = false;
                return;
            }

            overlayCanvas.enabled = true;
            EnsureEventSystem();
            RebindSceneDependencies();
            pausePanel.SetActive(false);
            completionPanel.SetActive(false);
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
            controlsLabel = CreateText("ControlsLabel", overlayCanvas.transform, 28, TextAnchor.UpperLeft);
            RectTransform controlsRect = controlsLabel.rectTransform;
            controlsRect.anchorMin = new Vector2(0f, 1f);
            controlsRect.anchorMax = new Vector2(0f, 1f);
            controlsRect.pivot = new Vector2(0f, 1f);
            controlsRect.anchoredPosition = new Vector2(24f, -24f);
            controlsRect.sizeDelta = new Vector2(900f, 120f);

            pausePanel = CreatePanel("PausePanel", overlayCanvas.transform, new Color(0.05f, 0.08f, 0.12f, 0.92f));
            pausePanel.SetActive(false);
            CreateText("PauseTitle", pausePanel.transform, 42, TextAnchor.MiddleCenter, "Paused");
            CreateText("PauseBody", pausePanel.transform, 24, TextAnchor.MiddleCenter,
                "Esc to resume\nTab / Backspace resets the room");

            CreateButton("ResumeButton", pausePanel.transform, "Resume", new Vector2(0f, -12f), ResumeGameplay);
            CreateButton("RestartButton", pausePanel.transform, "Restart Room", new Vector2(0f, -72f), RestartRoom);
            CreateButton("MainMenuButton", pausePanel.transform, "Return To Menu", new Vector2(0f, -132f), ReturnToMenu);

            completionPanel = CreatePanel("CompletionPanel", overlayCanvas.transform, new Color(0.08f, 0.15f, 0.11f, 0.94f));
            completionPanel.SetActive(false);
            completionTitle = CreateText("CompletionTitle", completionPanel.transform, 40, TextAnchor.MiddleCenter, "Level Complete");
            completionBody = CreateText("CompletionBody", completionPanel.transform, 24, TextAnchor.MiddleCenter,
                "You solved the room.");

            nextLevelButton = CreateButton("NextLevelButton", completionPanel.transform, "Next Level", new Vector2(0f, -12f), LoadNextLevel);
            CreateButton("ReplayLevelButton", completionPanel.transform, "Replay Level", new Vector2(0f, -72f), RestartRoom);
            CreateButton("CompletionMenuButton", completionPanel.transform, "Return To Menu", new Vector2(0f, -132f), ReturnToMenu);
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
            if (controlsLabel == null)
            {
                return;
            }

            controlsLabel.text = "Move with A / D or arrows. Jump with Space. Press R to record, E to replay, Tab to reset, and Esc to pause.";
        }

        private void SetPaused(bool paused)
        {
            isPaused = paused;
            pausePanel.SetActive(paused);
            ApplyInteractiveState(paused);
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
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ReturnToMenu()
        {
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
                completionTitle.text = "Level Complete";
                completionBody.text = $"Room cleared.\nNext up: {nextSceneName}";
                nextLevelButton.gameObject.SetActive(true);
            }
            else
            {
                completionTitle.text = "Prototype Complete";
                completionBody.text = "You reached the end of the current Shadow Clone campaign slice.\nReturn to the menu or replay the room.";
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

        private Text CreateText(string name, Transform parent, int fontSize, TextAnchor alignment, string content = "")
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = content;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = name.Contains("Title") ? new Vector2(0f, -40f) : new Vector2(0f, -108f);
            rect.sizeDelta = new Vector2(640f, 110f);

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

            Text buttonText = CreateText($"{name}Label", buttonObject.transform, 24, TextAnchor.MiddleCenter, label);
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

            if (goalZone != null)
            {
                goalZone.Completed += HandleLevelCompleted;
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
    }
}
