using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MuseumGetaway
{
    public sealed class GameHUD : MonoBehaviour
    {
        [SerializeField] private Text lootText;
        [SerializeField] private Text lootGainText;
        [SerializeField] private Text roomsText;
        [SerializeField] private Text artifactsText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text messageText;

        private Coroutine messageRoutine;
        private Coroutine lootGainRoutine;

        public void Configure(
            Text loot,
            Text lootGain,
            Text rooms,
            Text artifacts,
            Text health,
            Text time,
            Text message)
        {
            lootText = loot;
            lootGainText = lootGain;
            roomsText = rooms;
            artifactsText = artifacts;
            healthText = health;
            timeText = time;
            messageText = message;
        }

        public void SetLoot(int amount) => lootText.text = $"LOOT: ${amount:N0}";
        public void SetRooms(int rooms) => roomsText.text = $"ROOMS: {rooms}";
        public void SetArtifacts(int collected, int total) => artifactsText.text = $"ARTIFACTS: {collected} / {total}";
        public void SetHealth(int current, int maximum) => healthText.text = $"HEALTH: {current} / {maximum}";
        public void SetTime(int seconds) => timeText.text = $"TIME: {seconds}";

        public void ShowMessage(string message, float duration)
        {
            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
            }

            messageRoutine = StartCoroutine(ShowMessageForSeconds(message, duration));
        }

        public void ShowLootGain(int amount)
        {
            if (lootGainRoutine != null)
            {
                StopCoroutine(lootGainRoutine);
            }

            lootGainRoutine = StartCoroutine(ShowLootGainForSeconds(amount, 1.5f));
        }

        public void HideMessage()
        {
            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
                messageRoutine = null;
            }

            messageText.gameObject.SetActive(false);
        }

        private IEnumerator ShowMessageForSeconds(string message, float duration)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            messageText.gameObject.SetActive(false);
            messageRoutine = null;
        }

        private IEnumerator ShowLootGainForSeconds(int amount, float duration)
        {
            lootGainText.text = $"+ ${amount:N0}";
            lootGainText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            lootGainText.gameObject.SetActive(false);
            lootGainRoutine = null;
        }
    }
}
