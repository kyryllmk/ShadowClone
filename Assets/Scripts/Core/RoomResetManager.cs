using ShadowClone.Clone;
using ShadowClone.Gameplay;
using UnityEngine;

namespace ShadowClone.Core
{
    public class RoomResetManager : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private SpawnPoint spawnPoint;
        [SerializeField] private CloneMechanicController cloneMechanicController;
        [SerializeField] private KeyCode resetKey = KeyCode.Tab;
        [SerializeField] private KeyCode alternateResetKey = KeyCode.Backspace;

        private Vector3 initialPlayerScale;

        private void Awake()
        {
            if (playerController != null)
            {
                initialPlayerScale = playerController.transform.localScale;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(resetKey) || Input.GetKeyDown(alternateResetKey))
            {
                RequestReset("Manual reset");
            }
        }

        public void RequestReset(string reason)
        {
            if (cloneMechanicController != null)
            {
                cloneMechanicController.ResetMechanicState(reason);
            }

            if (playerController != null && spawnPoint != null)
            {
                playerController.ResetToSpawn(spawnPoint.transform.position, initialPlayerScale);
            }
        }
    }
}
