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
        [SerializeField] private TutorialProgressSection progressSection;
        [SerializeField] private TutorialProgressAction progressAction;
        [SerializeField] private bool replayAfterReset;

        private bool hasTriggered;

        private void Awake()
        {
            Collider2D triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }

            if (progressSection != TutorialProgressSection.None)
            {
                if (progressAction == TutorialProgressAction.Complete)
                {
                    LevelOneOnboardingBootstrap.CompleteProgressSection(progressSection);
                    return;
                }

                LevelOneOnboardingBootstrap.TryShowProgressPrompt(progressSection, message, duration);
                return;
            }

            if (hasTriggered)
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

        public void Configure(
            string prompt,
            float promptDuration,
            TutorialProgressSection section,
            TutorialProgressAction action,
            bool shouldReplayAfterReset = false)
        {
            message = prompt;
            duration = promptDuration;
            progressSection = section;
            progressAction = action;
            replayAfterReset = shouldReplayAfterReset;
        }

        public void ResetForAttempt()
        {
            if (replayAfterReset)
            {
                hasTriggered = false;
            }
        }
    }
}
