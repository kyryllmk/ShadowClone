using ShadowClone.Core;
using ShadowClone.Gameplay;
using UnityEngine;

namespace ShadowClone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class Hazard : MonoBehaviour
    {
        [SerializeField] private RoomResetManager roomResetManager;
        [SerializeField] private string resetReason = "Hazard hit";
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color baseColor = new Color(1f, 0.45f, 0.45f, 1f);
        [SerializeField] private Color pulseColor = new Color(1f, 0.8f, 0.35f, 1f);
        [SerializeField] private float pulseSpeed = 6f;

        private Collider2D triggerCollider;
        public event System.Action Triggered;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
        }

        private void Update()
        {
            if (targetRenderer == null)
            {
                return;
            }

            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            targetRenderer.color = Color.Lerp(baseColor, pulseColor, pulse);
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
            if (roomResetManager == null)
            {
                return;
            }

            if (other.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }

            Triggered?.Invoke();
            roomResetManager.RequestReset(resetReason);
        }
    }
}
