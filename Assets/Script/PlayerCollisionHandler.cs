using System.Collections;
using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerCollisionHandler : MonoBehaviour
    {
        private PlayerCore core;
        private PlayerAnimationFacade anim;

        private void Awake()
        {
            core = GetComponent<PlayerCore>();
            anim = GetComponent<PlayerAnimationFacade>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.otherCollider == core.edgeCollider)
            {
                if (collision.gameObject.CompareTag("Wall") && !core.isGrounded && !core.isAirSlamming)
                {
                    core.isWallHanging = true;
                    core.isCurrentlyJumping = false;
                    core.lastWallContactPoint = collision.contacts[0].point;
                    core.rb.gravityScale = 0;

                    anim.SetWallHang(true);

                    if (core.isAirDashing)
                    {
                        core.isAirDashing = false;
                        core.canMove = true;
                    }
                }
            }

            if (collision.gameObject.CompareTag("Ground"))
            {
                HandleGroundCollision();
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                core.isWallHanging = false;
                core.rb.gravityScale = 1;
                anim.SetWallHang(false);
                anim.SetWallSlide(false);
            }
        }

        private void HandleGroundCollision()
        {
            core.isGrounded = true;
            core.jumpCount = 0;
            core.isWallHanging = false;
            core.rb.gravityScale = 1;
            core.isCurrentlyJumping = false;

            bool isMoving = Mathf.Abs(core.rb.linearVelocity.x) > 0;

            if (core.isAirSlamming)
            {
                anim.TriggerAirSlamLand();
                core.isAirSlamming = false;

                if (core.aoEPrefab != null)
                {
                    GameObject aoe = Instantiate(core.aoEPrefab, transform.position, Quaternion.identity);
                    StartCoroutine(DestroyAfterDelay(aoe, 0.2f));
                }

                anim.SetAirSlam(false);
            }
            else
            {
                anim.TriggerLanding(isMoving);
            }

            core.isAirDashing = false;
            core.canMove = true;
            anim.SetJumpMid(false);
            anim.SetFalling(false);
        }

        private IEnumerator DestroyAfterDelay(GameObject target, float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(target);
        }
    }
}