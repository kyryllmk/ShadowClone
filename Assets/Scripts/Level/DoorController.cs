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
        [SerializeField] private float openScaleY = 0.2f;
        [SerializeField] private float transitionSpeed = 8f;

        private Vector3 baseScale;

        public bool IsOpen { get; private set; }
        public event System.Action<bool> StateChanged;

        private void Awake()
        {
            baseScale = transform.localScale;
            ApplyState(startOpen);
        }

        private void Update()
        {
            Vector3 targetScale = IsOpen
                ? new Vector3(baseScale.x, baseScale.y * openScaleY, baseScale.z)
                : baseScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, transitionSpeed * Time.deltaTime);

            if (targetRenderer != null)
            {
                Color targetColor = IsOpen ? openColor : closedColor;
                targetRenderer.color = Color.Lerp(targetRenderer.color, targetColor, transitionSpeed * Time.deltaTime);
            }
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
            bool wasOpen = IsOpen;
            IsOpen = shouldOpen;

            if (blockingCollider != null)
            {
                blockingCollider.enabled = !IsOpen;
            }

            if (!wasOpen.Equals(IsOpen))
            {
                StateChanged?.Invoke(IsOpen);
            }
        }
    }
}
