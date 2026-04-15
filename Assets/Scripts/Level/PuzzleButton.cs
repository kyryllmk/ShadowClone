using System;
using System.Collections.Generic;
using ShadowClone.Clone;
using ShadowClone.Gameplay;
using UnityEngine;

namespace ShadowClone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class PuzzleButton : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color inactiveColor = new Color(0.55f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color activeColor = new Color(0.35f, 0.9f, 0.45f, 1f);
        [SerializeField] private bool reactToPlayer = true;
        [SerializeField] private bool reactToClone = true;

        private readonly HashSet<Collider2D> occupants = new HashSet<Collider2D>();
        private Collider2D triggerCollider;

        public event Action<bool> PressedStateChanged;

        public bool IsPressed => occupants.Count > 0;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
            UpdateVisual();
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
            if (!IsValidActivator(other))
            {
                return;
            }

            bool wasPressed = IsPressed;
            occupants.Add(other);

            if (wasPressed != IsPressed)
            {
                HandlePressedStateChanged();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!occupants.Remove(other))
            {
                return;
            }

            HandlePressedStateChanged();
        }

        public void ClearState()
        {
            if (occupants.Count == 0)
            {
                return;
            }

            occupants.Clear();
            HandlePressedStateChanged();
        }

        private bool IsValidActivator(Collider2D other)
        {
            if (other == null)
            {
                return false;
            }

            if (reactToPlayer && other.GetComponentInParent<PlayerController>() != null)
            {
                return true;
            }

            if (reactToClone && other.GetComponentInParent<CloneActor>() != null)
            {
                return true;
            }

            return false;
        }

        private void HandlePressedStateChanged()
        {
            UpdateVisual();
            PressedStateChanged?.Invoke(IsPressed);
        }

        private void UpdateVisual()
        {
            if (targetRenderer != null)
            {
                targetRenderer.color = IsPressed ? activeColor : inactiveColor;
            }
        }
    }
}
