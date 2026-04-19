using ShadowClone.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowClone.Core
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = SceneRegistry.Tutorial;

        public void Play()
        {
            PresentationFeedbackBootstrap.PlayMenuConfirm();
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void Quit()
        {
            PresentationFeedbackBootstrap.PlayMenuConfirm();
            if (Application.isEditor)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }

            Application.Quit();
        }
    }
}
