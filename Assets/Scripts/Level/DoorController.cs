using UnityEngine;

namespace ShadowClone.Level
{
    public class DoorController : MonoBehaviour
    {
        [SerializeField] private PuzzleButton linkedButton;
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color closedColor = new Color(0.18f, 0.9f, 1f, 1f);
        [SerializeField] private Color openColor = new Color(0.58f, 1f, 1f, 0.32f);
        [SerializeField] private bool startOpen;
        [SerializeField] private float openScaleY = 0.2f;
        [SerializeField] private float transitionSpeed = 8f;

        private Vector3 baseScale;
        private SpriteRenderer glowRenderer;

        public bool IsOpen { get; private set; }
        public event System.Action<bool> StateChanged;

        private void Awake()
        {
            baseScale = transform.localScale;
            CreateGlowRenderer();
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

            if (glowRenderer != null)
            {
                Color glowColor = IsOpen ? new Color(0.56f, 1f, 1f, 0.18f) : new Color(0.18f, 0.9f, 1f, 0.28f);
                glowRenderer.color = Color.Lerp(glowRenderer.color, glowColor, transitionSpeed * Time.deltaTime);
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

        private void CreateGlowRenderer()
        {
            if (targetRenderer == null || glowRenderer != null)
            {
                return;
            }

            GameObject glowObject = new GameObject("DoorGlow");
            glowObject.transform.SetParent(targetRenderer.transform, false);
            glowObject.transform.localPosition = Vector3.zero;
            glowObject.transform.localScale = Vector3.one * 1.18f;

            glowRenderer = glowObject.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = targetRenderer.sprite;
            glowRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            glowRenderer.sortingOrder = targetRenderer.sortingOrder - 1;
            glowRenderer.color = new Color(0.18f, 0.9f, 1f, 0.28f);
        }
    }
}
