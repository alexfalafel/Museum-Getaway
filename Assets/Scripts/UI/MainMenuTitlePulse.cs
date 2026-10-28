using UnityEngine;

namespace MuseumGetaway
{
    public sealed class MainMenuTitlePulse : MonoBehaviour
    {
        [SerializeField] private float minimumScale = 0.95f;
        [SerializeField] private float maximumScale = 1.05f;
        [SerializeField] private float cycleDuration = 1.8f;

        private Vector3 baseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            float cycle = Mathf.Max(0.1f, cycleDuration);
            float normalized = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / cycle) + 1f) * 0.5f;
            float scale = Mathf.Lerp(minimumScale, maximumScale, normalized);
            transform.localScale = baseScale * scale;
        }
    }
}
