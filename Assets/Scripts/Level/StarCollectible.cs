using ShadowClone.Gameplay;
using ShadowClone.Presentation;
using UnityEngine;

namespace ShadowClone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class StarCollectible : MonoBehaviour
    {
        [SerializeField] private int starIndex;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color starColor = new Color(1f, 0.92f, 0.24f, 1f);
        [SerializeField] private Color glowColor = new Color(0.3f, 1f, 0.95f, 0.28f);

        private static Sprite starSprite;
        private Collider2D triggerCollider;
        private SpriteRenderer glowRenderer;
        private Vector3 baseScale;
        private bool isCollected;
        private float collectPulseTimer;

        public int StarIndex => starIndex;
        public bool IsCollected => isCollected;
        public event System.Action<StarCollectible> Collected;

        private void Awake()
        {
            baseScale = transform.localScale;
            triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;

            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = GetStarSprite();
            spriteRenderer.color = starColor;
            spriteRenderer.sortingOrder = 12;
            CreateGlowRenderer();
        }

        private void Update()
        {
            if (isCollected)
            {
                collectPulseTimer -= Time.deltaTime;
                float pulse = Mathf.Clamp01(collectPulseTimer / 0.14f);
                transform.localScale = baseScale * Mathf.Lerp(1.45f, 0.4f, 1f - pulse);
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(starColor.r, starColor.g, starColor.b, pulse);
                }

                if (glowRenderer != null)
                {
                    glowRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * pulse);
                }

                if (collectPulseTimer <= 0f)
                {
                    gameObject.SetActive(false);
                }

                return;
            }

            float beatPulse = PresentationFeedbackBootstrap.BeatPulse01;
            float hover = Mathf.Sin(Time.time * 3.2f + starIndex) * 0.08f;
            transform.localScale = baseScale * (1f + beatPulse * 0.08f + hover);
            transform.Rotate(0f, 0f, 45f * Time.deltaTime);

            if (glowRenderer != null)
            {
                glowRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a + beatPulse * 0.12f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected || other.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }

            Collect();
        }

        public void Configure(int index)
        {
            starIndex = index;
        }

        public void RestoreForRun()
        {
            isCollected = false;
            collectPulseTimer = 0f;
            gameObject.SetActive(true);
            transform.localScale = baseScale;

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = starColor;
            }

            if (glowRenderer != null)
            {
                glowRenderer.enabled = true;
                glowRenderer.color = glowColor;
            }
        }

        private void Collect()
        {
            isCollected = true;
            collectPulseTimer = 0.14f;
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            PresentationFeedbackBootstrap.PlayStarCollected();
            Collected?.Invoke(this);
        }

        private void OnEnable()
        {
            if (triggerCollider != null)
            {
                triggerCollider.enabled = !isCollected;
            }
        }

        private void CreateGlowRenderer()
        {
            if (spriteRenderer == null || glowRenderer != null)
            {
                return;
            }

            GameObject glowObject = new GameObject("StarGlow");
            glowObject.transform.SetParent(transform, false);
            glowObject.transform.localScale = Vector3.one * 1.42f;

            glowRenderer = glowObject.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = spriteRenderer.sprite;
            glowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
            glowRenderer.color = glowColor;
        }

        private static Sprite GetStarSprite()
        {
            if (starSprite != null)
            {
                return starSprite;
            }

            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "CollectibleStarTexture"
            };

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            Vector2[] points = BuildStarPoints(center, 28f, 12f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = IsPointInPolygon(new Vector2(x, y), points);
                    texture.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            starSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 64f);
            starSprite.name = "CollectibleStarSprite";
            return starSprite;
        }

        private static Vector2[] BuildStarPoints(Vector2 center, float outerRadius, float innerRadius)
        {
            Vector2[] points = new Vector2[10];
            for (int i = 0; i < points.Length; i++)
            {
                float radius = i % 2 == 0 ? outerRadius : innerRadius;
                float angle = Mathf.Deg2Rad * (-90f + i * 36f);
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            return points;
        }

        private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                bool intersects = ((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
                    Mathf.Max(0.0001f, polygon[j].y - polygon[i].y) + polygon[i].x);

                if (intersects)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
