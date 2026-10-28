using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MuseumGetaway
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private Button closeControlsButton;
        [SerializeField] private string gameplaySceneName = "MuseumGetaway";

        public void Configure(
            Button play,
            Button controls,
            Button quit,
            GameObject panel,
            Button closeButton,
            string sceneName)
        {
            playButton = play;
            controlsButton = controls;
            quitButton = quit;
            controlsPanel = panel;
            closeControlsButton = closeButton;
            gameplaySceneName = sceneName;
        }

        private void Awake()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playButton != null)
            {
                playButton.onClick.AddListener(PlayGame);
                playButton.Select();
            }

            if (controlsButton != null)
            {
                controlsButton.onClick.AddListener(ShowControls);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }

            if (closeControlsButton != null)
            {
                closeControlsButton.onClick.AddListener(HideControls);
            }

            if (controlsPanel != null)
            {
                controlsPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (controlsPanel != null &&
                controlsPanel.activeSelf &&
                Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HideControls();
            }
        }

        private void OnDestroy()
        {
            if (playButton != null)
            {
                playButton.onClick.RemoveListener(PlayGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
            }

            if (controlsButton != null)
            {
                controlsButton.onClick.RemoveListener(ShowControls);
            }

            if (closeControlsButton != null)
            {
                closeControlsButton.onClick.RemoveListener(HideControls);
            }
        }

        private void PlayGame()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void QuitGame()
        {
            Application.Quit();
        }

        private void ShowControls()
        {
            controlsPanel.SetActive(true);
            closeControlsButton.Select();
        }

        private void HideControls()
        {
            controlsPanel.SetActive(false);
            controlsButton.Select();
        }
    }
}
