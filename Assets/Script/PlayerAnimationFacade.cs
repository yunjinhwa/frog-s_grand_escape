using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerAnimationFacade : MonoBehaviour
    {
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void SetDirection(int value) => animator.SetInteger("Direction", value);
        public void SetWalking(bool value) => animator.SetBool("isWalking", value);
        public void SetCrouching(bool value) => animator.SetBool("isCrouching", value);
        public void SetCrouchingWalking(bool value) => animator.SetBool("isCrouchingWalking", value);
        public void SetSliding(bool value) => animator.SetBool("isSliding", value);
        public void SetFalling(bool value) => animator.SetBool("isFalling", value);
        public void SetJumpMid(bool value) => animator.SetBool("isJumpMid", value);
        public void SetWallHang(bool value) => animator.SetBool("isWallHang", value);
        public void SetWallSlide(bool value) => animator.SetBool("isWallSlide", value);
        public void SetClimbing(bool value) => animator.SetBool("isClimbing", value);
        public void SetAirSlam(bool value) => animator.SetBool("isAirSlam", value);
        public void SetDead(bool value) => animator.SetBool("isDead", value);

        public void TriggerAttack1() => animator.SetTrigger("isAttack1");
        public void TriggerRunAttack() => animator.SetTrigger("isRunAttack");
        public void TriggerJumpStart(bool isMoving) => animator.SetTrigger(isMoving ? "isJumpRunStart" : "isJumpStart");
        public void TriggerDoubleJump() => animator.SetTrigger("isDoubleJump");
        public void TriggerWallJump() => animator.SetTrigger("isWallJump");
        public void TriggerAirDashAttack() => animator.SetTrigger("isAirDashAttack");
        public void TriggerAirDashUpward() => animator.SetTrigger("isAirDashUpward");
        public void TriggerGroundDash() => animator.SetTrigger("isDashForward");
        public void TriggerSlideStart() => animator.SetTrigger("isSlideStart");
        public void TriggerSlideEnd() => animator.SetTrigger("isSlideEnd");
        public void TriggerLanding(bool running) => animator.SetTrigger(running ? "isLandingRunning" : "isLanding");
        public void TriggerAirSlamLand() => animator.SetTrigger("isAirSlamLand");
        public void TriggerSkill1() => animator.SetTrigger("isUsingSpecialAbility1");
        public void TriggerSkill2() => animator.SetTrigger("isUsingSpecialAbility2");
        public void TriggerTakingDamage() => animator.SetTrigger("isTakingDamage");
        public void TriggerLedgeClimb() => animator.SetTrigger("isLedgeClimbing");
    }
}