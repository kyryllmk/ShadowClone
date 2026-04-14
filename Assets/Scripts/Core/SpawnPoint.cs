using UnityEngine;

namespace ShadowClone.Core
{
    public class SpawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.25f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.75f);
        }
    }
}
