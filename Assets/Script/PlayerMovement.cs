using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerMovement : MonoBehaviour
    {
        private PlayerCore core;
        private PlayerInputReader input;
        private PlayerAnimationFacade anim;

        private void Awake()
        {
            core = GetComponent<PlayerCore>();
            input = GetComponent<PlayerInputReader>();
            anim = GetComponent<PlayerAnimationFacade>();
        }

        public void Tick()
        {
            HandleCrouching();
            HandleMovement();
            HandleSliding();
        }

        private void HandleMovement()
        {
            if (!core.canMove || core.isClimbingLedge) return;

            bool isMoving = input.LeftPressed || input.RightPressed;
            float speed = core.isCrouching ? core.movementSpeed * core.crouchSpeedFactor : core.movementSpeed;

            if (input.RightPressed)
            {
                anim.SetDirection(0);
                core.rb.linearVelocity = new Vector2(speed, core.rb.linearVelocity.y);
            }
            else if (input.LeftPressed)
            {
                anim.SetDirection(1);
                core.rb.linearVelocity = new Vector2(-speed, core.rb.linearVelocity.y);
            }
            else
            {
                core.rb.linearVelocity = new Vector2(0f, core.rb.linearVelocity.y);
            }

            anim.SetWalking(isMoving);
            anim.SetCrouchingWalking(core.isCrouching && isMoving);

            if (!core.isGrounded && core.rb.linearVelocity.y < 0)
            {
                anim.SetSliding(false);
                anim.SetFalling(true);
            }
        }

        private void HandleCrouching()
        {
            if (input.CrouchDown)
            {
                core.isCrouching = true;
                anim.SetCrouching(true);
            }
            else if (input.CrouchUp)
            {
                core.isCrouching = false;
                anim.SetCrouching(false);
            }
        }

        private void HandleSliding()
        {
            if (input.SlideDown && core.isGrounded && !core.isCrouching)
            {
                anim.SetSliding(true);
                anim.TriggerSlideStart();
            }

            if (input.SlideUp)
            {
                anim.TriggerSlideEnd();
                anim.SetSliding(false);
            }
        }
    }
}