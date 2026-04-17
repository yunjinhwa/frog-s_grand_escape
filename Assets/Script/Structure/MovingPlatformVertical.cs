using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatformVertical : MonoBehaviour
{
    public enum PlatformMoveMode
    {
        PingPong,
        Flow
    }

    public enum FlowDirection
    {
        Up,
        Down
    }

    [Header("Mode")]
    [SerializeField] private PlatformMoveMode moveMode = PlatformMoveMode.PingPong;

    [Header("Common Move Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool useLocalPosition = false;

    [Header("PingPong Settings")]
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private bool startGoingUp = true;

    [Header("Flow Settings")]
    [SerializeField] private FlowDirection flowDirection = FlowDirection.Down;

    [Header("Destroy Settings")]
    [SerializeField] private float destroyViewportMargin = 0.2f;
    [SerializeField] private bool destroyWhenOffScreen = true;

    [Header("Sprite")]
    [SerializeField] private bool flipSpriteOnDirectionChange = true;

    private IPlatformMovement movementStrategy;
    private PingPongPlatformMovement pingPongMovement;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private Vector3 lastWorldPosition;

    public Vector3 DeltaMovement { get; private set; }

    private void Reset()
    {
        ApplyDefaultPhysicsSettings();
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyDefaultPhysicsSettings();
        BuildMovementStrategy();
        // 초기 시작 위치를 저장 (로컬/월드 선택 반영)
        startPosition = useLocalPosition ? transform.localPosition : transform.position;

        lastWorldPosition = transform.position;
        UpdateSpriteFlip();
    }

    private void Update()
    {
        movementStrategy.Tick(Time.deltaTime);

        DeltaMovement = transform.position - lastWorldPosition;
        lastWorldPosition = transform.position;

        UpdateSpriteFlip();

        if (moveMode == PlatformMoveMode.Flow && destroyWhenOffScreen)
        {
            CheckOffScreenAndDestroy();
        }
    }

    private void ApplyDefaultPhysicsSettings()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void BuildMovementStrategy()
    {
        switch (moveMode)
        {
            case PlatformMoveMode.PingPong:
                pingPongMovement = new PingPongPlatformMovement(moveDistance, startGoingUp);
                movementStrategy = pingPongMovement;
                movementStrategy.Initialize(transform, useLocalPosition, moveSpeed);
                break;

            case PlatformMoveMode.Flow:
                FlowPlatformMovement.FlowDirection flow =
                    flowDirection == FlowDirection.Up
                    ? FlowPlatformMovement.FlowDirection.Up
                    : FlowPlatformMovement.FlowDirection.Down;

                movementStrategy = new FlowPlatformMovement(flow);
                movementStrategy.Initialize(transform, useLocalPosition, moveSpeed);
                pingPongMovement = null;
                break;
        }
    }

    private void UpdateSpriteFlip()
    {
        if (!flipSpriteOnDirectionChange || spriteRenderer == null || pingPongMovement == null)
            return;

        spriteRenderer.flipY = pingPongMovement.Direction < 0;
    }

    private void CheckOffScreenAndDestroy()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        Bounds bounds = GetWorldBounds();
        Vector3 min = cam.WorldToViewportPoint(bounds.min);
        Vector3 max = cam.WorldToViewportPoint(bounds.max);

        bool completelyAbove = min.y > 1f + destroyViewportMargin;
        bool completelyBelow = max.y < 0f - destroyViewportMargin;
        bool completelyLeft = max.x < 0f - destroyViewportMargin;
        bool completelyRight = min.x > 1f + destroyViewportMargin;

        if (completelyAbove || completelyBelow || completelyLeft || completelyRight)
        {
            // 화면 밖으로 나간 Flow 방식 플랫폼은 삭제하지 않고 시작 위치로 되돌립니다.
            if (useLocalPosition)
            {
                transform.localPosition = startPosition;
            }
            else
            {
                transform.position = startPosition;
            }

            // 순간 이동으로 인한 큰 DeltaMovement를 방지
            lastWorldPosition = transform.position;
            DeltaMovement = Vector3.zero;
        }
    }

    private Bounds GetWorldBounds()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            return rend.bounds;

        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null)
            return col2D.bounds;

        return new Bounds(transform.position, Vector3.one * 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = useLocalPosition ? transform.localPosition : transform.position;

        if (moveMode == PlatformMoveMode.PingPong)
        {
            Gizmos.color = Color.cyan;

            Vector3 top = origin + Vector3.up * moveDistance;
            Vector3 bottom = origin - Vector3.up * moveDistance;

            Gizmos.DrawLine(top, bottom);
            Gizmos.DrawWireSphere(top, 0.1f);
            Gizmos.DrawWireSphere(bottom, 0.1f);
        }
        else
        {
            Gizmos.color = Color.green;

            Vector3 dir = flowDirection == FlowDirection.Up ? Vector3.up : Vector3.down;
            Vector3 end = origin + dir * 2f;

            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(origin, 0.1f);
            Gizmos.DrawWireSphere(end, 0.1f);
        }
    }
}