using System.Collections.Generic;
using ShadowClone.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowClone.Presentation
{
    public class EnvironmentBackdropBootstrap : MonoBehaviour
    {
        private const string BootstrapObjectName = "EnvironmentBackdropBootstrap";
        private const string BackdropRootName = "EnvironmentBackdrop";

        private static EnvironmentBackdropBootstrap instance;

        private GameObject currentBackdropRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject(BootstrapObjectName);
            bootstrapObject.AddComponent<EnvironmentBackdropBootstrap>();
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
            DestroyCurrentBackdrop();

            currentBackdropRoot = new GameObject(BackdropRootName);
            SceneManager.MoveGameObjectToScene(currentBackdropRoot, scene);

            EnvironmentBackdropController controller = currentBackdropRoot.AddComponent<EnvironmentBackdropController>();
            controller.Build(scene.name);
        }

        private void DestroyCurrentBackdrop()
        {
            if (currentBackdropRoot == null)
            {
                return;
            }

            Destroy(currentBackdropRoot);
            currentBackdropRoot = null;
        }

    }

    public class EnvironmentBackdropController : MonoBehaviour
    {
        private enum BackdropMode
        {
            MainMenu,
            LevelSelect,
            Gameplay
        }

        private static Sprite solidSprite;

        private readonly List<ParallaxElement> parallaxElements = new List<ParallaxElement>();
        private readonly List<PulseElement> pulseElements = new List<PulseElement>();

        private Camera targetCamera;
        private BackdropMode backdropMode;

        public void Build(string sceneName)
        {
            targetCamera = Camera.main;
            backdropMode = GetMode(sceneName);
            transform.localScale = Vector3.one;
            transform.position = targetCamera != null ? new Vector3(targetCamera.transform.position.x, targetCamera.transform.position.y, 0f) : Vector3.zero;

            ScenePalette palette = GetPalette(sceneName);

            Transform farLayer = CreateLayer("FarLayer");
            Transform midLayer = CreateLayer("MidLayer");

            Vector2 sceneSize = GetSceneSize(backdropMode);
            CreateGradient(farLayer, palette.FarTop, palette.FarBottom, sceneSize, -320);
            CreateSolidPanel("FarFloorShadow", farLayer, new Vector2(0f, -sceneSize.y * 0.24f), new Vector2(sceneSize.x, sceneSize.y * 0.18f), palette.FloorShadow, -318);
            CreateSolidPanel("FarCeilingShadow", farLayer, new Vector2(0f, sceneSize.y * 0.23f), new Vector2(sceneSize.x, sceneSize.y * 0.14f), palette.CeilingShadow, -318);

            switch (backdropMode)
            {
                case BackdropMode.MainMenu:
                    BuildMenuBackdrop(midLayer, palette);
                    break;
                case BackdropMode.LevelSelect:
                    BuildLevelSelectBackdrop(midLayer, palette);
                    break;
                default:
                    BuildGameplayBackdrop(midLayer, palette, sceneName);
                    break;
            }
        }

        private void Update()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            Vector3 cameraPosition = targetCamera != null ? targetCamera.transform.position : Vector3.zero;
            transform.position = new Vector3(cameraPosition.x, cameraPosition.y, 0f);

            float time = Time.time;

            for (int i = 0; i < parallaxElements.Count; i++)
            {
                ParallaxElement element = parallaxElements[i];
                if (element.Transform == null)
                {
                    continue;
                }

                Vector3 localPosition = element.BaseLocalPosition;
                localPosition.x += cameraPosition.x * element.ParallaxX;
                localPosition.y += cameraPosition.y * element.ParallaxY;
                localPosition.x += Mathf.Sin(time * element.MotionSpeed + element.PhaseOffset) * element.MotionAmplitude;
                localPosition.y += Mathf.Cos(time * element.MotionSpeed * 0.7f + element.PhaseOffset) * element.MotionAmplitude * 0.35f;
                element.Transform.localPosition = localPosition;
            }

            for (int i = 0; i < pulseElements.Count; i++)
            {
                PulseElement element = pulseElements[i];
                if (element.Renderer == null)
                {
                    continue;
                }

                float localPulse = 0.5f + (Mathf.Sin(time * element.Speed + element.PhaseOffset) * 0.5f);
                float beatPulse = Mathf.Clamp01(PresentationFeedbackBootstrap.BeatPulse01 + localPulse * 0.12f);
                element.Renderer.color = Color.Lerp(element.BaseColor, element.TargetColor, beatPulse * element.Intensity);
            }
        }

        private void BuildGameplayBackdrop(Transform parent, ScenePalette palette, string sceneName)
        {
            GameplayBackdropPreset preset = GetGameplayPreset(sceneName);

            BuildFrames(parent, palette, preset.FrameWidth, preset.FrameHeight, preset.FrameParallaxX, preset.FrameParallaxY, preset.FrameMotionAmplitude);
            BuildColumns(parent, palette, preset.ColumnPositions, preset.ColumnParallaxX, preset.ColumnParallaxY, preset.ColumnMotionAmplitude, preset.ColumnPhaseOffset);
            BuildWindows(parent, palette, preset.WindowWidths, preset.WindowPositions, preset.WindowParallaxX, preset.WindowParallaxY, preset.WindowMotionAmplitude, preset.WindowPhaseOffset);
            BuildRearPlatforms(parent, palette, preset.RearParallaxX, preset.RearParallaxY, preset.RearMotionAmplitude, preset.RearPhaseOffset);

            BuildGameplaySceneAccent(parent, palette, preset);
        }

        private void BuildGameplaySceneAccent(Transform parent, ScenePalette palette, GameplayBackdropPreset preset)
        {
            switch (preset.SceneName)
            {
                case SceneRegistry.Tutorial:
                    CreateSolidPanel("TutorialVault", parent, new Vector2(0f, 4.2f), new Vector2(15.5f, 4f), palette.WindowPane * 0.8f, -205);
                    CreateSolidPanel("TutorialBeamLeft", parent, new Vector2(-15.4f, 0.6f), new Vector2(1.1f, 8.8f), palette.ColumnDark * 0.92f, -207);
                    CreateSolidPanel("TutorialBeamRight", parent, new Vector2(15.4f, 0.6f), new Vector2(1.1f, 8.8f), palette.ColumnDark * 0.92f, -207);
                    break;
                case SceneRegistry.ButtonDoor:
                    CreateSolidPanel("PuzzleFrameLeft", parent, new Vector2(-16.2f, 1.45f), new Vector2(2.6f, 11.2f), palette.FrameDark * 0.94f, -207);
                    CreateSolidPanel("PuzzleFrameMid", parent, new Vector2(1.6f, 1.7f), new Vector2(1.4f, 13.2f), palette.FrameDark * 0.9f, -207);
                    CreateSolidPanel("PuzzleFrameRight", parent, new Vector2(16.2f, 1.2f), new Vector2(2.2f, 12.7f), palette.FrameDark * 0.94f, -207);
                    SpriteRenderer routeAccent = CreateSolidPanel("PuzzleRouteAccent", parent, new Vector2(2.6f, 5.9f), new Vector2(12f, 0.08f), palette.AccentDim, -206);
                    AddPulse(routeAccent, palette.AccentDim, palette.AccentPulse, preset.ExtraAccentIntensity, preset.ExtraAccentSpeed, 0.2f);
                    break;
                case SceneRegistry.HazardTiming:
                    CreateSolidPanel("HazardTunnelTop", parent, new Vector2(0f, 6.2f), new Vector2(31.5f, 1.85f), palette.FrameDark * 0.96f, -207);
                    CreateSolidPanel("HazardTunnelLeft", parent, new Vector2(-17.1f, 0.95f), new Vector2(1.25f, 10.2f), palette.ColumnDark, -207);
                    CreateSolidPanel("HazardTunnelRight", parent, new Vector2(17.1f, 0.95f), new Vector2(1.25f, 10.2f), palette.ColumnDark, -207);
                    CreateSolidPanel("HazardBulkheadA", parent, new Vector2(-8.4f, 2.1f), new Vector2(1.1f, 12.6f), palette.FrameDark * 0.9f, -206);
                    CreateSolidPanel("HazardBulkheadB", parent, new Vector2(8.8f, 1.8f), new Vector2(1.1f, 13.2f), palette.FrameDark * 0.9f, -206);
                    break;
                case SceneRegistry.Final:
                    CreateSolidPanel("FinalArchLeft", parent, new Vector2(-19.8f, 2.7f), new Vector2(1.7f, 16.4f), palette.FrameDark, -207);
                    CreateSolidPanel("FinalArchRight", parent, new Vector2(19.8f, 2.7f), new Vector2(1.7f, 16.4f), palette.FrameDark, -207);
                    CreateSolidPanel("FinalArchTop", parent, new Vector2(0f, 9.2f), new Vector2(41.5f, 1.35f), palette.FrameDark, -207);
                    CreateSolidPanel("FinalSpine", parent, new Vector2(0.6f, 2.25f), new Vector2(2f, 14.4f), palette.FrameDark * 0.94f, -206);
                    CreateSolidPanel("FinalCrossBeamLeft", parent, new Vector2(-10.2f, 5.4f), new Vector2(8f, 0.5f), palette.FrameDark * 0.9f, -206);
                    CreateSolidPanel("FinalCrossBeamRight", parent, new Vector2(11.6f, 4.6f), new Vector2(9.2f, 0.5f), palette.FrameDark * 0.9f, -206);

                    SpriteRenderer finalAccent = CreateSolidPanel("FinalArchAccent", parent, new Vector2(0f, 8.35f), new Vector2(32f, 0.12f), palette.AccentSoft, -205);
                    AddPulse(finalAccent, palette.AccentSoft, palette.AccentPulse, preset.ExtraAccentIntensity, preset.ExtraAccentSpeed, 0.5f);
                    SpriteRenderer finalSpineAccent = CreateSolidPanel("FinalSpineAccent", parent, new Vector2(0.6f, 2.3f), new Vector2(0.12f, 12.8f), palette.AccentDim, -205);
                    AddPulse(finalSpineAccent, palette.AccentDim, palette.AccentPulse, preset.ExtraAccentIntensity * 0.72f, preset.ExtraAccentSpeed * 0.78f, 0.85f);
                    break;
            }
        }

        private static GameplayBackdropPreset GetGameplayPreset(string sceneName)
        {
            switch (sceneName)
            {
                case SceneRegistry.Tutorial:
                    return new GameplayBackdropPreset
                    {
                        SceneName = sceneName,
                        FrameWidth = 32f,
                        FrameHeight = 16f,
                        FrameParallaxX = 0.1f,
                        FrameParallaxY = 0.012f,
                        FrameMotionAmplitude = 0.12f,
                        ColumnPositions = new[] { -12f, -4.25f, 4.5f, 12.25f },
                        ColumnParallaxX = 0.14f,
                        ColumnParallaxY = 0.008f,
                        ColumnMotionAmplitude = 0.08f,
                        ColumnPhaseOffset = 0.55f,
                        WindowWidths = new[] { 5.2f, 8.6f, 5.2f },
                        WindowPositions = new[] { -11f, 0f, 11f },
                        WindowParallaxX = 0.2f,
                        WindowParallaxY = 0.01f,
                        WindowMotionAmplitude = 0.04f,
                        WindowPhaseOffset = 0.9f,
                        RearParallaxX = 0.26f,
                        RearParallaxY = 0.006f,
                        RearMotionAmplitude = 0.03f,
                        RearPhaseOffset = 1.2f,
                        ExtraAccentIntensity = 0.1f,
                        ExtraAccentSpeed = 0.2f
                    };
                case SceneRegistry.ButtonDoor:
                    return new GameplayBackdropPreset
                    {
                        SceneName = sceneName,
                        FrameWidth = 36f,
                        FrameHeight = 18f,
                        FrameParallaxX = 0.13f,
                        FrameParallaxY = 0.015f,
                        FrameMotionAmplitude = 0.16f,
                        ColumnPositions = new[] { -15f, -10.5f, -5.5f, 0.5f, 7.5f, 14.5f },
                        ColumnParallaxX = 0.18f,
                        ColumnParallaxY = 0.01f,
                        ColumnMotionAmplitude = 0.09f,
                        ColumnPhaseOffset = 0.72f,
                        WindowWidths = new[] { 4.2f, 4.2f, 7.4f, 4.2f },
                        WindowPositions = new[] { -13.8f, -6.4f, 2.8f, 12.6f },
                        WindowParallaxX = 0.24f,
                        WindowParallaxY = 0.012f,
                        WindowMotionAmplitude = 0.05f,
                        WindowPhaseOffset = 1.05f,
                        RearParallaxX = 0.32f,
                        RearParallaxY = 0.007f,
                        RearMotionAmplitude = 0.04f,
                        RearPhaseOffset = 1.45f,
                        ExtraAccentIntensity = 0.12f,
                        ExtraAccentSpeed = 0.24f
                    };
                case SceneRegistry.HazardTiming:
                    return new GameplayBackdropPreset
                    {
                        SceneName = sceneName,
                        FrameWidth = 38f,
                        FrameHeight = 18f,
                        FrameParallaxX = 0.15f,
                        FrameParallaxY = 0.018f,
                        FrameMotionAmplitude = 0.18f,
                        ColumnPositions = new[] { -16.5f, -11.5f, -6.75f, -1.25f, 4.5f, 10.2f, 16.2f },
                        ColumnParallaxX = 0.22f,
                        ColumnParallaxY = 0.012f,
                        ColumnMotionAmplitude = 0.1f,
                        ColumnPhaseOffset = 0.8f,
                        WindowWidths = new[] { 4.8f, 5.4f, 4.8f },
                        WindowPositions = new[] { -12f, 0f, 12f },
                        WindowParallaxX = 0.28f,
                        WindowParallaxY = 0.014f,
                        WindowMotionAmplitude = 0.06f,
                        WindowPhaseOffset = 1.18f,
                        RearParallaxX = 0.36f,
                        RearParallaxY = 0.008f,
                        RearMotionAmplitude = 0.045f,
                        RearPhaseOffset = 1.75f,
                        ExtraAccentIntensity = 0.16f,
                        ExtraAccentSpeed = 0.26f
                    };
                case SceneRegistry.Final:
                    return new GameplayBackdropPreset
                    {
                        SceneName = sceneName,
                        FrameWidth = 42f,
                        FrameHeight = 20f,
                        FrameParallaxX = 0.18f,
                        FrameParallaxY = 0.02f,
                        FrameMotionAmplitude = 0.2f,
                        ColumnPositions = new[] { -18.5f, -13f, -7.5f, -1.5f, 4.5f, 10.8f, 16.4f, 20.8f },
                        ColumnParallaxX = 0.24f,
                        ColumnParallaxY = 0.014f,
                        ColumnMotionAmplitude = 0.11f,
                        ColumnPhaseOffset = 0.92f,
                        WindowWidths = new[] { 6.4f, 7.8f, 6.4f },
                        WindowPositions = new[] { -13.5f, 0.8f, 14.5f },
                        WindowParallaxX = 0.3f,
                        WindowParallaxY = 0.015f,
                        WindowMotionAmplitude = 0.07f,
                        WindowPhaseOffset = 1.26f,
                        RearParallaxX = 0.4f,
                        RearParallaxY = 0.01f,
                        RearMotionAmplitude = 0.05f,
                        RearPhaseOffset = 1.9f,
                        ExtraAccentIntensity = 0.28f,
                        ExtraAccentSpeed = 0.36f
                    };
                default:
                    return new GameplayBackdropPreset
                    {
                        SceneName = sceneName,
                        FrameWidth = 36f,
                        FrameHeight = 18f,
                        FrameParallaxX = 0.12f,
                        FrameParallaxY = 0.015f,
                        FrameMotionAmplitude = 0.18f,
                        ColumnPositions = new[] { -14f, -8.5f, -3.5f, 3.25f, 9f, 14.5f },
                        ColumnParallaxX = 0.18f,
                        ColumnParallaxY = 0.01f,
                        ColumnMotionAmplitude = 0.12f,
                        ColumnPhaseOffset = 0.7f,
                        WindowWidths = new[] { 6.2f, 5.5f, 7f },
                        WindowPositions = new[] { -11f, -1.25f, 9.2f },
                        WindowParallaxX = 0.26f,
                        WindowParallaxY = 0.012f,
                        WindowMotionAmplitude = 0.08f,
                        WindowPhaseOffset = 1.1f,
                        RearParallaxX = 0.34f,
                        RearParallaxY = 0.008f,
                        RearMotionAmplitude = 0.05f,
                        RearPhaseOffset = 1.65f,
                        ExtraAccentIntensity = 0.16f,
                        ExtraAccentSpeed = 0.3f
                    };
            }
        }

        private void BuildMenuBackdrop(Transform parent, ScenePalette palette)
        {
            BuildFrames(parent, palette, 44f, 22f, 0.06f, 0.01f, 0.2f);
            BuildColumns(parent, palette, new[] { -18f, -11f, -4f, 4.5f, 12.5f, 19f }, 0.1f, 0.008f, 0.16f, 0.25f);
            BuildWindows(parent, palette, new[] { 9f, 7.4f, 9f }, new[] { -15f, 0f, 15f }, 0.14f, 0.01f, 0.09f, 0.8f);

            Transform heroGroup = CreateGroup(parent, "MenuHero", new Vector2(0f, 1.2f), 0.04f, 0.004f, 0.12f, 1.4f);
            CreateSolidPanel("HeroWindow", heroGroup, new Vector2(0f, 3.4f), new Vector2(24f, 8f), palette.WindowPane * 1.05f, -208);
            CreateSolidPanel("HeroFrameTop", heroGroup, new Vector2(0f, 7.5f), new Vector2(28f, 0.8f), palette.FrameDark, -207);
            CreateSolidPanel("HeroFrameBottom", heroGroup, new Vector2(0f, -0.8f), new Vector2(28f, 0.55f), palette.FrameDark * 0.92f, -207);
            CreateSolidPanel("HeroFrameLeft", heroGroup, new Vector2(-13.9f, 3.35f), new Vector2(0.9f, 8.8f), palette.FrameDark, -207);
            CreateSolidPanel("HeroFrameRight", heroGroup, new Vector2(13.9f, 3.35f), new Vector2(0.9f, 8.8f), palette.FrameDark, -207);

            SpriteRenderer heroAccentTop = CreateSolidPanel("HeroAccentTop", heroGroup, new Vector2(0f, 7.05f), new Vector2(20f, 0.14f), palette.AccentSoft, -206);
            AddPulse(heroAccentTop, palette.AccentSoft, palette.AccentPulse, 0.34f, 0.42f, 0.1f);

            SpriteRenderer heroAccentBottom = CreateSolidPanel("HeroAccentBottom", heroGroup, new Vector2(0f, -0.35f), new Vector2(16f, 0.1f), palette.AccentDim, -206);
            AddPulse(heroAccentBottom, palette.AccentDim, palette.AccentPulse, 0.22f, 0.34f, 0.6f);

            Transform sweepGroup = CreateGroup(parent, "LightSweeps", new Vector2(0f, 0f), 0.02f, 0.002f, 0.08f, 0.95f);
            SpriteRenderer sweepLeft = CreateSolidPanel("SweepLeft", sweepGroup, new Vector2(-11f, 0.8f), new Vector2(4.2f, 18f), new Color(0.16f, 0.86f, 1f, 0.045f), -215);
            sweepLeft.transform.localEulerAngles = new Vector3(0f, 0f, 12f);
            SpriteRenderer sweepRight = CreateSolidPanel("SweepRight", sweepGroup, new Vector2(11f, -0.4f), new Vector2(4.2f, 18f), new Color(0.16f, 0.86f, 1f, 0.035f), -215);
            sweepRight.transform.localEulerAngles = new Vector3(0f, 0f, -12f);
            AddPulse(sweepLeft, sweepLeft.color, new Color(0.22f, 0.94f, 1f, 0.08f), 0.65f, 0.18f, 0.3f);
            AddPulse(sweepRight, sweepRight.color, new Color(0.18f, 0.84f, 1f, 0.07f), 0.6f, 0.16f, 0.95f);
        }

        private void BuildLevelSelectBackdrop(Transform parent, ScenePalette palette)
        {
            BuildFrames(parent, palette, 40f, 20f, 0.04f, 0.006f, 0.08f);
            BuildColumns(parent, palette, new[] { -16f, -9f, -2.5f, 4.5f, 11f, 17f }, 0.08f, 0.004f, 0.05f, 0.4f);

            Transform selectGroup = CreateGroup(parent, "LevelSelectCore", new Vector2(0f, 0.25f), 0.03f, 0.003f, 0.03f, 1.2f);
            CreateSolidPanel("SelectSpine", selectGroup, new Vector2(0f, 0.7f), new Vector2(18f, 12.4f), palette.WindowPane * 0.9f, -209);
            CreateSolidPanel("SelectSpineTop", selectGroup, new Vector2(0f, 6.85f), new Vector2(20.5f, 0.4f), palette.FrameDark, -208);
            CreateSolidPanel("SelectSpineBottom", selectGroup, new Vector2(0f, -5.7f), new Vector2(20.5f, 0.35f), palette.FrameDark * 0.92f, -208);
            SpriteRenderer selectAccent = CreateSolidPanel("SelectAccent", selectGroup, new Vector2(0f, 6.15f), new Vector2(13f, 0.08f), palette.AccentDim, -207);
            AddPulse(selectAccent, palette.AccentDim, palette.AccentPulse, 0.14f, 0.22f, 0.4f);
        }

        private void BuildFrames(Transform parent, ScenePalette palette, float width, float height, float parallaxX, float parallaxY, float motionAmplitude)
        {
            Transform frameGroup = CreateGroup(parent, "Frames", Vector2.zero, parallaxX, parallaxY, motionAmplitude, 0.32f);

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            CreateSolidPanel("LeftFrame", frameGroup, new Vector2(-halfWidth, 0.25f), new Vector2(1.1f, height + 2f), palette.FrameDark, -220);
            CreateSolidPanel("RightFrame", frameGroup, new Vector2(halfWidth, 0.25f), new Vector2(1.1f, height + 2f), palette.FrameDark, -220);
            CreateSolidPanel("TopFrame", frameGroup, new Vector2(0f, halfHeight - 0.2f), new Vector2(width + 2f, 0.9f), palette.FrameDark, -220);
            CreateSolidPanel("BottomFrame", frameGroup, new Vector2(0f, -halfHeight + 0.4f), new Vector2(width + 2f, 0.7f), palette.FrameDark, -220);

            SpriteRenderer topAccent = CreateSolidPanel("TopAccent", frameGroup, new Vector2(0f, halfHeight - 0.85f), new Vector2(width - 3f, 0.12f), palette.AccentSoft, -218);
            AddPulse(topAccent, palette.AccentSoft, palette.AccentPulse, 0.22f, 0.4f, 0.15f);

            SpriteRenderer bottomAccent = CreateSolidPanel("BottomAccent", frameGroup, new Vector2(0f, -halfHeight + 1f), new Vector2(width - 7f, 0.08f), palette.AccentSoft * 0.9f, -218);
            AddPulse(bottomAccent, palette.AccentSoft * 0.9f, palette.AccentPulse, 0.16f, 0.32f, 0.45f);
        }

        private void BuildColumns(Transform parent, ScenePalette palette, float[] positions, float parallaxX, float parallaxY, float motionAmplitude, float phaseOffset)
        {
            Transform columnsGroup = CreateGroup(parent, "Columns", Vector2.zero, parallaxX, parallaxY, motionAmplitude, phaseOffset);

            for (int i = 0; i < positions.Length; i++)
            {
                float x = positions[i];
                float height = (i % 2 == 0) ? 14.5f : 12.25f;
                CreateSolidPanel($"Column_{i}", columnsGroup, new Vector2(x, -0.3f), new Vector2(0.9f, height), palette.ColumnDark, -216);
                SpriteRenderer innerStrip = CreateSolidPanel($"ColumnAccent_{i}", columnsGroup, new Vector2(x, 0.1f), new Vector2(0.12f, height - 2.4f), palette.AccentDim, -214);
                AddPulse(innerStrip, palette.AccentDim, palette.AccentPulse, 0.18f, 0.55f, 0.2f * i);
            }
        }

        private void BuildWindows(Transform parent, ScenePalette palette, float[] widths, float[] positions, float parallaxX, float parallaxY, float motionAmplitude, float phaseOffset)
        {
            Transform windowGroup = CreateGroup(parent, "Windows", new Vector2(0f, 1.6f), parallaxX, parallaxY, motionAmplitude, phaseOffset);

            for (int i = 0; i < widths.Length; i++)
            {
                CreateSolidPanel($"WindowFrame_{i}", windowGroup, new Vector2(positions[i], 2.6f), new Vector2(widths[i], 3.2f), palette.FrameDark * 1.06f, -212);
                SpriteRenderer pane = CreateSolidPanel($"WindowPane_{i}", windowGroup, new Vector2(positions[i], 2.6f), new Vector2(widths[i] - 0.5f, 2.7f), palette.WindowPane, -210);
                AddPulse(pane, palette.WindowPane, palette.WindowPulse, 0.16f, 0.22f, 0.33f * i);

                CreateSolidPanel($"WindowLine_{i}", windowGroup, new Vector2(positions[i], 2.6f), new Vector2(0.08f, 2.7f), palette.FrameLine, -209);
                CreateSolidPanel($"WindowLineTop_{i}", windowGroup, new Vector2(positions[i], 3.55f), new Vector2(widths[i] - 0.7f, 0.06f), palette.FrameLine * 0.85f, -209);
            }
        }

        private void BuildRearPlatforms(Transform parent, ScenePalette palette, float parallaxX, float parallaxY, float motionAmplitude, float phaseOffset)
        {
            Transform rearGroup = CreateGroup(parent, "RearPlatforms", new Vector2(0f, -2.1f), parallaxX, parallaxY, motionAmplitude, phaseOffset);

            CreateSolidPanel("RearWalkwayLeft", rearGroup, new Vector2(-12f, -1.1f), new Vector2(8.5f, 0.42f), palette.PlatformDark, -208);
            CreateSolidPanel("RearWalkwayMid", rearGroup, new Vector2(0f, -0.25f), new Vector2(10.5f, 0.38f), palette.PlatformDark, -208);
            CreateSolidPanel("RearWalkwayRight", rearGroup, new Vector2(11.5f, -1.45f), new Vector2(7.8f, 0.42f), palette.PlatformDark, -208);

            SpriteRenderer railA = CreateSolidPanel("RearRailA", rearGroup, new Vector2(-12f, -0.8f), new Vector2(8.8f, 0.06f), palette.AccentDim, -206);
            SpriteRenderer railB = CreateSolidPanel("RearRailB", rearGroup, new Vector2(0f, 0.05f), new Vector2(10.8f, 0.06f), palette.AccentDim, -206);
            SpriteRenderer railC = CreateSolidPanel("RearRailC", rearGroup, new Vector2(11.5f, -1.15f), new Vector2(8.1f, 0.06f), palette.AccentDim, -206);

            AddPulse(railA, palette.AccentDim, palette.AccentPulse, 0.15f, 0.3f, 0.1f);
            AddPulse(railB, palette.AccentDim, palette.AccentPulse, 0.15f, 0.3f, 0.4f);
            AddPulse(railC, palette.AccentDim, palette.AccentPulse, 0.15f, 0.3f, 0.7f);
        }

        private Transform CreateLayer(string name)
        {
            GameObject layerObject = new GameObject(name);
            layerObject.transform.SetParent(transform, false);
            layerObject.transform.localPosition = Vector3.zero;
            return layerObject.transform;
        }

        private Transform CreateGroup(Transform parent, string name, Vector2 localPosition, float parallaxX, float parallaxY, float motionAmplitude, float phaseOffset)
        {
            GameObject groupObject = new GameObject(name);
            groupObject.transform.SetParent(parent, false);
            groupObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);

            parallaxElements.Add(new ParallaxElement
            {
                Transform = groupObject.transform,
                BaseLocalPosition = groupObject.transform.localPosition,
                ParallaxX = parallaxX,
                ParallaxY = parallaxY,
                MotionAmplitude = motionAmplitude,
                MotionSpeed = 0.18f,
                PhaseOffset = phaseOffset
            });

            return groupObject.transform;
        }

        private void CreateGradient(Transform parent, Color topColor, Color bottomColor, Vector2 size, int sortingOrder)
        {
            GameObject gradientObject = new GameObject("FarGradient");
            gradientObject.transform.SetParent(parent, false);
            gradientObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            gradientObject.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = gradientObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateGradientSprite(topColor, bottomColor);
            renderer.sortingOrder = sortingOrder;
        }

        private SpriteRenderer CreateSolidPanel(string name, Transform parent, Vector2 localPosition, Vector2 size, Color color, int sortingOrder)
        {
            GameObject panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent, false);
            panelObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            panelObject.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = panelObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSolidSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void AddPulse(SpriteRenderer renderer, Color baseColor, Color targetColor, float intensity, float speed, float phaseOffset)
        {
            pulseElements.Add(new PulseElement
            {
                Renderer = renderer,
                BaseColor = baseColor,
                TargetColor = targetColor,
                Intensity = intensity,
                Speed = speed,
                PhaseOffset = phaseOffset
            });
        }

        private static Sprite GetSolidSprite()
        {
            if (solidSprite != null)
            {
                return solidSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "EnvironmentBackdropSolid"
            };

            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            solidSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            solidSprite.name = "EnvironmentBackdropSolidSprite";
            return solidSprite;
        }

        private static Sprite CreateGradientSprite(Color topColor, Color bottomColor)
        {
            const int width = 2;
            const int height = 64;

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "EnvironmentBackdropGradient"
            };

            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                Color rowColor = Color.Lerp(bottomColor, topColor, t);
                texture.SetPixel(0, y, rowColor);
                texture.SetPixel(1, y, rowColor);
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 1f);
            sprite.name = "EnvironmentBackdropGradientSprite";
            return sprite;
        }

        private static BackdropMode GetMode(string sceneName)
        {
            if (sceneName == SceneRegistry.MainMenu)
            {
                return BackdropMode.MainMenu;
            }

            if (sceneName == SceneRegistry.LevelSelect)
            {
                return BackdropMode.LevelSelect;
            }

            return BackdropMode.Gameplay;
        }

        private static Vector2 GetSceneSize(BackdropMode mode)
        {
            return mode switch
            {
                BackdropMode.MainMenu => new Vector2(96f, 54f),
                BackdropMode.LevelSelect => new Vector2(88f, 50f),
                _ => new Vector2(80f, 48f)
            };
        }

        private static ScenePalette GetPalette(string sceneName)
        {
            ScenePalette basePalette = new ScenePalette
            {
                FarTop = new Color(0.02f, 0.03f, 0.06f, 1f),
                FarBottom = new Color(0.04f, 0.08f, 0.12f, 1f),
                FloorShadow = new Color(0.01f, 0.02f, 0.03f, 0.92f),
                CeilingShadow = new Color(0.01f, 0.02f, 0.04f, 0.85f),
                FrameDark = new Color(0.08f, 0.13f, 0.19f, 0.82f),
                ColumnDark = new Color(0.06f, 0.11f, 0.16f, 0.72f),
                PlatformDark = new Color(0.08f, 0.12f, 0.18f, 0.66f),
                WindowPane = new Color(0.08f, 0.19f, 0.26f, 0.28f),
                FrameLine = new Color(0.12f, 0.42f, 0.56f, 0.18f),
                AccentDim = new Color(0.1f, 0.45f, 0.62f, 0.12f),
                AccentSoft = new Color(0.14f, 0.62f, 0.82f, 0.16f),
                AccentPulse = new Color(0.22f, 0.88f, 1f, 0.32f),
                WindowPulse = new Color(0.18f, 0.75f, 0.92f, 0.42f)
            };

            switch (sceneName)
            {
                case SceneRegistry.MainMenu:
                    basePalette.FarTop = new Color(0.015f, 0.025f, 0.05f, 1f);
                    basePalette.FarBottom = new Color(0.03f, 0.07f, 0.11f, 1f);
                    basePalette.FrameDark = new Color(0.08f, 0.15f, 0.22f, 0.9f);
                    basePalette.WindowPane = new Color(0.08f, 0.18f, 0.28f, 0.32f);
                    basePalette.AccentSoft = new Color(0.2f, 0.76f, 0.94f, 0.22f);
                    basePalette.AccentPulse = new Color(0.34f, 0.95f, 1f, 0.38f);
                    basePalette.WindowPulse = new Color(0.24f, 0.88f, 1f, 0.48f);
                    break;
                case SceneRegistry.LevelSelect:
                    basePalette.FarTop = new Color(0.018f, 0.028f, 0.055f, 1f);
                    basePalette.FarBottom = new Color(0.032f, 0.062f, 0.1f, 1f);
                    basePalette.FrameDark = new Color(0.075f, 0.13f, 0.2f, 0.86f);
                    basePalette.WindowPane = new Color(0.07f, 0.15f, 0.24f, 0.24f);
                    basePalette.AccentSoft = new Color(0.16f, 0.62f, 0.82f, 0.14f);
                    basePalette.AccentPulse = new Color(0.24f, 0.82f, 1f, 0.24f);
                    basePalette.WindowPulse = new Color(0.18f, 0.72f, 0.92f, 0.32f);
                    break;
                case SceneRegistry.ButtonDoor:
                    basePalette.AccentSoft = new Color(0.14f, 0.7f, 0.9f, 0.18f);
                    basePalette.WindowPane = new Color(0.06f, 0.16f, 0.25f, 0.3f);
                    break;
                case SceneRegistry.HazardTiming:
                    basePalette.FarBottom = new Color(0.05f, 0.06f, 0.1f, 1f);
                    basePalette.AccentSoft = new Color(0.2f, 0.46f, 0.7f, 0.14f);
                    basePalette.AccentPulse = new Color(0.3f, 0.7f, 0.95f, 0.26f);
                    break;
                case SceneRegistry.Final:
                    basePalette.FarTop = new Color(0.015f, 0.025f, 0.055f, 1f);
                    basePalette.FarBottom = new Color(0.035f, 0.07f, 0.11f, 1f);
                    basePalette.FrameDark = new Color(0.09f, 0.14f, 0.21f, 0.86f);
                    basePalette.AccentSoft = new Color(0.18f, 0.72f, 0.88f, 0.18f);
                    basePalette.AccentPulse = new Color(0.36f, 0.94f, 1f, 0.34f);
                    break;
            }

            return basePalette;
        }

        private sealed class ParallaxElement
        {
            public Transform Transform;
            public Vector3 BaseLocalPosition;
            public float ParallaxX;
            public float ParallaxY;
            public float MotionAmplitude;
            public float MotionSpeed;
            public float PhaseOffset;
        }

        private sealed class PulseElement
        {
            public SpriteRenderer Renderer;
            public Color BaseColor;
            public Color TargetColor;
            public float Intensity;
            public float Speed;
            public float PhaseOffset;
        }

        private struct ScenePalette
        {
            public Color FarTop;
            public Color FarBottom;
            public Color FloorShadow;
            public Color CeilingShadow;
            public Color FrameDark;
            public Color ColumnDark;
            public Color PlatformDark;
            public Color WindowPane;
            public Color FrameLine;
            public Color AccentDim;
            public Color AccentSoft;
            public Color AccentPulse;
            public Color WindowPulse;
        }

        private struct GameplayBackdropPreset
        {
            public string SceneName;
            public float FrameWidth;
            public float FrameHeight;
            public float FrameParallaxX;
            public float FrameParallaxY;
            public float FrameMotionAmplitude;
            public float[] ColumnPositions;
            public float ColumnParallaxX;
            public float ColumnParallaxY;
            public float ColumnMotionAmplitude;
            public float ColumnPhaseOffset;
            public float[] WindowWidths;
            public float[] WindowPositions;
            public float WindowParallaxX;
            public float WindowParallaxY;
            public float WindowMotionAmplitude;
            public float WindowPhaseOffset;
            public float RearParallaxX;
            public float RearParallaxY;
            public float RearMotionAmplitude;
            public float RearPhaseOffset;
            public float ExtraAccentIntensity;
            public float ExtraAccentSpeed;
        }
    }
}
