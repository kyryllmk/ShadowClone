using System;
using ShadowClone.Core;
using ShadowClone.Gameplay;
using ShadowClone.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowClone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class GoalZone : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private MechanicHudController mechanicHudController;
        [SerializeField] private string completionMessage = "Level complete!";
        [SerializeField] private bool loadNextSceneOnComplete;
        [SerializeField] private string nextSceneName;
        [SerializeField] private bool saveProgressOnComplete = true;
        [SerializeField] private int completedLevelNumber;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color idleColor = new Color(0.2f, 0.86f, 1f, 1f);
        [SerializeField] private Color completeColor = new Color(1f, 0.95f, 0.45f, 1f);
        [SerializeField] private float pulseSpeed = 3f;

        private Collider2D triggerCollider;
        private bool isCompleted;

        public bool IsCompleted => isCompleted;
        public event Action Completed;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
        }

        private void Update()
        {
            if (targetRenderer == null)
            {
                return;
            }

            if (isCompleted)
            {
                targetRenderer.color = Color.Lerp(targetRenderer.color, completeColor, 8f * Time.deltaTime);
                return;
            }

            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            targetRenderer.color = Color.Lerp(idleColor * 0.8f, idleColor, pulse);
        }

        private void OnValidate()
        {
            Collider2D localCollider = GetComponent<Collider2D>();
            if (localCollider != null)
            {
                localCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCompleted)
            {
                return;
            }

            if (other.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }

            isCompleted = true;
            playerController?.SetMovementLocked(true);
            mechanicHudController?.ShowCompletion(completionMessage);

            if (saveProgressOnComplete)
            {
                if (completedLevelNumber >= LevelProgressManager.FirstLevel && completedLevelNumber <= LevelProgressManager.MaxLevel)
                {
                    LevelProgressManager.CompleteLevel(completedLevelNumber);
                }
                else
                {
                    LevelProgressManager.TryCompleteCurrentScene();
                }
            }

            Completed?.Invoke();

            if (loadNextSceneOnComplete && !string.IsNullOrWhiteSpace(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
