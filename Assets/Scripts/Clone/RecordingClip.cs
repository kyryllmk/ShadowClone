using System.Collections.Generic;
using UnityEngine;

namespace ShadowClone.Clone
{
    [System.Serializable]
    public class RecordingClip
    {
        [SerializeField] private List<RecordedFrame> frames = new List<RecordedFrame>();
        [SerializeField] private float duration;

        public IReadOnlyList<RecordedFrame> Frames => frames;
        public float Duration => duration;
        public int FrameCount => frames.Count;

        public void Clear()
        {
            frames.Clear();
            duration = 0f;
        }

        public void SetFrames(List<RecordedFrame> sourceFrames, float clipDuration)
        {
            frames.Clear();
            frames.AddRange(sourceFrames);
            duration = clipDuration;
        }
    }
}
