using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerCore core;
        private PlayerMovement movement;
        private PlayerJump jump;
        private PlayerDash dash;
        private PlayerCombat combat;
        private PlayerStatusAction statusAction;
        private PlayerWallInteraction wallInteraction;
        private PlayerLedgeClimb ledgeClimb;

        private void Awake()
        {
            core = GetComponent<PlayerCore>();
            movement = GetComponent<PlayerMovement>();
            jump = GetComponent<PlayerJump>();
            dash = GetComponent<PlayerDash>();
            combat = GetComponent<PlayerCombat>();
            statusAction = GetComponent<PlayerStatusAction>();
            wallInteraction = GetComponent<PlayerWallInteraction>();
            ledgeClimb = GetComponent<PlayerLedgeClimb>();
        }

        private void Update()
        {
            if (!core.canMove && !core.isClimbingLedge)
            {
                statusAction.Tick();
                return;
            }

            if (!core.isClimbingLedge)
            {
                movement.Tick();
                combat.Tick();
                jump.Tick();
                dash.Tick();
                statusAction.Tick();
                ledgeClimb.Tick();

                if (core.isWallHanging)
                {
                    core.jumpCount = 0;
                }

                if (core.rb.linearVelocity.y < 0)
                {
                    core.isGrounded = false;
                    core.isCurrentlyJumping = true;
                }
            }
        }

        private void FixedUpdate()
        {
            wallInteraction.FixedTick();
        }
    }
}