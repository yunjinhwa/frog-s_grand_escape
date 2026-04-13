using UnityEngine;
using System.Collections;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerController_Original : MonoBehaviour
    {
        private Rigidbody2D rb;

        private Animator animator;

        public float movementSpeed = 5f;
        public float crouchSpeedFactor = 0.5f;
        public bool isCrouching = false;
        public bool canMove = true; // 플레이어가 이동할 수 있는지 제어
        public float attackMoveLockDuration = 0.8f; // 공격 중 이동을 잠그는 시간(초)
        public bool isGrounded = false; // 플레이어는 공중에서 시작함
        public bool isCurrentlyJumping = false;
        public int jumpCount = 0;
        public float jumpForce = 7f;
        public float jumpDelay = 0.4f; // 애니메이션 동기화를 위한 점프 딜레이
        public float jumpCooldown = 0.2f; // 점프 후 쿨다운 시간

        public bool isWallHanging = false;
        public float wallSlideSpeed = 1f;  // 벽을 타고 내려올 때의 감소된 속도
        public Vector2 wallJumpForce = new Vector2(5f, 7f);  // 벽 점프 시 적용되는 힘
        private Vector3 lastWallContactPoint;  // 마지막으로 벽과 접촉한 지점 저장
        public float wallClimbSpeed = 2f;
        public bool isClimbingLedge = false;
        public Transform leftLedgeDetector;
        public Transform rightLedgeDetector;
        public bool isAirSlamming = false;
        public float airSlamForce = 20f;  // 에어 슬램 중 아래 방향으로 가해지는 힘

        public float airDashForce = 10f;  // 공중 대시 중 수평 방향으로 가해지는 힘
        public bool isAirDashing = false;  // 현재 공중 대시가 활성화되어 있는지 확인
        public float dashCooldown = 1.0f;
        private float lastDashTime = 0;

        public EdgeCollider2D edgeCollider; // Edge Collider 참조

        public GameObject aoEPrefab;

        void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            edgeCollider = GetComponent<EdgeCollider2D>();
        }

        void Update()
        {
            if (canMove && !isClimbingLedge)
            {
                HandleMovement();
                HandleCrouching();
                HandleAttacking();
                HandleJumping();
                HandleAirDash();
                HandleGroundDash();
                HandleSliding();

                // 특수 능력 처리
                HandleSpecialAbility1();
                HandleSpecialAbility2();

                // 피격 및 사망 처리
                HandleTakingDamage();
                HandleTemporaryDeath();
                CheckForLedges();



                if (isWallHanging)
                {
                    jumpCount = 0; // 이후 점프는 지상 점프 로직이 처리할 수 있도록 2단 점프 횟수 초기화
                }
                if (rb.linearVelocity.y < 0) // 예: 절벽에서 떨어지는 경우
                {
                    isGrounded = false;
                    isCurrentlyJumping = true;
                }
            }
        }

        private void HandleMovement()
        {
            if (!canMove)
                return;

            // 왼쪽 또는 오른쪽 키 입력을 직접 확인
            bool isPressingLeft = Input.GetKey(KeyCode.A);
            bool isPressingRight = Input.GetKey(KeyCode.D);
            bool isMoving = isPressingLeft || isPressingRight;

            // 플레이어가 웅크리고 있는지에 따라 속도 결정
            float speed = isCrouching ? movementSpeed * crouchSpeedFactor : movementSpeed;

            // 입력 키를 기준으로 직접 방향 설정
            if (isPressingRight)
            {
                animator.SetInteger("Direction", 0); // 0은 오른쪽(동쪽) 이동이라고 가정
                rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
            }
            else if (isPressingLeft)
            {
                animator.SetInteger("Direction", 1); // 1은 왼쪽(서쪽) 이동이라고 가정
                rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // 수평 이동 없음
            }

            // Animator의 걷기 상태 업데이트
            animator.SetBool("isWalking", isMoving);

            // 낙하 및 웅크리기 애니메이션 처리
            if (!isGrounded && rb.linearVelocity.y < 0)
            {
                animator.SetBool("isSliding", false);
                animator.SetBool("isFalling", true);
            }

            animator.SetBool("isCrouchingWalking", isCrouching && isMoving);
        }

        private void HandleCrouching()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                isCrouching = true;
                animator.SetBool("isCrouching", isCrouching);
            }
            else if (Input.GetKeyUp(KeyCode.C))
            {
                isCrouching = false;
                animator.SetBool("isCrouching", isCrouching);
            }
        }

        private void HandleAttacking()
        {
            if (Input.GetMouseButtonDown(0) && isGrounded) // 공격은 마우스 왼쪽 버튼
            {
                bool wasMovingWhenAttacked = Mathf.Abs(rb.linearVelocity.x) > 0;
                float attackDirection = Mathf.Sign(rb.linearVelocity.x); // 속도를 기준으로 공격 방향 가져오기

                // 공격 시 이동 중이었는지에 따라 적절한 애니메이션 실행
                if (wasMovingWhenAttacked)
                {
                    animator.SetTrigger("isRunAttack");
                    StartCoroutine(ApplyRunningAttackForce(attackDirection));
                }
                else
                {
                    animator.SetTrigger("isAttack1");
                    StartCoroutine(AttackMoveLock(attackMoveLockDuration));
                }
            }
        }

        IEnumerator ApplyRunningAttackForce(float direction)
        {
            float initialForce = 2f; // 공격 방향으로 플레이어가 계속 움직이도록 할 힘
            rb.AddForce(new Vector2(direction * initialForce, 0), ForceMode2D.Impulse);
            yield return StartCoroutine(AttackMoveLock(0.5f)); // 0.5초 동안 이동 잠금, 필요에 따라 조정
        }

        IEnumerator AttackMoveLock(float duration)
        {
            canMove = false;
            yield return new WaitForSeconds(duration);
            canMove = true;
        }

        // 점프 로직:
        private void HandleJumping()
        {
            if (canMove)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (isWallHanging)
                    {
                        JumpOffWall();
                    }
                    else if (isGrounded || jumpCount < 2) // 일반 점프와 2단 점프 처리
                    {
                        StartCoroutine(PerformJump());
                    }
                }
                else if (!isGrounded && !isWallHanging && Input.GetKeyDown(KeyCode.S)) // 공중에 있고 S 키가 눌렸는지 확인
                {
                    StartAirSlam();
                }
            }
        }

        private void JumpOffWall()
        {
            float directionMultiplier = transform.position.x > lastWallContactPoint.x ? -1 : 1;
            animator.SetTrigger("isWallJump");
            Vector2 jumpForce = new Vector2(wallJumpForce.x * directionMultiplier, wallJumpForce.y);
            rb.AddForce(jumpForce, ForceMode2D.Impulse);
            isWallHanging = false;
            isGrounded = false;
            isCurrentlyJumping = true;
            jumpCount++;
        }

        IEnumerator PerformJump()
        {
            if (jumpCount == 0 && isGrounded)
            {
                bool isMoving = animator.GetBool("isWalking");
                animator.SetTrigger(isMoving ? "isJumpRunStart" : "isJumpStart");
                yield return new WaitForSeconds(0.1f);  // 첫 점프에만 딜레이 적용
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                isGrounded = false;
                isCurrentlyJumping = true;
                jumpCount++;
            }
            else // 2단 점프
            {
                // 먼저 기존의 아래 방향 속도를 제거
                if (rb.linearVelocity.y < 0)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);  // 수직 속도 초기화
                }

                // 2단 점프 애니메이션 실행
                animator.SetTrigger("isDoubleJump");

                // 그 다음 2단 점프 힘 적용
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                isGrounded = false;
                isCurrentlyJumping = true;
                jumpCount++;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.otherCollider == edgeCollider) // 이 GameObject에서 충돌한 콜라이더가 Edge Collider인지 확인
            {
                if (collision.gameObject.CompareTag("Wall") && !isGrounded && !isAirSlamming)
                {
                    isWallHanging = true;
                    isCurrentlyJumping = false;
                    lastWallContactPoint = collision.contacts[0].point;
                    rb.gravityScale = 0; // 벽에 매달린 동안 중력 무효화
                    animator.SetBool("isWallHang", true); // 벽 매달리기 애니메이션 상태 설정

                    if (isAirDashing)
                    {
                        isAirDashing = false;
                        canMove = true; // 대시 중단 후 이동 가능하게 함
                    }
                }
            }

            if (collision.gameObject.CompareTag("Ground"))
            {
                // 지면 관련 충돌 로직 처리
                HandleGroundCollision();
            }
        }

        void HandleGroundCollision()
        {
            isGrounded = true;
            jumpCount = 0;
            isWallHanging = false;
            rb.gravityScale = 1;
            isCurrentlyJumping = false;

            bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0;
            if (isAirSlamming && isGrounded)
            {
                animator.SetTrigger("isAirSlamLand");
                isAirSlamming = false;
                // AoE 프리팹을 생성하고 파괴 코루틴 시작
                GameObject aoEInstance = Instantiate(aoEPrefab, transform.position, Quaternion.identity);
                StartCoroutine(DestroyAfterDelay(aoEInstance, 0.2f));  // 0.2초 후 파괴

                animator.SetBool("isAirSlam", false);
            }
            else
            {
                animator.SetTrigger(isMoving ? "isLandingRunning" : "isLanding");
            }

            if (isAirDashing)
            {
                isAirDashing = false;
            }
            canMove = true;
            animator.SetBool("isJumpMid", false);
            animator.SetBool("isFalling", false);
        }

        // AoE 프리팹을 파괴하는 보조 메서드
        IEnumerator DestroyAfterDelay(GameObject objectToDestroy, float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(objectToDestroy);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                isWallHanging = false;
                rb.gravityScale = 1; // 일반 중력 복원
                animator.SetBool("isWallHang", false);
                animator.SetBool("isWallSlide", false);
            }
        }

        // 에어 슬램
        private void StartAirSlam()
        {
            if (!isAirSlamming) // 에어 슬램이 이미 활성화되어 있지 않은지 확인
            {
                StartCoroutine(PerformAirSlam());
            }
        }

        IEnumerator PerformAirSlam()
        {
            isAirSlamming = true;
            animator.SetBool("isAirSlam", true);
            canMove = false;
            yield return new WaitForSeconds(0.5f); // 애니메이션이 시작되도록 0.5초 대기

            // 딜레이 후 아래 방향 힘 적용
            rb.linearVelocity = new Vector2(0, -airSlamForce); // 수평 이동을 멈추고 강한 아래 방향 힘 적용
        }

        // 대시:
        void HandleAirDash()
        {
            if (Input.GetMouseButtonDown(1) && !isGrounded && !isWallHanging)  // 공중 대시는 마우스 오른쪽 버튼
            {
                if (canMove && !isAirDashing && Time.time >= lastDashTime + dashCooldown)  // 이동 가능, 현재 대시 중 아님, 쿨다운 경과 확인
                {
                    float horizontalInput = Input.GetAxisRaw("Horizontal");  // 현재 수평 입력값 가져오기
                    StartCoroutine(PerformAirDash(horizontalInput));
                    lastDashTime = Time.time;  // 마지막 대시 시간을 현재 시간으로 갱신
                }
            }
        }

        IEnumerator PerformAirDash(float directionInput)
        {
            isAirDashing = true;
            canMove = false;  // 대시 동안 다른 이동 조작 잠금
            float originalGravityScale = rb.gravityScale;
            rb.gravityScale = 0;

            float dashDirectionX = 0;
            float dashDirectionY = 0;

            // 위쪽 대시를 위해 W 키가 눌려 있는지 확인
            if (Input.GetKey(KeyCode.W))
            {
                dashDirectionY = 1;  // W가 눌려 있으면 위쪽 힘 추가
                animator.SetTrigger("isAirDashUpward");  // 위쪽 대시 애니메이션 실행
            }

            // 순수 위쪽 대시가 아닐 경우에만 수평 방향 확인
            if (dashDirectionY == 0)
            {
                if (directionInput < 0)
                {
                    dashDirectionX = -1;  // A 키가 눌리면 왼쪽 대시
                    animator.SetTrigger("isAirDashAttack"); // 수평 대시 애니메이션 실행
                }
                else if (directionInput > 0)
                {
                    dashDirectionX = 1;   // D 키가 눌리면 오른쪽 대시
                    animator.SetTrigger("isAirDashAttack");  // 수평 대시 애니메이션 실행
                }
            }
            else
            {
                // 위쪽 대시일 경우 대각선 이동을 위해 수평 방향도 고려
                if (directionInput < 0)
                {
                    dashDirectionX = -1;  // 왼쪽 위 대각선 대시를 위한 왼쪽 힘 추가
                }
                else if (directionInput > 0)
                {
                    dashDirectionX = 1;   // 오른쪽 위 대각선 대시를 위한 오른쪽 힘 추가
                }
            }

            // 실제로 방향이 설정된 경우에만 대시 힘 적용
            if (dashDirectionX != 0 || dashDirectionY != 0)
            {
                rb.linearVelocity = new Vector2(dashDirectionX * airDashForce, dashDirectionY * airDashForce);
                yield return new WaitForSeconds(0.7f);  // 대시 효과 지속 시간
            }

            rb.gravityScale = originalGravityScale;  // 원래 중력 값 복원
            canMove = true;
            isAirDashing = false;
        }

        // 지상 대시:
        void HandleGroundDash()
        {
            if (Input.GetMouseButtonDown(1) && isGrounded)  // 지상 대시는 마우스 오른쪽 버튼
            {
                float horizontalInput = Input.GetAxisRaw("Horizontal");  // 현재 수평 입력값 가져오기
                if (horizontalInput != 0 && canMove)  // 이동 가능하고 수평 입력이 있는지 확인
                {
                    StartCoroutine(PerformGroundDash(horizontalInput));
                }
            }
        }

        IEnumerator PerformGroundDash(float directionInput)
        {
            if (Input.GetMouseButtonDown(1) && isGrounded && Time.time >= lastDashTime + dashCooldown)  // 지상 대시 + 쿨다운 확인
            {
                if (directionInput != 0 && canMove)  // 이동 가능하고 수평 입력이 있는지 확인
                {
                    isAirDashing = true;  // 대시 상태 표시
                    canMove = false;  // 대시 동안 다른 이동 조작 잠금
                    float dashDirectionX = directionInput < 0 ? -1 : 1;

                    // 지상 대시 애니메이션 실행
                    animator.SetTrigger("isDashForward");

                    // 대시를 위한 힘 적용
                    rb.AddForce(new Vector2(dashDirectionX * airDashForce, 0), ForceMode2D.Impulse);

                    yield return new WaitForSeconds(0.5f);  // 대시 효과 지속 시간

                    // 대시 후 상태와 조작 복원
                    canMove = true;
                    isAirDashing = false;

                    lastDashTime = Time.time;  // 마지막 대시 시간을 현재 시간으로 갱신
                }
            }
        }

        void FixedUpdate()
        {
            // 벽 타기 및 점프 애니메이션 확인
            if (!isGrounded && isCurrentlyJumping)
            {
                if (rb.linearVelocity.y > 0)
                {
                    animator.SetBool("isFalling", false);
                    animator.SetBool("isJumpMid", true);
                }
                else if (rb.linearVelocity.y <= 0)
                {
                    animator.SetBool("isJumpMid", false);
                    animator.SetBool("isFalling", true);
                }
            }
            else if (!isGrounded && !isCurrentlyJumping && !isWallHanging && !isCrouching) // 안전 장치용 로직
            {
                isGrounded = true;
                animator.SetBool("isFalling", false);
            }

            if (isGrounded && !isCurrentlyJumping)
            {
                animator.SetBool("isFalling", false);
                animator.SetBool("isJumpMid", false);
            }

            // 벽 매달리기 동작과 애니메이션 처리
            if (isWallHanging && !isAirSlamming)
            {
                float moveVertical = Input.GetAxisRaw("Vertical");

                // 벽에서의 수직 이동 처리
                if (moveVertical > 0)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallClimbSpeed);
                    animator.SetBool("isClimbing", true);
                }
                else if (moveVertical < 0)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
                    animator.SetBool("isWallSlide", true);
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                    animator.SetBool("isWallSlide", false);
                    animator.SetBool("isClimbing", false);
                }
            }
            else
            {
                animator.SetBool("isClimbing", false);  // 벽에 매달려 있지 않으면 climbing 비활성화 보장
                animator.SetBool("isWallSlide", false); // 벽에 매달려 있지 않으면 wall slide 비활성화 보장
            }
        }

        void HandleSliding()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded && !isCrouching)
            {
                animator.SetBool("isSliding", true);
                animator.SetTrigger("isSlideStart");
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                animator.SetTrigger("isSlideEnd");
                animator.SetBool("isSliding", false);
            }
        }

        // 능력:
        private void HandleSpecialAbility1()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && isGrounded) // 1 키가 눌렸는지 확인
            {
                StartCoroutine(AttackMoveLock(attackMoveLockDuration));
                animator.SetTrigger("isUsingSpecialAbility1"); // 특수 능력 1 애니메이션 실행
            }
        }

        private void HandleSpecialAbility2()
        {
            if (Input.GetKeyDown(KeyCode.Alpha2) && isGrounded) // 2 키가 눌렸는지 확인
            {
                StartCoroutine(AttackMoveLock(attackMoveLockDuration));
                animator.SetTrigger("isUsingSpecialAbility2"); // 특수 능력 2 애니메이션 실행
            }
        }

        private void HandleTakingDamage()
        {
            if (Input.GetKeyDown(KeyCode.F) && isGrounded) // F 키가 눌렸는지 확인
            {
                StartCoroutine(AttackMoveLock(attackMoveLockDuration));
                animator.SetTrigger("isTakingDamage"); // 피격 애니메이션 실행
            }
        }

        private void HandleTemporaryDeath()
        {
            if (Input.GetKeyDown(KeyCode.V) && isGrounded) // V 키가 눌렸는지 확인
            {
                StartCoroutine(AttackMoveLock(attackMoveLockDuration));
                StartCoroutine(TemporaryDeath());
            }
        }

        IEnumerator TemporaryDeath()
        {
            animator.SetBool("isDead", true); // 사망 애니메이션/상태 실행
            yield return new WaitForSeconds(1); // 1초 대기
            animator.SetBool("isDead", false); // 사망 상태 초기화
        }

        void CheckForLedges()
        {
            // 참고: "Default" 레이어 대신 "Ledge" 레이어를 만들고 모든 "Ledge" 오브젝트를 그 레이어로 설정하는 것이 좋음.
            int ledgeLayer = LayerMask.GetMask("Default");
            float rayLength = 0.22f;
            float angleDegrees = 45;  // 대각선 레이를 위한 각도 조정

            // 대각선 레이를 위한 방향 벡터 계산
            Vector2 leftUp = Quaternion.Euler(0, 0, angleDegrees) * Vector2.left;
            Vector2 leftDown = Quaternion.Euler(0, 0, -angleDegrees) * Vector2.left;
            Vector2 rightUp = Quaternion.Euler(0, 0, -angleDegrees) * Vector2.right;
            Vector2 rightDown = Quaternion.Euler(0, 0, angleDegrees) * Vector2.right;

            // 대각선 방향으로 레이캐스트 수행
            RaycastHit2D hitLeftUp = Physics2D.Raycast(leftLedgeDetector.position, leftUp, rayLength, ledgeLayer);
            RaycastHit2D hitLeftDown = Physics2D.Raycast(leftLedgeDetector.position, leftDown, rayLength, ledgeLayer);
            RaycastHit2D hitRightUp = Physics2D.Raycast(rightLedgeDetector.position, rightUp, rayLength, ledgeLayer);
            RaycastHit2D hitRightDown = Physics2D.Raycast(rightLedgeDetector.position, rightDown, rayLength, ledgeLayer);

            // Scene 뷰에서 레이캐스트를 시각화하기 위한 디버그 선
            Debug.DrawLine(leftLedgeDetector.position, leftLedgeDetector.position + (Vector3)leftUp * rayLength, Color.red);
            Debug.DrawLine(leftLedgeDetector.position, leftLedgeDetector.position + (Vector3)leftDown * rayLength, Color.blue);
            Debug.DrawLine(rightLedgeDetector.position, rightLedgeDetector.position + (Vector3)rightUp * rayLength, Color.red);
            Debug.DrawLine(rightLedgeDetector.position, rightLedgeDetector.position + (Vector3)rightDown * rayLength, Color.blue);

            // 충돌 여부를 확인하고 ledge climb 처리
            if (hitLeftUp.collider != null && hitLeftUp.collider.CompareTag("Ledge"))
            {
                HandleLedgeClimb(hitLeftUp, 1);  // 1은 왼쪽
            }
            else if (hitLeftDown.collider != null && hitLeftDown.collider.CompareTag("Ledge"))
            {
                HandleLedgeClimb(hitLeftDown, 1);  // 1은 왼쪽
            }
            if (hitRightUp.collider != null && hitRightUp.collider.CompareTag("Ledge"))
            {
                HandleLedgeClimb(hitRightUp, 0);  // 0은 오른쪽
            }
            else if (hitRightDown.collider != null && hitRightDown.collider.CompareTag("Ledge"))
            {
                HandleLedgeClimb(hitRightDown, 0);  // 0은 오른쪽
            }
        }

        void HandleLedgeClimb(RaycastHit2D hit, int direction)
        {
            StartCoroutine(ClimbOntoLedge(hit.collider.bounds, direction));
        }

        IEnumerator ClimbOntoLedge(Bounds ledgeBounds, int direction)
        {
            animator.SetTrigger("isLedgeClimbing");  // 오르기 애니메이션 실행
            animator.SetInteger("Direction", direction);  // Animator에 방향 설정
            animator.SetBool("isWalking", false);

            isClimbingLedge = true;
            canMove = false;  // 이동 비활성화

            // 원래 중력값과 속도 저장
            float originalGravityScale = rb.gravityScale;
            Vector2 originalVelocity = rb.linearVelocity;

            // 콜라이더 비활성화
            EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
            CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
            if (edgeCollider != null) edgeCollider.enabled = false;
            if (capsuleCollider != null) capsuleCollider.enabled = false;

            // 모든 이동과 중력 정지
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;  // 즉시 모든 이동 정지

            yield return new WaitForSeconds(0.66f); // 애니메이션에 맞게 시간 조정

            // 캐릭터가 ledge 위에 정확히 올라가도록 새 위치 계산
            float newYPosition = ledgeBounds.max.y + (0.65f / 2); // 캐릭터 높이
            float newXPosition = transform.position.x + (direction == 0 ? 0.3f : -0.3f); // 필요 시 좌우 위치 조정

            transform.position = new Vector3(newXPosition, newYPosition, transform.position.z);
            animator.SetBool("isWalking", false);
            rb.linearVelocity = Vector2.zero;

            // 콜라이더 다시 활성화
            if (edgeCollider != null) edgeCollider.enabled = true;
            if (capsuleCollider != null) capsuleCollider.enabled = true;

            // 원래 설정 복원
            rb.gravityScale = 1;

            canMove = true;  // 이동 다시 활성화
            yield return new WaitForSeconds(0.6f);  // 추가 대기 시간
            isClimbingLedge = false;  // climbing 플래그 초기화
        }
    }
}