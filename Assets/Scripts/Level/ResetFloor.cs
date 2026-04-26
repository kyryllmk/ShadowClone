using ShadowClone.Core;
using ShadowClone.Gameplay;
using ShadowClone.Presentation;
using UnityEngine;

namespace ShadowClone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class ResetFloor : MonoBehaviour
    {
        [SerializeField] private RoomResetManager roomResetManager;
        [SerializeField] private string resetReason = "Fall reset";
        [SerializeField] private Color surfaceColor = new Color(1f, 0.18f, 0.02f, 0.82f);
        [SerializeField] private Color pulseColor = new Color(1f, 0.58f, 0.04f, 0.92f);
        [SerializeField] private Color glowColor = new Color(1f, 0.08f, 0.01f, 0.28f);
        [SerializeField] private float pulseSpeed = 6.5f;
        [SerializeField] private float lavaLakeDepth = 4.5f;

        private Collider2D resetCollider;
        private SpriteRenderer surfaceRenderer;
        private SpriteRenderer lakeRenderer;
        private SpriteRenderer glowRenderer;
        private SpriteRenderer hotCoreRenderer;
        private float visualWidth = 80f;
        private float visualHeight = 0.75f;
        private static Sprite runtimeWhiteSprite;

        private void Awake()
        {
            resetCollider = GetComponent<Collider2D>();
            resetCollider.isTrigger = true;

            if (roomResetManager == null)
            {
                roomResetManager = FindObjectOfType<RoomResetManager>();
            }

            ConfigureHazardFallback();
            EnsureVisuals();
        }

        private void Update()
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float beatPulse = PresentationFeedbackBootstrap.BeatPulse01;
            float combinedPulse = Mathf.Clamp01(pulse * 0.55f + beatPulse * 0.45f);

            if (surfaceRenderer != null)
            {
                surfaceRenderer.color = Color.Lerp(surfaceColor, pulseColor, combinedPulse);
            }

            if (lakeRenderer != null)
            {
                lakeRenderer.color = Color.Lerp(new Color(0.58f, 0.02f, 0f, 0.82f), new Color(1f, 0.2f, 0f, 0.9f), combinedPulse * 0.55f);
            }

            if (glowRenderer != null)
            {
                glowRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, Mathf.Lerp(0.18f, 0.36f, combinedPulse));
                glowRenderer.transform.localScale = new Vector3(visualWidth * 1.02f, lavaLakeDepth + visualHeight * Mathf.Lerp(1.9f, 2.35f, combinedPulse), 1f);
            }

            if (hotCoreRenderer != null)
            {
                hotCoreRenderer.color = new Color(1f, Mathf.Lerp(0.34f, 0.72f, combinedPulse), 0.04f, 0.9f);
                hotCoreRenderer.transform.localScale = new Vector3(visualWidth, visualHeight * Mathf.Lerp(0.2f, 0.32f, combinedPulse), 1f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryReset(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryReset(collision.collider);
        }

        public void Configure(RoomResetManager manager)
        {
            roomResetManager = manager;
            if (resetCollider == null)
            {
                resetCollider = GetComponent<Collider2D>();
            }

            if (resetCollider != null)
            {
                resetCollider.isTrigger = true;
            }

            ConfigureHazardFallback();
            EnsureVisuals();
        }

        private void TryReset(Collider2D other)
        {
            if (roomResetManager == null || other == null)
            {
                return;
            }

            if (other.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }

            roomResetManager.RequestReset(resetReason);
        }

        private void ConfigureHazardFallback()
        {
            Hazard hazard = GetComponent<Hazard>();
            if (hazard != null)
            {
                hazard.ConfigureReset(roomResetManager, resetReason);
            }
        }

        private void EnsureVisuals()
        {
            DestroyLegacyVisual("ResetFloor_LavaGlow");
            DestroyLegacyVisual("ResetFloor_HotCore");
            DestroyLegacyVisual("ResetFloor_LavaSurface");

            SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
            if (rootRenderer != null)
            {
                rootRenderer.enabled = false;
            }

            if (resetCollider == null)
            {
                resetCollider = GetComponent<Collider2D>();
            }

            if (resetCollider is BoxCollider2D boxCollider)
            {
                visualWidth = Mathf.Max(1f, boxCollider.size.x);
                visualHeight = Mathf.Max(0.1f, boxCollider.size.y);
            }

            surfaceRenderer = EnsureChildRenderer("LavaSurface", Vector3.zero, new Vector3(visualWidth, visualHeight, 1f), surfaceColor, -1);
            lakeRenderer = EnsureChildRenderer("LavaLake", new Vector3(0f, -lavaLakeDepth * 0.5f, 0.03f), new Vector3(visualWidth, lavaLakeDepth, 1f), new Color(0.58f, 0.02f, 0f, 0.82f), -3);
            glowRenderer = EnsureChildRenderer("LavaGlow", new Vector3(0f, -lavaLakeDepth * 0.38f, 0.04f), new Vector3(visualWidth * 1.02f, lavaLakeDepth + visualHeight * 2.05f, 1f), glowColor, -2);
            hotCoreRenderer = EnsureChildRenderer("LavaHotCore", new Vector3(0f, 0.24f, 0.05f), new Vector3(visualWidth, visualHeight * 0.26f, 1f), new Color(1f, 0.46f, 0.04f, 0.9f), 0);
        }

        private SpriteRenderer EnsureChildRenderer(string childName, Vector3 localPosition, Vector3 localScale, Color color, int sortingOrder)
        {
            Transform existing = transform.Find(childName);
            GameObject child = existing != null ? existing.gameObject : new GameObject(childName);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = localPosition;
            child.transform.localScale = localScale;

            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = child.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = GetRuntimeWhiteSprite();
            renderer.color = color;
            renderer.sortingOrder = surfaceRenderer != null ? surfaceRenderer.sortingOrder + sortingOrder : sortingOrder;
            return renderer;
        }

        private void DestroyLegacyVisual(string childName)
        {
            Transform legacy = transform.Find(childName);
            if (legacy != null)
            {
                Destroy(legacy.gameObject);
            }
        }

        private static Sprite GetRuntimeWhiteSprite()
        {
            if (runtimeWhiteSprite != null)
            {
                return runtimeWhiteSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "ResetFloorWhitePixel"
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            runtimeWhiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            runtimeWhiteSprite.name = "ResetFloorWhiteSprite";
            return runtimeWhiteSprite;
        }
    }
}
