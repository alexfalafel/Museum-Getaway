using UnityEngine;

namespace MuseumGetaway
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private MuseumGameManager gameManager;
        [SerializeField] private int maximumHealth = 3;
        [SerializeField] private float damageCooldown = 1.25f;
        [SerializeField] private Renderer playerRenderer;

        private int currentHealth;
        private float nextDamageTime;

        public void Configure(MuseumGameManager manager, Renderer visual)
        {
            gameManager = manager;
            playerRenderer = visual;
        }

        private void Start()
        {
            currentHealth = maximumHealth;
            gameManager.SetHealth(currentHealth, maximumHealth);
        }

        public void TakeDamage(int amount)
        {
            if (!gameManager.IsPlaying || Time.time < nextDamageTime)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - amount);
            nextDamageTime = Time.time + damageCooldown;
            gameManager.SetHealth(currentHealth, maximumHealth);

            if (playerRenderer != null)
            {
                StopAllCoroutines();
                StartCoroutine(FlashAfterDamage());
            }

            if (currentHealth == 0)
            {
                gameManager.PlayerDied();
            }
        }

        private System.Collections.IEnumerator FlashAfterDamage()
        {
            Color original = playerRenderer.material.color;
            playerRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.18f);
            playerRenderer.material.color = original;
        }
    }
}
