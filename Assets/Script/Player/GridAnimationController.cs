using System.Collections;
using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class GridAnimationController
    {
        private readonly Animator animator;
        private readonly MonoBehaviour coroutineRunner;

        private readonly string idleStateName;
        private readonly string runStateName;
        private readonly string jumpStateName;
        private readonly float jumpReturnDelay;

        private Coroutine jumpCoroutine;

        public int FacingHorizontalDirection { get; private set; }

        public GridAnimationController(
            Animator animator,
            MonoBehaviour coroutineRunner,
            string idleStateName = "Idle",
            string runStateName = "Run",
            string jumpStateName = "Jump",
            float jumpReturnDelay = 0.35f)
        {
            this.animator = animator;
            this.coroutineRunner = coroutineRunner;
            this.idleStateName = idleStateName;
            this.runStateName = runStateName;
            this.jumpStateName = jumpStateName;
            this.jumpReturnDelay = jumpReturnDelay;

            FacingHorizontalDirection = 0;
        }

        public void UpdateAnimation(Vector2 direction, bool walking, bool jumping)
        {
            if (direction == Vector2.right)
            {
                FacingHorizontalDirection = 0;
            }
            else if (direction == Vector2.left)
            {
                FacingHorizontalDirection = 1;
            }

            if (animator == null)
                return;

            UpdateSpriteDirection();

            if (jumping)
            {
                PlayJump();
            }
            else if (walking)
            {
                PlayRun();
            }
            else
            {
                PlayIdle();
            }
        }

        public void EndMoveAnimation()
        {
            if (animator == null)
                return;

            if (jumpCoroutine != null)
            {
                coroutineRunner.StopCoroutine(jumpCoroutine);
                jumpCoroutine = null;
            }

            PlayIdle();
        }

        private void PlayIdle()
        {
            animator.Play(idleStateName, 0, 0f);
        }

        private void PlayRun()
        {
            if (jumpCoroutine != null)
            {
                coroutineRunner.StopCoroutine(jumpCoroutine);
                jumpCoroutine = null;
            }

            animator.Play(runStateName, 0, 0f);
        }

        private void PlayJump()
        {
            if (jumpCoroutine != null)
            {
                coroutineRunner.StopCoroutine(jumpCoroutine);
            }

            jumpCoroutine = coroutineRunner.StartCoroutine(JumpRoutine());
        }

        private IEnumerator JumpRoutine()
        {
            animator.Play(jumpStateName, 0, 0f);
            yield return new WaitForSeconds(jumpReturnDelay);
            animator.Play(idleStateName, 0, 0f);
            jumpCoroutine = null;
        }

        private void UpdateSpriteDirection()
        {
            SpriteRenderer spriteRenderer = animator.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;

            spriteRenderer.flipX = FacingHorizontalDirection == 1;
        }
    }
}