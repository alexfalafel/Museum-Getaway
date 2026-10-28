using UnityEngine;

namespace MuseumGetaway
{
    public sealed class GetawayExit : MonoBehaviour
    {
        [SerializeField] private MuseumGameManager gameManager;
        [SerializeField] private Renderer exitRenderer;
        [SerializeField] private Material inactiveMaterial;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private GameObject getawayReadyVisual;

        private bool isActive;

        public void Configure(
            MuseumGameManager manager,
            Renderer visual,
            Material inactive,
            Material active,
            GameObject readyVisual)
        {
            gameManager = manager;
            exitRenderer = visual;
            inactiveMaterial = inactive;
            activeMaterial = active;
            getawayReadyVisual = readyVisual;
        }

        public void SetExitActive(bool active)
        {
            isActive = active;
            if (exitRenderer != null)
            {
                exitRenderer.sharedMaterial = active ? activeMaterial : inactiveMaterial;
            }

            if (getawayReadyVisual != null)
            {
                getawayReadyVisual.SetActive(active);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isActive && other.GetComponent<PlayerMovement>() != null)
            {
                gameManager.Win();
            }
        }
    }
}
