using UnityEngine;

namespace MuseumGetaway
{
    public sealed class ArtifactCollectible : MonoBehaviour
    {
        [SerializeField] private MuseumGameManager gameManager;
        [SerializeField] private int cashValue = 100;

        private bool collected;

        public void Configure(MuseumGameManager manager, int value)
        {
            gameManager = manager;
            cashValue = value;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected || other.GetComponent<PlayerMovement>() == null)
            {
                return;
            }

            if (gameManager.CollectArtifact(cashValue))
            {
                collected = true;
                gameObject.SetActive(false);
            }
        }
    }
}
