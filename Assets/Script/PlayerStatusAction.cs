using System.Collections;
using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerStatusAction : MonoBehaviour
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
            HandleTakingDamage();
            HandleTemporaryDeath();
        }

        private void HandleTakingDamage()
        {
            if (input.DamagePressed && core.isGrounded)
            {
                StartCoroutine(LockMovement(core.attackMoveLockDuration));
                anim.TriggerTakingDamage();
            }
        }

        private void HandleTemporaryDeath()
        {
            if (input.DeathPressed && core.isGrounded)
            {
                StartCoroutine(LockMovement(core.attackMoveLockDuration));
                StartCoroutine(TemporaryDeath());
            }
        }

        private IEnumerator TemporaryDeath()
        {
            anim.SetDead(true);
            yield return new WaitForSeconds(1f);
            anim.SetDead(false);
        }

        private IEnumerator LockMovement(float duration)
        {
            core.canMove = false;
            yield return new WaitForSeconds(duration);
            core.canMove = true;
        }
    }
}