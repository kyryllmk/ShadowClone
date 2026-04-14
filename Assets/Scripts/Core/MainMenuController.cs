using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowClone.Core
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = SceneRegistry.Tutorial;

        public void Play()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void Quit()
        {
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
