using System.Collections;
using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerDash : MonoBehaviour
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
            HandleAirDash();
            HandleGroundDash();
        }

        private void HandleAirDash()
        {
            if (!input.DashPressed || core.isGrounded || core.isWallHanging) return;
            if (!core.canMove || core.isAirDashing) return;
            if (Time.time < core.lastDashTime + core.dashCooldown) return;

            StartCoroutine(PerformAirDash(input.Horizontal));
            core.lastDashTime = Time.time;
        }

        private IEnumerator PerformAirDash(float directionInput)
        {
            core.isAirDashing = true;
            core.canMove = false;

            float originalGravity = core.rb.gravityScale;
            core.rb.gravityScale = 0;

            float dashX = 0;
            float dashY = 0;

            if (input.UpHeld)
            {
                dashY = 1;
                anim.TriggerAirDashUpward();
            }

            if (dashY == 0)
            {
                if (directionInput < 0)
                {
                    dashX = -1;
                    anim.TriggerAirDashAttack();
                }
                else if (directionInput > 0)
                {
                    dashX = 1;
                    anim.TriggerAirDashAttack();
                }
            }
            else
            {
                if (directionInput < 0) dashX = -1;
                else if (directionInput > 0) dashX = 1;
            }

            if (dashX != 0 || dashY != 0)
            {
                core.rb.linearVelocity = new Vector2(dashX * core.airDashForce, dashY * core.airDashForce);
                yield return new WaitForSeconds(0.7f);
            }

            core.rb.gravityScale = originalGravity;
            core.canMove = true;
            core.isAirDashing = false;
        }

        private void HandleGroundDash()
        {
            if (!input.DashPressed || !core.isGrounded) return;
            if (!core.canMove) return;
            if (Mathf.Approximately(input.Horizontal, 0f)) return;
            if (Time.time < core.lastDashTime + core.dashCooldown) return;

            StartCoroutine(PerformGroundDash(input.Horizontal));
        }

        private IEnumerator PerformGroundDash(float directionInput)
        {
            core.isAirDashing = true;
            core.canMove = false;

            float dashDirectionX = directionInput < 0 ? -1 : 1;

            anim.TriggerGroundDash();
            core.rb.AddForce(new Vector2(dashDirectionX * core.airDashForce, 0), ForceMode2D.Impulse);

            yield return new WaitForSeconds(0.5f);

            core.canMove = true;
            core.isAirDashing = false;
            core.lastDashTime = Time.time;
        }
    }
}