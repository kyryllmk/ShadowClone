using System;
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

        private Collider2D triggerCollider;
        private bool isCompleted;

        public bool IsCompleted => isCompleted;
        public event Action Completed;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
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
            Completed?.Invoke();

            if (loadNextSceneOnComplete && !string.IsNullOrWhiteSpace(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
