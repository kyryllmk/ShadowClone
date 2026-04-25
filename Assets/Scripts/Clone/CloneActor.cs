using System;
using System.Collections.Generic;
using ShadowClone.Presentation;
using UnityEngine;

namespace ShadowClone.Clone
{
    public class CloneActor : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color idleColor = new Color(0.56f, 0.3f, 1f, 0.65f);
        [SerializeField] private Color highlightColor = new Color(0.28f, 0.9f, 1f, 0.82f);
        [SerializeField] private Color silhouetteColor = new Color(0.04f, 0.02f, 0.12f, 0.72f);
        [SerializeField] private Color glowColor = new Color(0.28f, 0.95f, 1f, 0.24f);
        [SerializeField] private float pulseSpeed = 5f;
        [SerializeField] private float cloneHeightScale = 0.985f;
        [SerializeField] private float activationPulseDuration = 0.22f;
        [SerializeField] private float activationPulseScale = 0.26f;
        [SerializeField] private float trailSpawnInterval = 0.035f;
        [SerializeField] private float trailLifetime = 0.18f;
        [SerializeField] private Color trailColor = new Color(0.38f, 0.92f, 1f, 0.34f);

        private IReadOnlyList<RecordedFrame> frames;
        private float replayTime;
        private int frameIndex;
        private bool isPlaying;
        private Vector3 baseScale = Vector3.one;
        private SpriteRenderer silhouetteRenderer;
        private SpriteRenderer glowRenderer;
        private float activationPulseTimer;
        private float trailSpawnTimer;
        private readonly List<TrailGhost> trailGhosts = new List<TrailGhost>();

        public event Action ReplayFinished;

        private sealed class TrailGhost
        {
            public SpriteRenderer Renderer;
            public float RemainingLife;
            public float Lifetime;
        }

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
                CreateGlowRenderer();
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
            activationPulseTimer = activationPulseDuration;
            trailSpawnTimer = 0f;
            ClearTrailGhosts();
        }

        private void Update()
        {
            UpdateTrailGhosts();

            if (spriteRenderer != null)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                spriteRenderer.color = Color.Lerp(idleColor, highlightColor, pulse);
            }

            if (silhouetteRenderer != null)
            {
                silhouetteRenderer.color = silhouetteColor;
            }

            if (glowRenderer != null)
            {
                float basePulse = Mathf.Clamp01(PresentationFeedbackBootstrap.BeatPulse01 + ((Mathf.Sin(Time.time * pulseSpeed * 0.85f) + 1f) * 0.04f));
                float activationBoost = Mathf.Clamp01(activationPulseTimer / Mathf.Max(0.0001f, activationPulseDuration));
                Color nextGlow = glowColor;
                nextGlow.a *= 0.65f + basePulse * 0.35f + activationBoost * 1.15f;
                glowRenderer.color = nextGlow;
                glowRenderer.transform.localScale = Vector3.one * (1.28f + activationBoost * 0.3f);
            }

            if (!isPlaying || frames == null || frames.Count == 0)
            {
                ApplyActivationPulse();
                return;
            }

            replayTime += Time.deltaTime;
            trailSpawnTimer -= Time.deltaTime;

            while (frameIndex < frames.Count - 1 && replayTime >= frames[frameIndex + 1].Time)
            {
                frameIndex++;
            }

            if (frameIndex >= frames.Count - 1)
            {
                transform.position = frames[frames.Count - 1].Position;
                ApplyFacing(frames[frames.Count - 1].FacingDirection);
                SpawnTrailGhost();
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
            SpawnTrailGhostIfReady();
            ApplyActivationPulse();
        }

        public void StopReplay()
        {
            isPlaying = false;
            ClearTrailGhosts();
        }

        private void OnDestroy()
        {
            ClearTrailGhosts();
        }

        private void ApplyFacing(float facingDirection)
        {
            float facing = Mathf.Sign(facingDirection == 0f ? 1f : facingDirection);
            Vector3 nextScale = new Vector3(
                Mathf.Abs(baseScale.x) * facing,
                Mathf.Abs(baseScale.y) * cloneHeightScale,
                baseScale.z);

            if (activationPulseTimer > 0f)
            {
                float pulseProgress = 1f - Mathf.Clamp01(activationPulseTimer / activationPulseDuration);
                float pulseAmount = Mathf.Sin(pulseProgress * Mathf.PI) * activationPulseScale;
                nextScale.x *= 1f + pulseAmount;
                nextScale.y *= 1f + pulseAmount;
            }

            transform.localScale = nextScale;
        }

        private void ApplyActivationPulse()
        {
            if (activationPulseTimer <= 0f)
            {
                return;
            }

            activationPulseTimer = Mathf.Max(0f, activationPulseTimer - Time.deltaTime);
            float facing = Mathf.Sign(transform.localScale.x == 0f ? 1f : transform.localScale.x);
            ApplyFacing(facing);
        }

        private void SpawnTrailGhostIfReady()
        {
            if (trailSpawnTimer > 0f)
            {
                return;
            }

            SpawnTrailGhost();
            trailSpawnTimer = trailSpawnInterval;
        }

        private void SpawnTrailGhost()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            GameObject ghostObject = new GameObject("CloneGhostTrail");
            ghostObject.transform.SetParent(transform.parent, true);
            ghostObject.transform.position = transform.position;
            ghostObject.transform.rotation = transform.rotation;
            ghostObject.transform.localScale = transform.localScale;

            SpriteRenderer ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
            ghostRenderer.sprite = spriteRenderer.sprite;
            ghostRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            ghostRenderer.sortingOrder = spriteRenderer.sortingOrder - 2;
            ghostRenderer.color = trailColor;

            trailGhosts.Add(new TrailGhost
            {
                Renderer = ghostRenderer,
                RemainingLife = trailLifetime,
                Lifetime = trailLifetime
            });
        }

        private void UpdateTrailGhosts()
        {
            for (int i = trailGhosts.Count - 1; i >= 0; i--)
            {
                TrailGhost ghost = trailGhosts[i];
                if (ghost.Renderer == null)
                {
                    trailGhosts.RemoveAt(i);
                    continue;
                }

                ghost.RemainingLife -= Time.deltaTime;
                float fade = Mathf.Clamp01(ghost.RemainingLife / Mathf.Max(0.0001f, ghost.Lifetime));
                Color color = trailColor;
                color.a *= fade;
                ghost.Renderer.color = color;
                ghost.Renderer.transform.localScale *= 1f + Time.deltaTime * 0.55f;

                if (ghost.RemainingLife <= 0f)
                {
                    Destroy(ghost.Renderer.gameObject);
                    trailGhosts.RemoveAt(i);
                }
            }
        }

        private void ClearTrailGhosts()
        {
            for (int i = trailGhosts.Count - 1; i >= 0; i--)
            {
                if (trailGhosts[i].Renderer != null)
                {
                    Destroy(trailGhosts[i].Renderer.gameObject);
                }
            }

            trailGhosts.Clear();
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

        private void CreateGlowRenderer()
        {
            if (spriteRenderer == null || glowRenderer != null)
            {
                return;
            }

            GameObject glowObject = new GameObject("CloneActivationGlow");
            glowObject.transform.SetParent(spriteRenderer.transform, false);
            glowObject.transform.localPosition = Vector3.zero;
            glowObject.transform.localScale = Vector3.one * 1.28f;

            glowRenderer = glowObject.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = spriteRenderer.sprite;
            glowRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            glowRenderer.sortingOrder = spriteRenderer.sortingOrder - 2;
            glowRenderer.color = glowColor;
        }
    }
}
