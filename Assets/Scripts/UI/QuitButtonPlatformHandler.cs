using ShadowClone.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowClone.UI
{
    public class QuitButtonPlatformHandler : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_WEBGL
            gameObject.SetActive(false);
#endif
        }

        public void QuitGame()
        {
#if UNITY_WEBGL
            SceneManager.LoadScene(SceneRegistry.MainMenu);
#else
            Application.Quit();
#endif
        }
    }
}
