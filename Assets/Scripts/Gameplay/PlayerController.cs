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

        [Header("Physics")]
        [SerializeField] private bool applyNoFrictionMaterial = true;

        private Rigidbody2D body;
        private float moveInput;
        private bool wasGrounded;
        private bool hasJumpAvailable;
        private PhysicsMaterial2D noFrictionMaterial;

        public bool IsGrounded => groundCheck != null && groundCheck.IsGrounded;
        public bool IsMovementLocked { get; private set; }
        public Vector2 Velocity => body != null ? body.velocity : Vector2.zero;
        public float FacingDirection { get; private set; } = 1f;
        public event System.Action Jumped;
        public event System.Action Landed;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ApplyNoFrictionMaterial();
        }

        private void Start()
        {
            wasGrounded = IsGrounded;
            hasJumpAvailable = wasGrounded;
        }

        private void Update()
        {
            if (IsMovementLocked)
            {
                moveInput = 0f;
                return;
            }

            moveInput = Input.GetAxisRaw("Horizontal");
            bool isGroundedNow = IsGrounded;

            if (moveInput > 0.01f)
            {
                FacingDirection = 1f;
            }
            else if (moveInput < -0.01f)
            {
                FacingDirection = -1f;
            }

            if (!wasGrounded && isGroundedNow)
            {
                hasJumpAvailable = true;
                Landed?.Invoke();
            }

            if (Input.GetButtonDown("Jump") && hasJumpAvailable && isGroundedNow)
            {
                body.velocity = new Vector2(body.velocity.x, jumpForce);
                hasJumpAvailable = false;
                Jumped?.Invoke();
                wasGrounded = false;
                return;
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
            wasGrounded = false;
            hasJumpAvailable = false;
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

        private void ApplyNoFrictionMaterial()
        {
            if (!applyNoFrictionMaterial)
            {
                return;
            }

            noFrictionMaterial = new PhysicsMaterial2D("Player_NoFriction_Runtime")
            {
                friction = 0f,
                bounciness = 0f
            };

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D playerCollider = colliders[i];
                if (playerCollider == null || playerCollider.isTrigger)
                {
                    continue;
                }

                playerCollider.sharedMaterial = noFrictionMaterial;
            }
        }
    }
}
