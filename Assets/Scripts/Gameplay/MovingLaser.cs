using UnityEngine;

namespace MuseumGetaway
{
    public sealed class MovingLaser : MonoBehaviour
    {
        [SerializeField] private float xAmplitude = 1f;
        [SerializeField] private float zAmplitude = 1f;
        [SerializeField] private float xFrequency = 0.8f;
        [SerializeField] private float zFrequency = 1.1f;
        [SerializeField] private float xPhase;
        [SerializeField] private float zPhase = 1f;

        private Vector3 startPosition;

        public void Configure(
            float horizontalAmplitude,
            float depthAmplitude,
            float horizontalFrequency,
            float depthFrequency,
            float horizontalPhase,
            float depthPhase)
        {
            xAmplitude = horizontalAmplitude;
            zAmplitude = depthAmplitude;
            xFrequency = horizontalFrequency;
            zFrequency = depthFrequency;
            xPhase = horizontalPhase;
            zPhase = depthPhase;
        }

        private void Start()
        {
            startPosition = transform.localPosition;
        }

        private void Update()
        {
            float xOffset = Mathf.Sin((Time.time * xFrequency) + xPhase) * xAmplitude;
            float zOffset = Mathf.Sin((Time.time * zFrequency) + zPhase) * zAmplitude;
            transform.localPosition = startPosition + new Vector3(xOffset, 0f, zOffset);
        }
    }
}
