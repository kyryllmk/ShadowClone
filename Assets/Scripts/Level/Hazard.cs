using ShadowClone.Core;
using ShadowClone.Gameplay;
using ShadowClone.Presentation;
using UnityEngine;

namespace ShadowClone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class Hazard : MonoBehaviour
    {
        [SerializeField] private RoomResetManager roomResetManager;
        [SerializeField] private string resetReason = "Hazard hit";
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color baseColor = new Color(0.9f, 0.08f, 0.04f, 1f);
        [SerializeField] private Color pulseColor = new Color(1f, 0.42f, 0.02f, 1f);
        [SerializeField] private Color silhouetteColor = new Color(0.08f, 0.01f, 0.01f, 0.95f);
        [SerializeField] private float pulseSpeed = 8f;

        private Collider2D triggerCollider;
        private SpriteRenderer silhouetteRenderer;
        public event System.Action Triggered;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;

            if (roomResetManager == null)
            {
                roomResetManager = FindObjectOfType<RoomResetManager>();
            }

            CreateSilhouetteRenderer();
        }

        private void Update()
        {
            if (targetRenderer == null)
            {
                return;
            }

            float localPulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float pulse = Mathf.Clamp01(PresentationFeedbackBootstrap.BeatPulse01 + localPulse * 0.08f);
            targetRenderer.color = Color.Lerp(baseColor, pulseColor, pulse);
            targetRenderer.transform.localScale = new Vector3(1f + (pulse * 0.055f), 1f, 1f);
            if (silhouetteRenderer != null)
            {
                silhouetteRenderer.color = silhouetteColor;
            }
        }

        private void OnValidate()
        {
            Collider2D localCollider = GetComponent<Collider2D>();
            if (localCollider != null)
            {
                localCollider.isTrigger = true;
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

        public void ConfigureReset(RoomResetManager manager, string reason)
        {
            roomResetManager = manager;
            resetReason = reason;

            if (triggerCollider == null)
            {
                triggerCollider = GetComponent<Collider2D>();
            }

            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void TryReset(Collider2D other)
        {
            if (roomResetManager == null)
            {
                roomResetManager = FindObjectOfType<RoomResetManager>();
            }

            if (roomResetManager == null || other == null || other.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }

            Triggered?.Invoke();
            roomResetManager.RequestReset(resetReason);
        }

        private void CreateSilhouetteRenderer()
        {
            if (targetRenderer == null || silhouetteRenderer != null)
            {
                return;
            }

            GameObject silhouetteObject = new GameObject("HazardSilhouette");
            silhouetteObject.transform.SetParent(targetRenderer.transform, false);
            silhouetteObject.transform.localPosition = new Vector3(0f, -0.015f, 0f);
            silhouetteObject.transform.localScale = new Vector3(1.16f, 1.16f, 1f);

            silhouetteRenderer = silhouetteObject.AddComponent<SpriteRenderer>();
            silhouetteRenderer.sprite = targetRenderer.sprite;
            silhouetteRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            silhouetteRenderer.sortingOrder = targetRenderer.sortingOrder - 1;
            silhouetteRenderer.color = silhouetteColor;
        }
    }
}
