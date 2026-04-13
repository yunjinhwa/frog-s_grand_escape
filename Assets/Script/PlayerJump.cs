using System.Collections;
using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerJump : MonoBehaviour
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
            if (!core.canMove) return;

            if (input.JumpPressed)
            {
                if (core.isWallHanging)
                {
                    JumpOffWall();
                }
                else if (core.isGrounded || core.jumpCount < 2)
                {
                    StartCoroutine(PerformJump());
                }
            }
            else if (!core.isGrounded && !core.isWallHanging && input.SlamPressed)
            {
                StartAirSlam();
            }
        }

        private void JumpOffWall()
        {
            float directionMultiplier = transform.position.x > core.lastWallContactPoint.x ? -1 : 1;

            anim.TriggerWallJump();

            Vector2 force = new Vector2(core.wallJumpForce.x * directionMultiplier, core.wallJumpForce.y);
            core.rb.AddForce(force, ForceMode2D.Impulse);

            core.isWallHanging = false;
            core.isGrounded = false;
            core.isCurrentlyJumping = true;
            core.jumpCount++;
        }

        private IEnumerator PerformJump()
        {
            if (core.jumpCount == 0 && core.isGrounded)
            {
                bool isMoving = Mathf.Abs(core.rb.linearVelocity.x) > 0.01f;
                anim.TriggerJumpStart(isMoving);

                yield return new WaitForSeconds(0.1f);

                core.rb.AddForce(Vector2.up * core.jumpForce, ForceMode2D.Impulse);
                core.isGrounded = false;
                core.isCurrentlyJumping = true;
                core.jumpCount++;
            }
            else
            {
                if (core.rb.linearVelocity.y < 0)
                {
                    core.rb.linearVelocity = new Vector2(core.rb.linearVelocity.x, 0);
                }

                anim.TriggerDoubleJump();

                core.rb.AddForce(Vector2.up * core.jumpForce, ForceMode2D.Impulse);
                core.isGrounded = false;
                core.isCurrentlyJumping = true;
                core.jumpCount++;
            }
        }

        private void StartAirSlam()
        {
            if (!core.isAirSlamming)
            {
                StartCoroutine(PerformAirSlam());
            }
        }

        private IEnumerator PerformAirSlam()
        {
            core.isAirSlamming = true;
            core.canMove = false;
            anim.SetAirSlam(true);

            yield return new WaitForSeconds(0.5f);

            core.rb.linearVelocity = new Vector2(0, -core.airSlamForce);
        }
    }
}