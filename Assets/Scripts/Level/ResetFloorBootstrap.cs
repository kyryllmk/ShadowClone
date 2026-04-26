using ShadowClone.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowClone.Level
{
    public class ResetFloorBootstrap : MonoBehaviour
    {
        private const string BootstrapObjectName = "ResetFloorBootstrap";
        private const float ResetFloorY = -7f;
        private const float ResetFloorHeight = 0.75f;
        private const float ResetFloorPadding = 24f;
        private static ResetFloorBootstrap instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject(BootstrapObjectName);
            bootstrapObject.AddComponent<ResetFloorBootstrap>();
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
            if (!LevelProgressManager.TryGetLevelNumberForScene(scene.name, out _))
            {
                return;
            }

            RoomResetManager resetManager = FindObjectOfType<RoomResetManager>();
            if (resetManager == null)
            {
                return;
            }

            SpawnPoint spawnPoint = FindObjectOfType<SpawnPoint>();
            GoalZone goalZone = FindObjectOfType<GoalZone>();
            float startX = spawnPoint != null ? spawnPoint.transform.position.x : -12f;
            float endX = goalZone != null ? goalZone.transform.position.x : 120f;
            float minX = Mathf.Min(startX, endX) - ResetFloorPadding;
            float maxX = Mathf.Max(startX, endX) + ResetFloorPadding;
            float centerX = (minX + maxX) * 0.5f;
            float width = Mathf.Max(80f, maxX - minX);

            GameObject[] sceneObjects = scene.GetRootGameObjects();
            for (int i = 0; i < sceneObjects.Length; i++)
            {
                ConfigureResetFloors(sceneObjects[i].transform, resetManager, centerX, width);
            }
        }

        private static void ConfigureResetFloors(Transform root, RoomResetManager resetManager, float centerX, float width)
        {
            if (root == null)
            {
                return;
            }

            if (root.name.IndexOf("ResetFloor", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                root.position = new Vector3(centerX, ResetFloorY, root.position.z);
                root.localScale = Vector3.one;

                BoxCollider2D resetCollider = root.GetComponent<BoxCollider2D>();
                if (resetCollider == null)
                {
                    resetCollider = root.gameObject.AddComponent<BoxCollider2D>();
                }

                resetCollider.isTrigger = true;
                resetCollider.offset = Vector2.zero;
                resetCollider.size = new Vector2(width, ResetFloorHeight);

                SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }

                Hazard hazard = root.GetComponent<Hazard>();
                if (hazard == null)
                {
                    hazard = root.gameObject.AddComponent<Hazard>();
                }

                hazard.ConfigureReset(resetManager, "Fall reset");

                ResetFloor resetFloor = root.GetComponent<ResetFloor>();
                if (resetFloor == null)
                {
                    resetFloor = root.gameObject.AddComponent<ResetFloor>();
                }

                resetFloor.Configure(resetManager);
                return;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                ConfigureResetFloors(root.GetChild(i), resetManager, centerX, width);
            }
        }
    }
}
