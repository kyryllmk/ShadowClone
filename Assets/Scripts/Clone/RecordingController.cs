using System;
using System.Collections.Generic;
using ShadowClone.Gameplay;
using UnityEngine;

namespace ShadowClone.Clone
{
    public class RecordingController : MonoBehaviour
    {
        [SerializeField] private Transform sampleTarget;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private float sampleInterval = 0.05f;
        [SerializeField] private float maxRecordingDuration = 3f;
        [SerializeField] private RecordingClip currentClip = new RecordingClip();

        private readonly List<RecordedFrame> workingFrames = new List<RecordedFrame>();
        private float elapsedRecordingTime;
        private float sampleTimer;

        public event Action<bool> RecordingStateChanged;
        public event Action<RecordingClip> RecordingFinished;

        public bool IsRecording { get; private set; }
        public RecordingClip CurrentClip => currentClip;
        public bool HasRecording => currentClip != null && currentClip.FrameCount > 1;
        public float ElapsedRecordingTime => elapsedRecordingTime;
        public float MaxRecordingDuration => maxRecordingDuration;

        public bool BeginRecording()
        {
            if (sampleTarget == null || IsRecording)
            {
                return false;
            }

            workingFrames.Clear();
            currentClip.Clear();
            elapsedRecordingTime = 0f;
            sampleTimer = 0f;
            IsRecording = true;
            SampleCurrentFrame();
            RecordingStateChanged?.Invoke(true);
            return true;
        }

        public bool StopRecording()
        {
            if (!IsRecording)
            {
                return false;
            }

            IsRecording = false;
            currentClip.SetFrames(workingFrames, elapsedRecordingTime);
            RecordingStateChanged?.Invoke(false);
            RecordingFinished?.Invoke(currentClip);
            return currentClip.FrameCount > 1;
        }

        public void ClearRecording()
        {
            IsRecording = false;
            elapsedRecordingTime = 0f;
            sampleTimer = 0f;
            workingFrames.Clear();
            currentClip.Clear();
            RecordingStateChanged?.Invoke(false);
        }

        private void Update()
        {
            if (!IsRecording)
            {
                return;
            }

            elapsedRecordingTime += Time.deltaTime;
            sampleTimer += Time.deltaTime;

            while (sampleTimer >= sampleInterval)
            {
                sampleTimer -= sampleInterval;
                SampleCurrentFrame();
            }

            if (elapsedRecordingTime >= maxRecordingDuration)
            {
                StopRecording();
            }
        }

        private void SampleCurrentFrame()
        {
            float facingDirection = playerController != null ? playerController.FacingDirection : 1f;
            workingFrames.Add(new RecordedFrame(elapsedRecordingTime, sampleTarget.position, facingDirection));
        }
    }
}
