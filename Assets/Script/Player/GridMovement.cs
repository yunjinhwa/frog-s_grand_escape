using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class GridMovement : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float moveSpeed = 6f;

        [Header("Collision Check")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private Vector2 checkBoxSize = new Vector2(0.4f, 0.4f);

        [Header("Terrain Check")]
        [SerializeField] private LayerMask waterLayer;
        [SerializeField] private LayerMask rideableLayer;

        [Header("Jump Settings")]
        [SerializeField] private bool jump = true;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;

        [Header("Respawn Settings")]
        [SerializeField] private Vector3 startPosition;
        [SerializeField] private bool usePlacedPositionAsStart = true;

        [Header("Optional")]
        [SerializeField] private Animator animator;

        [Header("Offsets")]
        [SerializeField] private Vector2 footOffset = new Vector2(0f, -0.5f);

        private bool isMoving;
        private bool isJumping;
        private bool pendingFallToWater;

        private Vector3 targetPosition;
        private MovingPlatformVertical currentPlatform;
        private int maxRightSteps;

        private GridPositionHelper positionHelper;
        private GridCollisionChecker collisionChecker;
        private GridAnimationController animationController;
        private PlayerRespawnHandler respawnHandler;

        private void Start()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            positionHelper = new GridPositionHelper(cellSize, footOffset);
            collisionChecker = new GridCollisionChecker(
                obstacleLayer,
                waterLayer,
                rideableLayer,
                checkBoxSize,
                positionHelper
            );
            animationController = new GridAnimationController(
                animator,
                this,
                "Idle",
                "Run",
                "Jump",
                0.35f
            );
            respawnHandler = new PlayerRespawnHandler(animationController);

            targetPosition = positionHelper.SnapToGrid(transform.position);
            transform.position = targetPosition;

            if (usePlacedPositionAsStart)
                startPosition = targetPosition;
            else
                startPosition = positionHelper.SnapToGrid(startPosition);

        // 시작 지점을 기준(0)으로 보고 오른쪽으로 간 가장 먼 칸을 추적
        maxRightSteps = 0;
        }

        private void Update()
        {
            FollowCurrentPlatform();
            CheckForObstacleCollision();

            if (!isMoving)
                HandleInput();

            MoveToTarget();
            UpdateCurrentPlatform();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(jumpKey) && jump)
            {
                HandleJumpInput();
                return;
            }

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                TryMove(Vector2.up, 1, false);
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                TryMove(Vector2.down, 1, false);
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                TryMove(Vector2.left, 1, false);
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                TryMove(Vector2.right, 1, false);
        }

        private void HandleJumpInput()
        {
            Vector2 jumpDirection =
                animationController.FacingHorizontalDirection == 1 ? Vector2.left : Vector2.right;

            Vector3 landingCell = positionHelper.GetMoveDestination(targetPosition, jumpDirection, 2);

            if (collisionChecker.IsBlocked(landingCell))
            {
                HandleObstacleCollision();
                return;
            }

            animationController.UpdateAnimation(jumpDirection, false, true);

            targetPosition = landingCell;
            isMoving = true;
            isJumping = true;
            pendingFallToWater =
                collisionChecker.IsWater(landingCell) &&
                !collisionChecker.HasRideableObject(landingCell);
        }

        private void TryMove(Vector2 direction, int distanceInCells, bool isJump)
        {
            Vector3 destination = positionHelper.GetMoveDestination(targetPosition, direction, distanceInCells);

            if (!collisionChecker.CanStandOn(destination))
            {
                if (collisionChecker.IsBlocked(destination))
                {
                    HandleObstacleCollision();
                    return;
                }

                return;
            }

            targetPosition = destination;
            isMoving = true;
            isJumping = isJump;
            pendingFallToWater = false;

            // Y축(위/아래) 이동에는 점수를 주지 않음

        // 오른쪽으로 이동할 때만 점수 처리 (위로 이동 등은 제외)
        if (direction.x > 0.5f)
        {
            int destSteps = Mathf.RoundToInt((destination.x - startPosition.x) / cellSize);
            if (destSteps > maxRightSteps)
            {
                int newSteps = destSteps - maxRightSteps;
                GameState.Instance.AddScore(10 * newSteps);
                maxRightSteps = destSteps;
            }
        }

            animationController.UpdateAnimation(direction, true, false);
        }

        private void MoveToTarget()
        {
            if (!isMoving)
                return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) <= 0.001f)
            {
                transform.position = targetPosition;
                isMoving = false;

                if (pendingFallToWater)
                {
                    FallIntoWater();
                    return;
                }

                isJumping = false;
                animationController.EndMoveAnimation();
            }
        }

        private void FollowCurrentPlatform()
        {
            if (currentPlatform == null || isJumping)
                return;

            Vector3 delta = currentPlatform.DeltaMovement;
            if (delta == Vector3.zero)
                return;

            transform.position += delta;
            targetPosition += delta;
        }

        private void UpdateCurrentPlatform()
        {
            currentPlatform = collisionChecker.GetCurrentPlatform(transform.position);
        }

        private void CheckForObstacleCollision()
        {
            if (collisionChecker.IsObstacleTouching(transform.position))
            {
                HandleObstacleCollision();
            }
        }

        private void FallIntoWater()
        {
            pendingFallToWater = false;
            isJumping = false;
            isMoving = false;

            respawnHandler.RespawnFromWater(
                transform,
                ref targetPosition,
                ref currentPlatform,
                startPosition
            );

            if (StageFlowManager.Instance != null)
            {
                StageFlowManager.Instance.HandlePlayerDeathAndRestartStage();
            }
        }

        private void HandleObstacleCollision()
        {
            pendingFallToWater = false;
            isJumping = false;
            isMoving = false;

            respawnHandler.RespawnFromObstacle(
                transform,
                ref targetPosition,
                ref currentPlatform,
                startPosition
            );

            if (StageFlowManager.Instance != null)
            {
                StageFlowManager.Instance.HandlePlayerDeathAndRestartStage();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 drawPos = Application.isPlaying ? targetPosition : transform.position;
            Vector3 drawFoot = drawPos + (Vector3)footOffset;
            Vector3 startFoot = startPosition + (Vector3)footOffset;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(drawFoot, checkBoxSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startFoot, 0.15f);
        }
    }
}