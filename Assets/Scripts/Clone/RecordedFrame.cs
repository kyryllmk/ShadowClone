using UnityEngine;

namespace ShadowClone.Clone
{
    [System.Serializable]
    public struct RecordedFrame
    {
        public float Time;
        public Vector3 Position;
        public float FacingDirection;

        public RecordedFrame(float time, Vector3 position, float facingDirection)
        {
            Time = time;
            Position = position;
            FacingDirection = facingDirection;
        }
    }
}
