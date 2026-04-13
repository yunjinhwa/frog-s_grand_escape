using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerWallInteraction : MonoBehaviour
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

        public void FixedTick()
        {
            HandleJumpFallAnimation();
            HandleWallMovement();
        }

        private void HandleJumpFallAnimation()
        {
            if (!core.isGrounded && core.isCurrentlyJumping)
            {
                if (core.rb.linearVelocity.y > 0)
                {
                    anim.SetFalling(false);
                    anim.SetJumpMid(true);
                }
                else
                {
                    anim.SetJumpMid(false);
                    anim.SetFalling(true);
                }
            }
            else if (!core.isGrounded && !core.isCurrentlyJumping && !core.isWallHanging && !core.isCrouching)
            {
                core.isGrounded = true;
                anim.SetFalling(false);
            }

            if (core.isGrounded && !core.isCurrentlyJumping)
            {
                anim.SetFalling(false);
                anim.SetJumpMid(false);
            }
        }

        private void HandleWallMovement()
        {
            if (core.isWallHanging && !core.isAirSlamming)
            {
                float moveVertical = input.Vertical;

                if (moveVertical > 0)
                {
                    core.rb.linearVelocity = new Vector2(core.rb.linearVelocity.x, core.wallClimbSpeed);
                    anim.SetClimbing(true);
                }
                else if (moveVertical < 0)
                {
                    core.rb.linearVelocity = new Vector2(core.rb.linearVelocity.x, -core.wallSlideSpeed);
                    anim.SetWallSlide(true);
                }
                else
                {
                    core.rb.linearVelocity = new Vector2(core.rb.linearVelocity.x, 0);
                    anim.SetWallSlide(false);
                    anim.SetClimbing(false);
                }
            }
            else
            {
                anim.SetClimbing(false);
                anim.SetWallSlide(false);
            }
        }
    }
}