using UnityEngine;

namespace ShadowClone.Level
{
    public class DoorController : MonoBehaviour
    {
        [SerializeField] private PuzzleButton linkedButton;
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color closedColor = new Color(0.9f, 0.25f, 0.25f, 1f);
        [SerializeField] private Color openColor = new Color(0.35f, 0.95f, 0.55f, 0.35f);
        [SerializeField] private bool startOpen;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            ApplyState(startOpen);
        }

        private void OnEnable()
        {
            if (linkedButton != null)
            {
                linkedButton.PressedStateChanged += HandleButtonPressedStateChanged;
            }
        }

        private void Start()
        {
            if (linkedButton != null)
            {
                ApplyState(linkedButton.IsPressed);
            }
        }

        private void OnDisable()
        {
            if (linkedButton != null)
            {
                linkedButton.PressedStateChanged -= HandleButtonPressedStateChanged;
            }
        }

        public void SetLinkedButton(PuzzleButton button)
        {
            if (linkedButton != null)
            {
                linkedButton.PressedStateChanged -= HandleButtonPressedStateChanged;
            }

            linkedButton = button;

            if (isActiveAndEnabled && linkedButton != null)
            {
                linkedButton.PressedStateChanged += HandleButtonPressedStateChanged;
                ApplyState(linkedButton.IsPressed);
            }
        }

        public void ForceClosed()
        {
            ApplyState(false);
        }

        private void HandleButtonPressedStateChanged(bool isPressed)
        {
            ApplyState(isPressed);
        }

        private void ApplyState(bool shouldOpen)
        {
            IsOpen = shouldOpen;

            if (blockingCollider != null)
            {
                blockingCollider.enabled = !IsOpen;
            }

            if (targetRenderer != null)
            {
                targetRenderer.color = IsOpen ? openColor : closedColor;
            }
        }
    }
}
