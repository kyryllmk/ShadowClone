using System;
using System.Collections.Generic;
using ShadowClone.Clone;
using ShadowClone.Gameplay;
using ShadowClone.Presentation;
using UnityEngine;

namespace ShadowClone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class PuzzleButton : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color inactiveColor = new Color(0.18f, 0.38f, 0.54f, 1f);
        [SerializeField] private Color activeColor = new Color(0.78f, 0.96f, 1f, 1f);
        [SerializeField] private bool reactToPlayer = true;
        [SerializeField] private bool reactToClone = true;
        [SerializeField] private float activePulseAmplitude = 0.08f;
        [SerializeField] private float activePulseSpeed = 4f;
        [SerializeField] private float pressedScaleY = 0.82f;

        private readonly HashSet<Collider2D> occupants = new HashSet<Collider2D>();
        private Collider2D triggerCollider;
        private Vector3 baseScale;
        private SpriteRenderer glowRenderer;

        public event Action<bool> PressedStateChanged;

        public bool IsPressed => occupants.Count > 0;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
            baseScale = transform.localScale;
            CreateGlowRenderer();
            UpdateVisual();
        }

        private void Update()
        {
            float targetY = IsPressed ? pressedScaleY : 1f;
            Vector3 targetScale = new Vector3(baseScale.x, baseScale.y * targetY, baseScale.z);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 10f * Time.deltaTime);

            if (targetRenderer == null)
            {
                return;
            }

            if (IsPressed)
            {
                float localPulse = (Mathf.Sin(Time.time * activePulseSpeed) + 1f) * 0.5f;
                float beatPulse = Mathf.Clamp01(PresentationFeedbackBootstrap.BeatPulse01 + localPulse * 0.08f);
                float pulse = 1f + beatPulse * activePulseAmplitude * 1.7f;
                targetRenderer.color = activeColor * pulse;
                targetRenderer.color = new Color(targetRenderer.color.r, targetRenderer.color.g, targetRenderer.color.b, 1f);
                if (glowRenderer != null)
                {
                    glowRenderer.color = new Color(activeColor.r, activeColor.g, activeColor.b, 0.2f + beatPulse * 0.24f);
                }
            }
            else
            {
                targetRenderer.color = Color.Lerp(targetRenderer.color, inactiveColor, 10f * Time.deltaTime);
                if (glowRenderer != null)
                {
                    float beatPulse = PresentationFeedbackBootstrap.BeatPulse01;
                    Color idleGlow = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0.06f + beatPulse * 0.035f);
                    glowRenderer.color = Color.Lerp(glowRenderer.color, idleGlow, 10f * Time.deltaTime);
                }
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

            if (glowRenderer != null)
            {
                Color color = IsPressed
                    ? new Color(activeColor.r, activeColor.g, activeColor.b, 0.22f)
                    : new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0.08f);
                glowRenderer.color = color;
            }
        }

        private void CreateGlowRenderer()
        {
            if (targetRenderer == null || glowRenderer != null)
            {
                return;
            }

            GameObject glowObject = new GameObject("ButtonGlow");
            glowObject.transform.SetParent(targetRenderer.transform, false);
            glowObject.transform.localPosition = Vector3.zero;
            glowObject.transform.localScale = Vector3.one * 1.18f;

            glowRenderer = glowObject.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = targetRenderer.sprite;
            glowRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            glowRenderer.sortingOrder = targetRenderer.sortingOrder - 1;
        }
    }
}
