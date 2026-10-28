using UnityEngine;

namespace MuseumGetaway
{
    public sealed class PlayerAnimationDriver : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int VictoryParameter = Animator.StringToHash("Victory");

        [SerializeField] private MuseumGameManager gameManager;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Animator characterAnimator;

        private bool victoryTriggered;

        public void Configure(
            MuseumGameManager manager,
            CharacterController controller,
            Animator animator)
        {
            gameManager = manager;
            characterController = controller;
            characterAnimator = animator;
            if (characterAnimator != null)
            {
                characterAnimator.applyRootMotion = false;
            }
        }

        private void LateUpdate()
        {
            if (gameManager == null || characterController == null || characterAnimator == null)
            {
                return;
            }

            if (gameManager.State == MuseumGameManager.GameState.Won)
            {
                characterAnimator.SetFloat(SpeedParameter, 0f);
                if (!victoryTriggered)
                {
                    victoryTriggered = true;
                    characterAnimator.SetTrigger(VictoryParameter);
                }

                return;
            }

            if (!gameManager.IsPlaying)
            {
                characterAnimator.SetFloat(SpeedParameter, 0f);
                return;
            }

            Vector3 horizontalVelocity = characterController.velocity;
            horizontalVelocity.y = 0f;
            characterAnimator.SetFloat(SpeedParameter, horizontalVelocity.magnitude);
        }
    }
}
