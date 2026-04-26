using ShadowClone.Clone;
using ShadowClone.Gameplay;
using ShadowClone.Level;
using UnityEngine;

namespace ShadowClone.Core
{
    public class RoomResetManager : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private SpawnPoint spawnPoint;
        [SerializeField] private CloneMechanicController cloneMechanicController;
        [SerializeField] private PuzzleButton[] resettableButtons;
        [SerializeField] private DoorController[] resettableDoors;
        [SerializeField] private KeyCode resetKey = KeyCode.Tab;
        [SerializeField] private KeyCode alternateResetKey = KeyCode.Backspace;
        [SerializeField] private bool enableFallResetFallback = true;
        [SerializeField] private float fallResetY = -7f;
        [SerializeField] private float resetCooldown = 0.05f;

        private Vector3 initialPlayerScale;
        private float resetCooldownTimer;
        public event System.Action<string> ResetCompleted;

        private void Awake()
        {
            if (playerController != null)
            {
                initialPlayerScale = playerController.transform.localScale;
            }
        }

        private void Update()
        {
            if (resetCooldownTimer > 0f)
            {
                resetCooldownTimer -= Time.deltaTime;
            }

            if (Input.GetKeyDown(resetKey) || Input.GetKeyDown(alternateResetKey))
            {
                RequestReset("Manual reset");
                return;
            }

            if (enableFallResetFallback && playerController != null && resetCooldownTimer <= 0f && playerController.transform.position.y <= fallResetY)
            {
                RequestReset("Fall reset");
            }
        }

        public void RequestReset(string reason)
        {
            resetCooldownTimer = resetCooldown;

            if (cloneMechanicController != null)
            {
                cloneMechanicController.ResetMechanicState(reason);
            }

            if (resettableButtons != null)
            {
                for (int i = 0; i < resettableButtons.Length; i++)
                {
                    if (resettableButtons[i] != null)
                    {
                        resettableButtons[i].ClearState();
                    }
                }
            }

            if (resettableDoors != null)
            {
                for (int i = 0; i < resettableDoors.Length; i++)
                {
                    if (resettableDoors[i] != null)
                    {
                        resettableDoors[i].ForceClosed();
                    }
                }
            }

            if (playerController != null && spawnPoint != null)
            {
                playerController.ResetToSpawn(spawnPoint.transform.position, initialPlayerScale);
            }

            ResetCompleted?.Invoke(reason);
        }
    }
}
