using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowClone.Clone
{
    public class CloneActor : MonoBehaviour
    {
        private IReadOnlyList<RecordedFrame> frames;
        private float replayTime;
        private int frameIndex;
        private bool isPlaying;

        public event Action ReplayFinished;

        public void Play(IReadOnlyList<RecordedFrame> sourceFrames)
        {
            frames = sourceFrames;
            replayTime = 0f;
            frameIndex = 0;
            isPlaying = frames != null && frames.Count > 0;

            if (!isPlaying)
            {
                ReplayFinished?.Invoke();
                return;
            }

            transform.position = frames[0].Position;
            ApplyFacing(frames[0].FacingDirection);
        }

        private void Update()
        {
            if (!isPlaying || frames == null || frames.Count == 0)
            {
                return;
            }

            replayTime += Time.deltaTime;

            while (frameIndex < frames.Count - 1 && replayTime >= frames[frameIndex + 1].Time)
            {
                frameIndex++;
            }

            if (frameIndex >= frames.Count - 1)
            {
                transform.position = frames[frames.Count - 1].Position;
                ApplyFacing(frames[frames.Count - 1].FacingDirection);
                isPlaying = false;
                ReplayFinished?.Invoke();
                return;
            }

            RecordedFrame from = frames[frameIndex];
            RecordedFrame to = frames[frameIndex + 1];
            float segmentDuration = Mathf.Max(0.0001f, to.Time - from.Time);
            float t = Mathf.InverseLerp(from.Time, from.Time + segmentDuration, replayTime);
            transform.position = Vector3.Lerp(from.Position, to.Position, t);
            ApplyFacing(to.FacingDirection);
        }

        public void StopReplay()
        {
            isPlaying = false;
        }

        private void ApplyFacing(float facingDirection)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(facingDirection == 0f ? 1f : facingDirection);
            transform.localScale = scale;
        }
    }
}
