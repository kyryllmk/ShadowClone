using ShadowClone.Core;
using ShadowClone.Gameplay;
using UnityEngine;

namespace ShadowClone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class ResetFloor : MonoBehaviour
    {
        [SerializeField] private RoomResetManager roomResetManager;
        [SerializeField] private string resetReason = "Out of bounds";

        private Collider2D resetCollider;

        private void Awake()
        {
            resetCollider = GetComponent<Collider2D>();
            resetCollider.isTrigger = true;

            if (roomResetManager == null)
            {
                roomResetManager = FindObjectOfType<RoomResetManager>();
            }

            ConfigureHazardFallback();
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
    }
}
