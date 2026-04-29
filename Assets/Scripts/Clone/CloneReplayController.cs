using System;
using ShadowClone.Gameplay;
using UnityEngine;

namespace ShadowClone.Clone
{
    public class CloneReplayController : MonoBehaviour
    {
        [SerializeField] private CloneActor clonePrefab;
        [SerializeField] private Transform cloneParent;

        private CloneActor activeClone;

        public event Action ReplayStarted;
        public event Action ReplayFinished;

        public bool HasActiveClone => activeClone != null;

        public bool StartReplay(RecordingClip clip)
        {
            if (clonePrefab == null || clip == null || clip.FrameCount == 0)
            {
                return false;
            }

            ClearClone();
            activeClone = Instantiate(clonePrefab, clip.Frames[0].Position, Quaternion.identity, cloneParent);
            IgnorePlayerCollision(activeClone);
            activeClone.ReplayFinished += HandleReplayFinished;
            activeClone.Play(clip.Frames);
            ReplayStarted?.Invoke();
            return true;
        }

        public void ClearClone()
        {
            if (activeClone == null)
            {
                return;
            }

            activeClone.ReplayFinished -= HandleReplayFinished;
            activeClone.StopReplay();
            Destroy(activeClone.gameObject);
            activeClone = null;
            ReplayFinished?.Invoke();
        }

        private void HandleReplayFinished()
        {
            if (activeClone == null)
            {
                return;
            }

            activeClone.ReplayFinished -= HandleReplayFinished;
            Destroy(activeClone.gameObject);
            activeClone = null;
            ReplayFinished?.Invoke();
        }

        private static void IgnorePlayerCollision(CloneActor clone)
        {
            if (clone == null)
            {
                return;
            }

            PlayerController player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                return;
            }

            Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>();
            Collider2D[] cloneColliders = clone.GetComponentsInChildren<Collider2D>();

            for (int playerIndex = 0; playerIndex < playerColliders.Length; playerIndex++)
            {
                Collider2D playerCollider = playerColliders[playerIndex];
                if (playerCollider == null)
                {
                    continue;
                }

                for (int cloneIndex = 0; cloneIndex < cloneColliders.Length; cloneIndex++)
                {
                    Collider2D cloneCollider = cloneColliders[cloneIndex];
                    if (cloneCollider == null)
                    {
                        continue;
                    }

                    Physics2D.IgnoreCollision(playerCollider, cloneCollider, true);
                }
            }
        }
    }
}
