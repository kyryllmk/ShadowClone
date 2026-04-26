using ShadowClone.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowClone.Level
{
    public class LevelOneOnboardingBootstrap : MonoBehaviour
    {
        private const string BootstrapObjectName = "LevelOneOnboardingBootstrap";
        private static LevelOneOnboardingBootstrap instance;

        private static readonly PromptPlacement[] PromptPlacements =
        {
            new PromptPlacement("A = MOVE LEFT | D = MOVE RIGHT\nSPACE = JUMP", 0f),
            new PromptPlacement("JUMP OVER PLATFORMS", 0.06f),
            new PromptPlacement("R = RECORD SHADOW | E = REPLAY SHADOW\nSHADOW REPLAY LASTS 3 SECONDS", 0.16f),
            new PromptPlacement("CLONE CAN HOLD BUTTONS", 0.215f),
            new PromptPlacement("USE CLONE TO OPEN THE DOOR", 0.24f),
            new PromptPlacement("REACH THE EXIT", 0.9f)
        };

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

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                instance = null;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != SceneRegistry.Tutorial)
            {
                return;
            }

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
                CreatePromptTrigger(root.transform, i + 1, placement.Message, new Vector2(x, -1.2f));
            }
        }

        private static void CreatePromptTrigger(Transform parent, int index, string prompt, Vector2 position)
        {
            string safePromptName = prompt
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
            promptTrigger.Configure(prompt, 3.2f);
        }

        private readonly struct PromptPlacement
        {
            public PromptPlacement(string message, float normalizedX)
            {
                Message = message;
                NormalizedX = normalizedX;
            }

            public string Message { get; }
            public float NormalizedX { get; }
        }
    }
}
