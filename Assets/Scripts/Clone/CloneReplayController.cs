using System;
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
    }
}
