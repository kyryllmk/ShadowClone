using ShadowClone.Clone;
using ShadowClone.Level;
using UnityEngine;

namespace ShadowClone.Gameplay
{
    public class GroundCheck : MonoBehaviour
    {
        [SerializeField] private Transform checkPoint;
        [SerializeField] private Vector2 boxSize = new Vector2(0.68f, 0.06f);
        [SerializeField] private float castDistance = 0.12f;
        [SerializeField, Range(0f, 1f)] private float minimumGroundNormalY = 0.7f;
        [SerializeField] private LayerMask groundLayers = Physics2D.DefaultRaycastLayers;

        private readonly RaycastHit2D[] hitResults = new RaycastHit2D[8];
        private readonly ContactPoint2D[] contactResults = new ContactPoint2D[16];
        private Rigidbody2D ownerBody;

        public bool IsGrounded
        {
            get
            {
                if (HasValidGroundContact())
                {
                    return true;
                }

                Transform target = checkPoint != null ? checkPoint : transform;
                Vector2 origin = target.position;
                Vector2 size = new Vector2(Mathf.Max(0.01f, boxSize.x), Mathf.Max(0.01f, boxSize.y));
                float distance = Mathf.Max(0.01f, castDistance);

                int hitCount = Physics2D.BoxCastNonAlloc(
                    origin,
                    size,
                    0f,
                    Vector2.down,
                    hitResults,
                    distance,
                    groundLayers);
                Transform ownerRoot = transform.root;

                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit2D hit = hitResults[i];
                    Collider2D hitCollider = hit.collider;
                    if (!IsValidGroundCollider(hitCollider, ownerRoot))
                    {
                        continue;
                    }

                    if (hit.normal.y < minimumGroundNormalY)
                    {
                        continue;
                    }

                    return true;
                }

                return false;
            }
        }

        private void Awake()
        {
            ownerBody = GetComponentInParent<Rigidbody2D>();
        }

        private bool HasValidGroundContact()
        {
            if (ownerBody == null)
            {
                ownerBody = GetComponentInParent<Rigidbody2D>();
            }

            if (ownerBody == null)
            {
                return false;
            }

            int contactCount = ownerBody.GetContacts(contactResults);
            Transform ownerRoot = transform.root;

            for (int i = 0; i < contactCount; i++)
            {
                ContactPoint2D contact = contactResults[i];
                if (!IsValidGroundCollider(contact.collider, ownerRoot))
                {
                    continue;
                }

                if (contact.normal.y >= minimumGroundNormalY)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsValidGroundCollider(Collider2D hitCollider, Transform ownerRoot)
        {
            if (hitCollider == null || hitCollider.isTrigger || hitCollider.transform.IsChildOf(ownerRoot))
            {
                return false;
            }

            int layerMask = 1 << hitCollider.gameObject.layer;
            if ((groundLayers.value & layerMask) == 0)
            {
                return false;
            }

            if (hitCollider.GetComponentInParent<CloneActor>() != null ||
                hitCollider.GetComponentInParent<ResetFloor>() != null ||
                hitCollider.GetComponentInParent<Hazard>() != null ||
                hitCollider.GetComponentInParent<PuzzleButton>() != null ||
                hitCollider.GetComponentInParent<DoorController>() != null)
            {
                return false;
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Transform target = checkPoint != null ? checkPoint : transform;
            Vector3 origin = target.position;
            Vector3 size = new Vector3(Mathf.Max(0.01f, boxSize.x), Mathf.Max(0.01f, boxSize.y), 0f);
            Vector3 castOffset = Vector3.down * Mathf.Max(0.01f, castDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(origin, size);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(origin + castOffset, size);
        }
    }
}
