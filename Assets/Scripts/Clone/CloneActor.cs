using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowClone.Clone
{
    public class CloneActor : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color idleColor = new Color(0.56f, 0.3f, 1f, 0.65f);
        [SerializeField] private Color highlightColor = new Color(0.28f, 0.9f, 1f, 0.82f);
        [SerializeField] private Color silhouetteColor = new Color(0.04f, 0.02f, 0.12f, 0.72f);
        [SerializeField] private float pulseSpeed = 5f;
        [SerializeField] private float cloneHeightScale = 0.985f;

        private IReadOnlyList<RecordedFrame> frames;
        private float replayTime;
        private int frameIndex;
        private bool isPlaying;
        private Vector3 baseScale = Vector3.one;
        private SpriteRenderer silhouetteRenderer;

        public event Action ReplayFinished;

        private void Awake()
        {
            baseScale = transform.localScale;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = idleColor;
                CreateSilhouetteRenderer();
            }
        }

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
            if (spriteRenderer != null)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                spriteRenderer.color = Color.Lerp(idleColor, highlightColor, pulse);
            }

            if (silhouetteRenderer != null)
            {
                silhouetteRenderer.color = silhouetteColor;
            }

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
            float facing = Mathf.Sign(facingDirection == 0f ? 1f : facingDirection);
            transform.localScale = new Vector3(
                Mathf.Abs(baseScale.x) * facing,
                Mathf.Abs(baseScale.y) * cloneHeightScale,
                baseScale.z);
        }

        private void CreateSilhouetteRenderer()
        {
            if (spriteRenderer == null || silhouetteRenderer != null)
            {
                return;
            }

            GameObject silhouetteObject = new GameObject("CloneSilhouette");
            silhouetteObject.transform.SetParent(spriteRenderer.transform, false);
            silhouetteObject.transform.localPosition = new Vector3(0.04f, -0.02f, 0f);
            silhouetteObject.transform.localScale = new Vector3(1.16f, 1.16f, 1f);

            silhouetteRenderer = silhouetteObject.AddComponent<SpriteRenderer>();
            silhouetteRenderer.sprite = spriteRenderer.sprite;
            silhouetteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            silhouetteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
            silhouetteRenderer.color = silhouetteColor;
        }
    }
}
