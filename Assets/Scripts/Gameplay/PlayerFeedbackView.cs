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

        private Vector3 baseScale = Vector3.one;

        private void Awake()
        {
            if (playerController != null)
            {
                baseScale = playerController.transform.localScale;
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
            playerController.transform.localScale = nextScale;

            if (spriteRenderer == null)
            {
                return;
            }

            if (recordingController != null && recordingController.IsRecording)
            {
                spriteRenderer.color = recordingColor;
            }
            else if (!playerController.IsGrounded)
            {
                spriteRenderer.color = airborneColor;
            }
            else
            {
                spriteRenderer.color = idleColor;
            }
        }
    }
}
