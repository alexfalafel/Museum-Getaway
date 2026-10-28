using UnityEngine;
using UnityEngine.SceneManagement;

namespace MuseumGetaway
{
    public sealed class MuseumGameManager : MonoBehaviour
    {
        public enum GameState
        {
            Playing,
            Won,
            Lost
        }

        [SerializeField] private int requiredArtifacts = 5;
        [SerializeField] private float alarmDuration = 60f;
        [SerializeField] private GameHUD hud;
        [SerializeField] private EndScreenUI endScreen;
        [SerializeField] private GetawayExit getawayExit;

        private int loot;
        private int collectedArtifacts;
        private int currentHealth = 3;
        private float timeRemaining;
        private bool alarmActive;

        public GameState State { get; private set; } = GameState.Playing;
        public bool IsPlaying => State == GameState.Playing;
        public int CurrentSeed { get; private set; }
        public int RoomCount { get; private set; } = 5;

        public void Configure(
            int artifactCount,
            float duration,
            GameHUD gameHud,
            EndScreenUI gameEndScreen,
            GetawayExit exit)
        {
            requiredArtifacts = artifactCount;
            alarmDuration = duration;
            hud = gameHud;
            endScreen = gameEndScreen;
            getawayExit = exit;
        }

        public void ConfigureRun(
            int seed,
            int rooms,
            int artifactCount,
            float duration,
            GameHUD gameHud,
            EndScreenUI gameEndScreen,
            GetawayExit exit)
        {
            CurrentSeed = seed;
            RoomCount = rooms;
            Configure(artifactCount, duration, gameHud, gameEndScreen, exit);
        }

        private void Start()
        {
            Time.timeScale = 1f;
            SetGameplayCursor(false);
            timeRemaining = alarmDuration;
            endScreen.Hide();
            getawayExit.SetExitActive(false);
            hud.SetRooms(RoomCount);
            RefreshHud();
        }

        private void Update()
        {
            if (!IsPlaying || !alarmActive)
            {
                return;
            }

            timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
            hud.SetTime(Mathf.CeilToInt(timeRemaining));

            if (timeRemaining <= 0f)
            {
                Lose("THE ALARM EXPIRED!");
            }
        }

        public bool CollectArtifact(int cashValue)
        {
            if (!IsPlaying)
            {
                return false;
            }

            loot += cashValue;
            collectedArtifacts++;
            hud.ShowLootGain(cashValue);

            if (!alarmActive)
            {
                alarmActive = true;
                hud.ShowMessage("ALARM ACTIVATED!", 2.5f);
            }

            if (collectedArtifacts >= requiredArtifacts)
            {
                getawayExit.SetExitActive(true);
                hud.ShowMessage("GETAWAY READY!", 3f);
            }

            RefreshHud();
            return true;
        }

        public void SetHealth(int health, int maximumHealth)
        {
            currentHealth = health;
            hud.SetHealth(currentHealth, maximumHealth);
        }

        public void PlayerDied()
        {
            Lose("YOU WERE CAUGHT!");
        }

        public void Win()
        {
            if (!IsPlaying)
            {
                return;
            }

            State = GameState.Won;
            SetGameplayCursor(false);
            hud.HideMessage();
            endScreen.ShowWin(loot);
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SetGameplayCursor(false);
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
        }

        private void Lose(string reason)
        {
            if (!IsPlaying)
            {
                return;
            }

            State = GameState.Lost;
            SetGameplayCursor(false);
            hud.HideMessage();
            endScreen.ShowLose(reason, loot);
        }

        private void RefreshHud()
        {
            hud.SetLoot(loot);
            hud.SetArtifacts(collectedArtifacts, requiredArtifacts);
            hud.SetHealth(currentHealth, 3);
            hud.SetTime(Mathf.CeilToInt(timeRemaining));
        }

        private void SetGameplayCursor(bool gameplayActive)
        {
            Cursor.lockState = gameplayActive ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !gameplayActive;
        }
    }
}
