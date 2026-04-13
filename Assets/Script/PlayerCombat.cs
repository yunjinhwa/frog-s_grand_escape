using System.Collections;
using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerCombat : MonoBehaviour
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
            HandleAttack();
            HandleSkill1();
            HandleSkill2();
        }

        private void HandleAttack()
        {
            if (!input.AttackPressed || !core.isGrounded) return;

            bool wasMoving = Mathf.Abs(core.rb.linearVelocity.x) > 0;
            float attackDirection = Mathf.Sign(core.rb.linearVelocity.x);

            if (wasMoving)
            {
                anim.TriggerRunAttack();
                StartCoroutine(ApplyRunningAttackForce(attackDirection));
            }
            else
            {
                anim.TriggerAttack1();
                StartCoroutine(LockMovement(core.attackMoveLockDuration));
            }
        }

        private void HandleSkill1()
        {
            if (input.Skill1Pressed && core.isGrounded)
            {
                StartCoroutine(LockMovement(core.attackMoveLockDuration));
                anim.TriggerSkill1();
            }
        }

        private void HandleSkill2()
        {
            if (input.Skill2Pressed && core.isGrounded)
            {
                StartCoroutine(LockMovement(core.attackMoveLockDuration));
                anim.TriggerSkill2();
            }
        }

        private IEnumerator ApplyRunningAttackForce(float direction)
        {
            float initialForce = 2f;
            core.rb.AddForce(new Vector2(direction * initialForce, 0), ForceMode2D.Impulse);
            yield return StartCoroutine(LockMovement(0.5f));
        }

        private IEnumerator LockMovement(float duration)
        {
            core.canMove = false;
            yield return new WaitForSeconds(duration);
            core.canMove = true;
        }
    }
}