using UnityEngine;
using UnityEngine.EventSystems;

namespace MuseumGetaway
{
    public sealed class MainMenuButtonFeedback : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float pressedScale = 0.96f;
        [SerializeField] private float responseSpeed = 14f;

        private Vector3 baseScale;
        private float targetMultiplier = 1f;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            float blend = 1f - Mathf.Exp(-responseSpeed * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                baseScale * targetMultiplier,
                blend);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetMultiplier = hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetMultiplier = 1f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetMultiplier = pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetMultiplier = eventData.pointerEnter == gameObject ? hoverScale : 1f;
        }
    }
}
