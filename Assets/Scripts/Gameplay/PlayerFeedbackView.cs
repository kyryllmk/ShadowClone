using ShadowClone.Clone;
using UnityEngine;

namespace ShadowClone.Gameplay
{
    public class PlayerFeedbackView : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private RecordingController recordingController;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color idleColor = Color.white;
        [SerializeField] private Color airborneColor = new Color(0.9f, 0.95f, 1f, 1f);
        [SerializeField] private Color recordingColor = new Color(1f, 0.85f, 0.45f, 1f);
        [SerializeField] private float colorLerpSpeed = 8f;
        [SerializeField] private float impactRecoverSpeed = 10f;
        [SerializeField] private float jumpStretch = 0.12f;
        [SerializeField] private float landingSquash = 0.18f;

        private Vector3 baseScale = Vector3.one;
        private float stretchOffset;
        private float squashOffset;
        private Color currentColor = Color.white;

        private void Awake()
        {
            if (playerController != null)
            {
                baseScale = playerController.transform.localScale;
                playerController.Jumped += HandleJumped;
                playerController.Landed += HandleLanded;
            }

            if (spriteRenderer != null)
            {
                currentColor = spriteRenderer.color;
            }
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.Jumped -= HandleJumped;
                playerController.Landed -= HandleLanded;
            }
        }

        private void LateUpdate()
        {
            if (playerController == null)
            {
                return;
            }

            Vector3 nextScale = baseScale;
            nextScale.x = Mathf.Abs(baseScale.x) * playerController.FacingDirection;
            squashOffset = Mathf.MoveTowards(squashOffset, 0f, impactRecoverSpeed * Time.deltaTime);
            stretchOffset = Mathf.MoveTowards(stretchOffset, 0f, impactRecoverSpeed * Time.deltaTime);
            nextScale.y *= 1f + stretchOffset - squashOffset;
            nextScale.x *= 1f - stretchOffset + squashOffset;
            playerController.transform.localScale = nextScale;

            if (spriteRenderer == null)
            {
                return;
            }

            Color targetColor;
            if (recordingController != null && recordingController.IsRecording)
            {
                targetColor = recordingColor;
            }
            else if (!playerController.IsGrounded)
            {
                targetColor = airborneColor;
            }
            else
            {
                targetColor = idleColor;
            }

            currentColor = Color.Lerp(currentColor, targetColor, colorLerpSpeed * Time.deltaTime);
            spriteRenderer.color = currentColor;
        }

        private void HandleJumped()
        {
            stretchOffset = jumpStretch;
            squashOffset = 0f;
        }

        private void HandleLanded()
        {
            squashOffset = landingSquash;
            stretchOffset = 0f;
        }
    }
}
