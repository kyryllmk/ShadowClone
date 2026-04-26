using UnityEngine;

namespace ShadowClone.Gameplay
{
    public class GroundCheck : MonoBehaviour
    {
        [SerializeField] private Transform checkPoint;
        [SerializeField] private float checkRadius = 0.15f;
        [SerializeField] private LayerMask groundLayers = Physics2D.DefaultRaycastLayers;

        private readonly Collider2D[] overlapResults = new Collider2D[8];

        public bool IsGrounded
        {
            get
            {
                Transform target = checkPoint != null ? checkPoint : transform;
                ContactFilter2D filter = new ContactFilter2D
                {
                    useLayerMask = true,
                    useTriggers = false
                };
                filter.SetLayerMask(groundLayers);

                int hitCount = Physics2D.OverlapCircle(target.position, checkRadius, filter, overlapResults);
                Transform ownerRoot = transform.root;
                for (int i = 0; i < hitCount; i++)
                {
                    Collider2D hit = overlapResults[i];
                    if (hit == null || hit.isTrigger || hit.transform.IsChildOf(ownerRoot))
                    {
                        continue;
                    }

                    return true;
                }

                return false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Transform target = checkPoint != null ? checkPoint : transform;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, checkRadius);
        }
    }
}
