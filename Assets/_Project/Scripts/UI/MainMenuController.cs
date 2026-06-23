using UnityEngine;
using UnityEngine.UI;

namespace Foosball
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button m_PlayButton;
        [SerializeField] private Button m_QuitButton;

        void Awake()
        {
            if (m_PlayButton != null)       
                m_PlayButton.onClick.AddListener(OnPlayPressed);

            if (m_QuitButton != null)       
                m_QuitButton.onClick.AddListener(OnQuitPressed);
        }

        void OnDestroy()
        {
            if (m_PlayButton != null)       
                m_PlayButton.onClick.RemoveListener(OnPlayPressed);

            if (m_QuitButton != null)       
                m_QuitButton.onClick.RemoveListener(OnQuitPressed);
        }

        public void OnPlayPressed()       => GameStateManager.Instance?.GoToSetup();
        public void OnQuitPressed()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}