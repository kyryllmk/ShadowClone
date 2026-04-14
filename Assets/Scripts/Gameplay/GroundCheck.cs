using UnityEngine;

namespace ShadowClone.Gameplay
{
    public class GroundCheck : MonoBehaviour
    {
        [SerializeField] private Transform checkPoint;
        [SerializeField] private float checkRadius = 0.15f;
        [SerializeField] private LayerMask groundLayers = Physics2D.DefaultRaycastLayers;

        public bool IsGrounded
        {
            get
            {
                Transform target = checkPoint != null ? checkPoint : transform;
                return Physics2D.OverlapCircle(target.position, checkRadius, groundLayers) != null;
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
