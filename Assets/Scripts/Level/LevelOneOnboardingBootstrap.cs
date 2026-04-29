using ShadowClone.Core;
using ShadowClone.Gameplay;
using ShadowClone.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowClone.Level
{
    public class LevelOneOnboardingBootstrap : MonoBehaviour
    {
        private const string BootstrapObjectName = "LevelOneOnboardingBootstrap";
        private const float DoorTutorialCompleteX = 56f;
        private const float DefaultPromptDuration = 3.2f;
        private const float PersistentPromptDuration = 999f;
        private const string MovementPrompt = "A = MOVE LEFT | D = MOVE RIGHT\nSPACE = JUMP\n<size=70%>ESC = PAUSE</size>";
        private const string ClonePrompt = "R = RECORD SHADOW | E = REPLAY SHADOW\nSHADOW REPLAY LASTS 3 SECONDS";
        private static LevelOneOnboardingBootstrap instance;
        private static bool hasCompletedGapTutorial;
        private static bool hasCompletedDoorTutorial;
        private static bool hasCompletedExitTutorial;

        private static readonly PromptPlacement[] PromptPlacements =
        {
            new PromptPlacement(MovementPrompt, 0f, TutorialProgressSection.None, TutorialProgressAction.Show, true, PersistentPromptDuration),
            new PromptPlacement("JUMP OVER GAPS", 0.06f, TutorialProgressSection.Gap, TutorialProgressAction.Show),
            new PromptPlacement(string.Empty, 0.12f, TutorialProgressSection.Gap, TutorialProgressAction.Complete),
            new PromptPlacement(ClonePrompt, 0.16f, TutorialProgressSection.None, TutorialProgressAction.Show, true, PersistentPromptDuration),
            new PromptPlacement("CLONE CAN HOLD BUTTONS\nUSE IT TO OPEN THE DOOR", 0.215f, TutorialProgressSection.Door, TutorialProgressAction.Show, false, PersistentPromptDuration),
            new PromptPlacement("REACH THE EXIT", 0.9f, TutorialProgressSection.Exit, TutorialProgressAction.Show)
        };

        private GoalZone trackedGoalZone;
        private RoomResetManager trackedResetManager;
        private PlayerController trackedPlayer;
        private readonly List<OnboardingPromptTrigger> replayAfterResetTriggers = new List<OnboardingPromptTrigger>();
        private bool hasPressedMoveRight;
        private bool hasPressedJump;
        private bool hasCompletedMovementPrompt;
        private bool hasPressedRecord;
        private bool hasPressedReplay;
        private bool hasCompletedClonePrompt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject(BootstrapObjectName);
            bootstrapObject.AddComponent<LevelOneOnboardingBootstrap>();
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
            SceneManager.sceneLoaded += HandleSceneLoaded;
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void Update()
        {
            TrackActionPromptCompletion();

            if (trackedPlayer == null || hasCompletedDoorTutorial)
            {
                return;
            }

            if (trackedPlayer.transform.position.x >= DoorTutorialCompleteX)
            {
                CompleteProgressSection(TutorialProgressSection.Door);
            }
        }

        private void TrackActionPromptCompletion()
        {
            if (SceneManager.GetActiveScene().name != SceneRegistry.Tutorial)
            {
                return;
            }

            if (!hasCompletedMovementPrompt)
            {
                hasPressedMoveRight |= Input.GetKeyDown(KeyCode.D);
                hasPressedJump |= Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump");

                if (hasPressedMoveRight && hasPressedJump)
                {
                    hasCompletedMovementPrompt = true;
                    GameplayUiBootstrap.HideOnboardingPrompt(MovementPrompt);
                }
            }

            if (!hasCompletedClonePrompt)
            {
                hasPressedRecord |= Input.GetKeyDown(KeyCode.R);
                hasPressedReplay |= Input.GetKeyDown(KeyCode.E);

                if (hasPressedRecord && hasPressedReplay)
                {
                    hasCompletedClonePrompt = true;
                    GameplayUiBootstrap.HideOnboardingPrompt(ClonePrompt);
                }
            }
        }

        private void OnDestroy()
        {
            ReleaseLevelOneSubscriptions();

            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                instance = null;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ReleaseLevelOneSubscriptions();

            if (scene.name != SceneRegistry.Tutorial)
            {
                return;
            }

            ResetRuntimeProgress();
            ResetActionPromptProgress();

            SpawnPoint spawnPoint = FindObjectOfType<SpawnPoint>();
            GoalZone goalZone = FindObjectOfType<GoalZone>();
            if (spawnPoint == null || goalZone == null)
            {
                return;
            }

            GameObject root = new GameObject("Level01_OnboardingTriggers");
            SceneManager.MoveGameObjectToScene(root, scene);

            float startX = spawnPoint.transform.position.x;
            float endX = goalZone.transform.position.x;
            float length = Mathf.Max(20f, endX - startX);

            for (int i = 0; i < PromptPlacements.Length; i++)
            {
                PromptPlacement placement = PromptPlacements[i];
                float x = startX + length * placement.NormalizedX;
                CreatePromptTrigger(root.transform, i + 1, placement, new Vector2(x, -1.2f));
            }

            trackedPlayer = FindObjectOfType<PlayerController>();
            trackedResetManager = FindObjectOfType<RoomResetManager>();
            if (trackedResetManager != null)
            {
                trackedResetManager.ResetCompleted += HandleResetCompleted;
            }

            trackedGoalZone = goalZone;
            trackedGoalZone.Completed += HandleGoalCompleted;
        }

        private static void CreatePromptTrigger(Transform parent, int index, PromptPlacement placement, Vector2 position)
        {
            string label = string.IsNullOrWhiteSpace(placement.Message)
                ? $"{placement.Section}_{placement.Action}"
                : placement.Message;

            string safePromptName = label
                .Replace('\n', '_')
                .Replace(' ', '_')
                .Replace('/', '_')
                .Replace('|', '_')
                .Replace('=', '_');
            GameObject triggerObject = new GameObject($"Onboarding_{index:00}_{safePromptName}");
            triggerObject.transform.SetParent(parent, false);
            triggerObject.transform.position = new Vector3(position.x, position.y, 0f);

            BoxCollider2D trigger = triggerObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(8f, 8f);

            OnboardingPromptTrigger promptTrigger = triggerObject.AddComponent<OnboardingPromptTrigger>();
            promptTrigger.Configure(placement.Message, placement.Duration, placement.Section, placement.Action, placement.ReplayAfterReset);

            if (placement.ReplayAfterReset && instance != null)
            {
                instance.replayAfterResetTriggers.Add(promptTrigger);
            }
        }

        internal static bool TryShowProgressPrompt(TutorialProgressSection section, string message, float duration)
        {
            if (IsSectionCompleted(section))
            {
                return false;
            }

            GameplayUiBootstrap.ShowOnboardingPrompt(message, duration);
            return true;
        }

        internal static void CompleteProgressSection(TutorialProgressSection section)
        {
            if (IsSectionCompleted(section))
            {
                return;
            }

            switch (section)
            {
                case TutorialProgressSection.Gap:
                    hasCompletedGapTutorial = true;
                    break;
                case TutorialProgressSection.Door:
                    hasCompletedDoorTutorial = true;
                    break;
                case TutorialProgressSection.Exit:
                    hasCompletedExitTutorial = true;
                    break;
            }

            GameplayUiBootstrap.HideOnboardingPrompt();
        }

        private static bool IsSectionCompleted(TutorialProgressSection section)
        {
            return section switch
            {
                TutorialProgressSection.Gap => hasCompletedGapTutorial,
                TutorialProgressSection.Door => hasCompletedDoorTutorial,
                TutorialProgressSection.Exit => hasCompletedExitTutorial,
                _ => false
            };
        }

        private static void ResetRuntimeProgress()
        {
            hasCompletedGapTutorial = false;
            hasCompletedDoorTutorial = false;
            hasCompletedExitTutorial = false;
        }

        private void ResetActionPromptProgress()
        {
            hasPressedMoveRight = false;
            hasPressedJump = false;
            hasCompletedMovementPrompt = false;
            hasPressedRecord = false;
            hasPressedReplay = false;
            hasCompletedClonePrompt = false;
        }

        private void HandleResetCompleted(string reason)
        {
            GameplayUiBootstrap.HideOnboardingPrompt();
            ResetActionPromptProgress();

            for (int i = 0; i < replayAfterResetTriggers.Count; i++)
            {
                if (replayAfterResetTriggers[i] != null)
                {
                    replayAfterResetTriggers[i].ResetForAttempt();
                }
            }

            GameplayUiBootstrap.ShowOnboardingPrompt(MovementPrompt, PersistentPromptDuration);
        }

        private void HandleGoalCompleted()
        {
            CompleteProgressSection(TutorialProgressSection.Exit);
        }

        private void ReleaseLevelOneSubscriptions()
        {
            if (trackedGoalZone != null)
            {
                trackedGoalZone.Completed -= HandleGoalCompleted;
                trackedGoalZone = null;
            }

            if (trackedResetManager != null)
            {
                trackedResetManager.ResetCompleted -= HandleResetCompleted;
                trackedResetManager = null;
            }

            trackedPlayer = null;
            replayAfterResetTriggers.Clear();
        }

        private readonly struct PromptPlacement
        {
            public PromptPlacement(
                string message,
                float normalizedX,
                TutorialProgressSection section,
                TutorialProgressAction action,
                bool replayAfterReset = false,
                float duration = DefaultPromptDuration)
            {
                Message = message;
                NormalizedX = normalizedX;
                Section = section;
                Action = action;
                ReplayAfterReset = replayAfterReset;
                Duration = duration;
            }

            public string Message { get; }
            public float NormalizedX { get; }
            public TutorialProgressSection Section { get; }
            public TutorialProgressAction Action { get; }
            public bool ReplayAfterReset { get; }
            public float Duration { get; }
        }
    }

    public enum TutorialProgressSection
    {
        None,
        Gap,
        Door,
        Exit
    }

    public enum TutorialProgressAction
    {
        Show,
        Complete
    }
}
