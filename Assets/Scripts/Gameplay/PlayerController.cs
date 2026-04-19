using UnityEngine;

namespace ShadowClone.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float jumpForce = 13f;

        [Header("References")]
        [SerializeField] private GroundCheck groundCheck;

        private Rigidbody2D body;
        private float moveInput;
        private bool wasGrounded;

        public bool IsGrounded => groundCheck != null && groundCheck.IsGrounded;
        public bool IsMovementLocked { get; private set; }
        public Vector2 Velocity => body != null ? body.velocity : Vector2.zero;
        public float FacingDirection { get; private set; } = 1f;
        public event System.Action Jumped;
        public event System.Action Landed;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            wasGrounded = IsGrounded;
        }

        private void Update()
        {
            if (IsMovementLocked)
            {
                moveInput = 0f;
                return;
            }

            moveInput = Input.GetAxisRaw("Horizontal");

            if (moveInput > 0.01f)
            {
                FacingDirection = 1f;
            }
            else if (moveInput < -0.01f)
            {
                FacingDirection = -1f;
            }

            if (Input.GetButtonDown("Jump") && IsGrounded)
            {
                body.velocity = new Vector2(body.velocity.x, jumpForce);
                Jumped?.Invoke();
                wasGrounded = false;
            }

            bool isGroundedNow = IsGrounded;
            if (!wasGrounded && isGroundedNow)
            {
                Landed?.Invoke();
            }

            wasGrounded = isGroundedNow;
        }

        private void FixedUpdate()
        {
            body.velocity = new Vector2(moveInput * moveSpeed, body.velocity.y);
        }

        public void ResetToSpawn(Vector3 spawnPosition, Vector3 spawnScale)
        {
            transform.position = spawnPosition;
            transform.localScale = spawnScale;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            FacingDirection = Mathf.Sign(spawnScale.x == 0f ? 1f : spawnScale.x);
            moveInput = 0f;
        }

        public void SetMovementLocked(bool isLocked)
        {
            IsMovementLocked = isLocked;

            if (isLocked && body != null)
            {
                moveInput = 0f;
                body.velocity = new Vector2(0f, body.velocity.y);
            }
        }
    }
}
