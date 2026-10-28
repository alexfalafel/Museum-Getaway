using UnityEngine;
using UnityEngine.UI;

namespace MuseumGetaway
{
    public sealed class EndScreenUI : MonoBehaviour
    {
        [SerializeField] private MuseumGameManager gameManager;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Text winSummary;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private Text loseSummary;
        [SerializeField] private Button winRestartButton;
        [SerializeField] private Button loseRestartButton;

        public void Configure(
            MuseumGameManager manager,
            GameObject win,
            Text winText,
            Button winButton,
            GameObject lose,
            Text loseText,
            Button loseButton)
        {
            gameManager = manager;
            winPanel = win;
            winSummary = winText;
            winRestartButton = winButton;
            losePanel = lose;
            loseSummary = loseText;
            loseRestartButton = loseButton;
        }

        private void Awake()
        {
            winRestartButton.onClick.AddListener(gameManager.RestartGame);
            loseRestartButton.onClick.AddListener(gameManager.RestartGame);
        }

        public void Hide()
        {
            winPanel.SetActive(false);
            losePanel.SetActive(false);
        }

        public void ShowWin(int loot)
        {
            winSummary.text = $"YOU ESCAPED!\nFinal loot: ${loot:N0}";
            winPanel.SetActive(true);
        }

        public void ShowLose(string reason, int loot)
        {
            loseSummary.text = $"{reason}\nLoot collected: ${loot:N0}";
            losePanel.SetActive(true);
        }
    }
}
