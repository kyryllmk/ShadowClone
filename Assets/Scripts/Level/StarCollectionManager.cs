using System.Collections.Generic;
using ShadowClone.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowClone.Level
{
    public class StarCollectionManager : MonoBehaviour
    {
        private const int StarsPerLevel = 3;
        private static StarCollectionManager activeManager;

        private readonly List<StarCollectible> stars = new List<StarCollectible>();
        private RoomResetManager roomResetManager;
        private GoalZone goalZone;
        private int levelNumber;
        private int collectedThisRun;

        public static StarCollectionManager Active => activeManager;
        public int CollectedThisRun => collectedThisRun;
        public int TotalStars => StarsPerLevel;
        public event System.Action<int, int> StarsChanged;

        private void Awake()
        {
            activeManager = this;
        }

        private void OnDestroy()
        {
            if (activeManager == this)
            {
                activeManager = null;
            }

            Unbind();
        }

        public void Initialize(int sceneLevelNumber, Vector3 spawnPosition, Vector3 goalPosition)
        {
            levelNumber = sceneLevelNumber;
            roomResetManager = FindObjectOfType<RoomResetManager>();
            goalZone = FindObjectOfType<GoalZone>();

            if (roomResetManager != null)
            {
                roomResetManager.ResetCompleted += HandleResetCompleted;
            }

            if (goalZone != null)
            {
                goalZone.Completed += HandleLevelCompleted;
            }

            BuildStars(spawnPosition, goalPosition);
            NotifyStarsChanged();
        }

        private void BuildStars(Vector3 spawnPosition, Vector3 goalPosition)
        {
            float levelLength = Mathf.Max(20f, goalPosition.x - spawnPosition.x);
            StarPlacement[] placements = GetPlacements(levelNumber);

            for (int i = 0; i < StarsPerLevel; i++)
            {
                StarPlacement placement = placements[Mathf.Min(i, placements.Length - 1)];
                Vector3 position = new Vector3(
                    spawnPosition.x + levelLength * placement.NormalizedX,
                    placement.WorldY,
                    0f);

                GameObject starObject = new GameObject($"Star_{i + 1}");
                starObject.transform.SetParent(transform, false);
                starObject.transform.position = position;
                starObject.transform.localScale = Vector3.one * placement.Scale;

                CircleCollider2D collider = starObject.AddComponent<CircleCollider2D>();
                collider.radius = 0.58f;
                collider.isTrigger = true;

                StarCollectible star = starObject.AddComponent<StarCollectible>();
                star.Configure(i);
                star.Collected += HandleStarCollected;
                stars.Add(star);
            }
        }

        private void HandleStarCollected(StarCollectible star)
        {
            collectedThisRun = Mathf.Clamp(collectedThisRun + 1, 0, StarsPerLevel);
            NotifyStarsChanged();
        }

        private void HandleResetCompleted(string reason)
        {
            collectedThisRun = 0;
            for (int i = 0; i < stars.Count; i++)
            {
                if (stars[i] != null)
                {
                    stars[i].RestoreForRun();
                }
            }

            NotifyStarsChanged();
        }

        private void HandleLevelCompleted()
        {
            LevelProgressManager.SetStars(levelNumber, collectedThisRun);
            NotifyStarsChanged();
        }

        private void NotifyStarsChanged()
        {
            StarsChanged?.Invoke(collectedThisRun, StarsPerLevel);
        }

        private void Unbind()
        {
            if (roomResetManager != null)
            {
                roomResetManager.ResetCompleted -= HandleResetCompleted;
                roomResetManager = null;
            }

            if (goalZone != null)
            {
                goalZone.Completed -= HandleLevelCompleted;
                goalZone = null;
            }

            for (int i = 0; i < stars.Count; i++)
            {
                if (stars[i] != null)
                {
                    stars[i].Collected -= HandleStarCollected;
                }
            }

            stars.Clear();
        }

        private static StarPlacement[] GetPlacements(int level)
        {
            switch (level)
            {
                case 1:
                    return new[]
                    {
                        new StarPlacement(0.25f, -1.15f, 1f),
                        new StarPlacement(0.54f, -0.85f, 1f),
                        new StarPlacement(0.82f, -1.1f, 1f)
                    };
                case 2:
                    return new[]
                    {
                        new StarPlacement(0.22f, -0.85f, 1f),
                        new StarPlacement(0.55f, 0.2f, 1f),
                        new StarPlacement(0.84f, -0.75f, 1f)
                    };
                case 3:
                    return new[]
                    {
                        new StarPlacement(0.2f, -0.65f, 1f),
                        new StarPlacement(0.52f, 0.35f, 1f),
                        new StarPlacement(0.82f, -0.5f, 1f)
                    };
                case 4:
                    return new[]
                    {
                        new StarPlacement(0.18f, -0.55f, 1f),
                        new StarPlacement(0.5f, 0.55f, 1f),
                        new StarPlacement(0.82f, 0.15f, 1f)
                    };
                default:
                    return new[]
                    {
                        new StarPlacement(0.25f, -1f, 1f),
                        new StarPlacement(0.55f, -0.6f, 1f),
                        new StarPlacement(0.82f, -1f, 1f)
                    };
            }
        }

        private readonly struct StarPlacement
        {
            public StarPlacement(float normalizedX, float worldY, float scale)
            {
                NormalizedX = normalizedX;
                WorldY = worldY;
                Scale = scale;
            }

            public float NormalizedX { get; }
            public float WorldY { get; }
            public float Scale { get; }
        }
    }

    public class StarCollectionBootstrap : MonoBehaviour
    {
        private const string BootstrapObjectName = "StarCollectionBootstrap";
        private static StarCollectionBootstrap instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject(BootstrapObjectName);
            bootstrapObject.AddComponent<StarCollectionBootstrap>();
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
            if (!LevelProgressManager.TryGetLevelNumberForScene(scene.name, out int levelNumber))
            {
                return;
            }

            SpawnPoint spawnPoint = FindObjectOfType<SpawnPoint>();
            GoalZone goalZone = FindObjectOfType<GoalZone>();
            if (spawnPoint == null || goalZone == null)
            {
                return;
            }

            GameObject managerObject = new GameObject("StarCollectionManager");
            SceneManager.MoveGameObjectToScene(managerObject, scene);
            StarCollectionManager manager = managerObject.AddComponent<StarCollectionManager>();
            manager.Initialize(levelNumber, spawnPoint.transform.position, goalZone.transform.position);
        }
    }
}
