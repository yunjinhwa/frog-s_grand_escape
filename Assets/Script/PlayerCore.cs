using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerCore : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody2D rb;
        public Animator animator;
        public EdgeCollider2D edgeCollider;
        public CapsuleCollider2D capsuleCollider;

        [Header("Movement")]
        public float movementSpeed = 5f;
        public float crouchSpeedFactor = 0.5f;
        public bool isCrouching;
        public bool canMove = true;

        [Header("Jump")]
        public bool isGrounded;
        public bool isCurrentlyJumping;
        public int jumpCount;
        public float jumpForce = 7f;

        [Header("Wall")]
        public bool isWallHanging;
        public float wallSlideSpeed = 1f;
        public Vector2 wallJumpForce = new Vector2(5f, 7f);
        public float wallClimbSpeed = 2f;
        public Vector3 lastWallContactPoint;

        [Header("Dash")]
        public float airDashForce = 10f;
        public bool isAirDashing;
        public float dashCooldown = 1.0f;
        public float lastDashTime;

        [Header("Air Slam")]
        public bool isAirSlamming;
        public float airSlamForce = 20f;

        [Header("Attack")]
        public float attackMoveLockDuration = 0.8f;

        [Header("Ledge")]
        public bool isClimbingLedge;
        public Transform leftLedgeDetector;
        public Transform rightLedgeDetector;

        [Header("Effects")]
        public GameObject aoEPrefab;

        private void Reset()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            edgeCollider = GetComponent<EdgeCollider2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
        }
    }
}