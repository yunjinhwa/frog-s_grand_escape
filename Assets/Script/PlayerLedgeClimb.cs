using System.Collections;
using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerLedgeClimb : MonoBehaviour
    {
        private PlayerCore core;
        private PlayerAnimationFacade anim;

        private void Awake()
        {
            core = GetComponent<PlayerCore>();
            anim = GetComponent<PlayerAnimationFacade>();
        }

        public void Tick()
        {
            CheckForLedges();
        }

        private void CheckForLedges()
        {
            int ledgeLayer = LayerMask.GetMask("Default");
            float rayLength = 0.22f;
            float angleDegrees = 45f;

            Vector2 leftUp = Quaternion.Euler(0, 0, angleDegrees) * Vector2.left;
            Vector2 leftDown = Quaternion.Euler(0, 0, -angleDegrees) * Vector2.left;
            Vector2 rightUp = Quaternion.Euler(0, 0, -angleDegrees) * Vector2.right;
            Vector2 rightDown = Quaternion.Euler(0, 0, angleDegrees) * Vector2.right;

            RaycastHit2D hitLeftUp = Physics2D.Raycast(core.leftLedgeDetector.position, leftUp, rayLength, ledgeLayer);
            RaycastHit2D hitLeftDown = Physics2D.Raycast(core.leftLedgeDetector.position, leftDown, rayLength, ledgeLayer);
            RaycastHit2D hitRightUp = Physics2D.Raycast(core.rightLedgeDetector.position, rightUp, rayLength, ledgeLayer);
            RaycastHit2D hitRightDown = Physics2D.Raycast(core.rightLedgeDetector.position, rightDown, rayLength, ledgeLayer);

            Debug.DrawLine(core.leftLedgeDetector.position, core.leftLedgeDetector.position + (Vector3)leftUp * rayLength, Color.red);
            Debug.DrawLine(core.leftLedgeDetector.position, core.leftLedgeDetector.position + (Vector3)leftDown * rayLength, Color.blue);
            Debug.DrawLine(core.rightLedgeDetector.position, core.rightLedgeDetector.position + (Vector3)rightUp * rayLength, Color.red);
            Debug.DrawLine(core.rightLedgeDetector.position, core.rightLedgeDetector.position + (Vector3)rightDown * rayLength, Color.blue);

            if (hitLeftUp.collider != null && hitLeftUp.collider.CompareTag("Ledge"))
            {
                StartCoroutine(ClimbOntoLedge(hitLeftUp.collider.bounds, 1));
            }
            else if (hitLeftDown.collider != null && hitLeftDown.collider.CompareTag("Ledge"))
            {
                StartCoroutine(ClimbOntoLedge(hitLeftDown.collider.bounds, 1));
            }

            if (hitRightUp.collider != null && hitRightUp.collider.CompareTag("Ledge"))
            {
                StartCoroutine(ClimbOntoLedge(hitRightUp.collider.bounds, 0));
            }
            else if (hitRightDown.collider != null && hitRightDown.collider.CompareTag("Ledge"))
            {
                StartCoroutine(ClimbOntoLedge(hitRightDown.collider.bounds, 0));
            }
        }

        private IEnumerator ClimbOntoLedge(Bounds ledgeBounds, int direction)
        {
            if (core.isClimbingLedge) yield break;

            anim.TriggerLedgeClimb();
            anim.SetDirection(direction);
            anim.SetWalking(false);

            core.isClimbingLedge = true;
            core.canMove = false;

            if (core.edgeCollider != null) core.edgeCollider.enabled = false;
            if (core.capsuleCollider != null) core.capsuleCollider.enabled = false;

            core.rb.gravityScale = 0;
            core.rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(0.66f);

            float newY = ledgeBounds.max.y + (0.65f / 2f);
            float newX = transform.position.x + (direction == 0 ? 0.3f : -0.3f);

            transform.position = new Vector3(newX, newY, transform.position.z);
            core.rb.linearVelocity = Vector2.zero;

            if (core.edgeCollider != null) core.edgeCollider.enabled = true;
            if (core.capsuleCollider != null) core.capsuleCollider.enabled = true;

            core.rb.gravityScale = 1;
            core.canMove = true;

            yield return new WaitForSeconds(0.6f);
            core.isClimbingLedge = false;
        }
    }
}