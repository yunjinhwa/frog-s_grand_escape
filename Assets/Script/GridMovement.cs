using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
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

        // 0 = 오른쪽, 1 = 왼쪽, 2 = 아래, 3 = 위
        private int currentDirection = 0;

        // 0 = 오른쪽, 1 = 왼쪽
        private int facingHorizontalDirection = 0;

        private void Start()
        {
            targetPosition = SnapToGrid(transform.position);
            transform.position = targetPosition;

            if (usePlacedPositionAsStart)
                startPosition = targetPosition;
            else
                startPosition = SnapToGrid(startPosition);

            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void Update()
        {
            FollowCurrentPlatform();

            if (!isMoving)
                HandleInput();

            MoveToTarget();
            UpdateCurrentPlatform();
        }

        private void FollowCurrentPlatform()
        {
            // 항상 발판의 이동량을 적용하되, 점프 중일 때는 적용하지 않음
            if (currentPlatform == null || isJumping)
                return;

            Vector3 delta = currentPlatform.DeltaMovement;
            if (delta == Vector3.zero)
                return;

            // 발판이 움직일 때 플레이어의 현재 위치와 목표 위치 모두 보정하여
            // 발판 위에서 이동 중에도 발판을 이탈하지 않도록 함
            transform.position += delta;
            targetPosition += delta;
        }

        private void UpdateCurrentPlatform()
        {
            Collider2D hit = Physics2D.OverlapBox(GetFootPosition(), checkBoxSize, 0f, rideableLayer);

            if (hit != null)
                currentPlatform = hit.GetComponent<MovingPlatformVertical>();
            else
                currentPlatform = null;
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(jumpKey))
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
            Vector2 jumpDirection = (facingHorizontalDirection == 1) ? Vector2.left : Vector2.right;
            Vector3 landingCell = targetPosition + (Vector3)(jumpDirection * cellSize * 2);

            if (!CanJump(jumpDirection))
            {
                EndMoveAnimation();
                return;
            }

            if (animator != null)
                UpdateAnimation(jumpDirection, false, true);

            targetPosition = landingCell;
            isMoving = true;
            isJumping = true;

            // 강으로 점프했고, 발판이 없으면 도착 후 추락 처리
            pendingFallToWater = IsWater(landingCell) && !HasRideableObject(landingCell);
        }

        private void TryMove(Vector2 direction, int distanceInCells, bool isJump)
        {
            Vector3 destination = targetPosition + (Vector3)(direction * cellSize * distanceInCells);

            if (!CanStandOn(destination))
                return;

            targetPosition = destination;
            isMoving = true;
            isJumping = isJump;
            pendingFallToWater = false;

            if (isJump)
                UpdateAnimation(direction, false, true);
            else
                UpdateAnimation(direction, true, false);
        }

        private bool CanJump(Vector2 direction)
        {
            Vector3 landingCell = targetPosition + (Vector3)(direction * cellSize * 2);

            // 장애물만 아니면 점프 가능
            if (IsBlocked(landingCell))
                return false;

            return true;
        }

        private bool CanStandOn(Vector3 worldPosition)
        {
            if (IsBlocked(worldPosition))
                return false;

            if (IsWater(worldPosition))
            {
                if (!HasRideableObject(worldPosition))
                    return false;
            }

            return true;
        }

        private bool IsBlocked(Vector3 worldPosition)
        {
            Collider2D hit = Physics2D.OverlapBox(GetFootPositionFromWorld(worldPosition), checkBoxSize, 0f, obstacleLayer);
            return hit != null;
        }

        private bool IsWater(Vector3 worldPosition)
        {
            Collider2D hit = Physics2D.OverlapBox(GetFootPositionFromWorld(worldPosition), checkBoxSize, 0f, waterLayer);
            return hit != null;
        }

        private bool HasRideableObject(Vector3 worldPosition)
        {
            Collider2D hit = Physics2D.OverlapBox(GetFootPositionFromWorld(worldPosition), checkBoxSize, 0f, rideableLayer);
            return hit != null;
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
                EndMoveAnimation();
            }
        }

        private void FallIntoWater()
        {
            pendingFallToWater = false;
            isJumping = false;

            GameState.Instance.DecreaseHP(1);

            transform.position = startPosition;
            targetPosition = startPosition;
            currentPlatform = null;

            EndMoveAnimation();

            Debug.Log("강에 빠짐! 시작 위치로 복귀");
        }

        private Vector3 SnapToGrid(Vector3 pos)
        {
            float half = cellSize * 0.5f;
            float x = Mathf.Round((pos.x - half) / cellSize) * cellSize + half;
            float y = Mathf.Round((pos.y - half) / cellSize) * cellSize + half;
            return new Vector3(x, y, pos.z);
        }

        private void UpdateAnimation(Vector2 direction, bool walking, bool jumping)
        {
            if (direction == Vector2.right)
            {
                currentDirection = 0;
                facingHorizontalDirection = 0;
            }
            else if (direction == Vector2.left)
            {
                currentDirection = 1;
                facingHorizontalDirection = 1;
            }
            else if (direction == Vector2.down)
            {
                currentDirection = 2;
            }
            else if (direction == Vector2.up)
            {
                currentDirection = 3;
            }

            if (animator == null)
                return;

            animator.SetInteger("Direction", currentDirection);

            if (jumping)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isJumpStart", true);
                animator.ResetTrigger("isJumpStart");
                animator.SetTrigger("isJumpStart");
            }
            else
            {
                animator.SetBool("isJumpStart", false);
                animator.SetBool("isWalking", walking);
            }
        }

        private void EndMoveAnimation()
        {
            if (animator == null)
                return;

            animator.SetBool("isWalking", false);
            animator.SetBool("isJumpStart", false);

            if (currentDirection == 2 || currentDirection == 3)
                animator.SetInteger("Direction", facingHorizontalDirection);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 drawPos = Application.isPlaying ? targetPosition : transform.position;
            Gizmos.DrawWireCube(Application.isPlaying ? GetFootPositionFromWorld(drawPos) : GetFootPosition(), checkBoxSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Application.isPlaying ? GetFootPositionFromWorld(startPosition) : GetFootPosition(), 0.15f);
        }

        private Vector3 GetFootPosition()
        {
            return transform.position + (Vector3)footOffset;
        }

        private Vector3 GetFootPositionFromWorld(Vector3 worldPos)
        {
            return worldPos + (Vector3)footOffset;
        }
    }
}