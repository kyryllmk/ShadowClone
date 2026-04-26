using ShadowClone.Gameplay;
using ShadowClone.UI;
using UnityEngine;

namespace ShadowClone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class OnboardingPromptTrigger : MonoBehaviour
    {
        [SerializeField] private string message;
        [SerializeField] private float duration = 3.2f;

        private bool hasTriggered;

        private void Awake()
        {
            Collider2D triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasTriggered || other.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }

            hasTriggered = true;
            GameplayUiBootstrap.ShowOnboardingPrompt(message, duration);
        }

        public void Configure(string prompt, float promptDuration)
        {
            message = prompt;
            duration = promptDuration;
        }
    }
}
