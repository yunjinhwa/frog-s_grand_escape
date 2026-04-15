using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatformVertical : MonoBehaviour
{
    public enum PlatformMoveMode
    {
        PingPong,   // 위아래 왕복
        Flow        // 한 방향으로 계속 이동 후 화면 밖에서 삭제
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

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 lastWorldPosition;
    private int direction = 1;

    public Vector3 DeltaMovement { get; private set; }

    private void Start()
    {
        startPosition = useLocalPosition ? transform.localPosition : transform.position;

        direction = startGoingUp ? 1 : -1;
        SetNextTargetForPingPong();

        lastWorldPosition = transform.position;
    }

    private void Update()
    {
        MovePlatform();

        DeltaMovement = transform.position - lastWorldPosition;
        lastWorldPosition = transform.position;

        if (moveMode == PlatformMoveMode.Flow && destroyWhenOffScreen)
        {
            CheckOffScreenAndDestroy();
        }
    }

    private void MovePlatform()
    {
        switch (moveMode)
        {
            case PlatformMoveMode.PingPong:
                MovePingPong();
                break;

            case PlatformMoveMode.Flow:
                MoveFlow();
                break;
        }
    }

    private void MovePingPong()
    {
        if (useLocalPosition)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.localPosition, targetPosition) <= 0.001f)
            {
                direction *= -1;
                SetNextTargetForPingPong();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) <= 0.001f)
            {
                direction *= -1;
                SetNextTargetForPingPong();
            }
        }
    }

    private void MoveFlow()
    {
        Vector3 moveDir = (flowDirection == FlowDirection.Up) ? Vector3.up : Vector3.down;
        Vector3 delta = moveDir * moveSpeed * Time.deltaTime;

        if (useLocalPosition)
            transform.localPosition += delta;
        else
            transform.position += delta;
    }

    private void SetNextTargetForPingPong()
    {
        Vector3 offset = Vector3.up * moveDistance * direction;
        targetPosition = startPosition + offset;
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
            Destroy(gameObject);
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
        Vector3 origin;

        if (Application.isPlaying)
            origin = startPosition;
        else
            origin = useLocalPosition ? transform.localPosition : transform.position;

        if (moveMode == PlatformMoveMode.PingPong)
        {
            Gizmos.color = Color.cyan;

            Vector3 top = origin + Vector3.up * moveDistance;
            Vector3 bottom = origin - Vector3.up * moveDistance;

            Gizmos.DrawLine(top, bottom);
            Gizmos.DrawWireSphere(top, 0.1f);
            Gizmos.DrawWireSphere(bottom, 0.1f);
        }
        else if (moveMode == PlatformMoveMode.Flow)
        {
            Gizmos.color = Color.green;

            Vector3 dir = (flowDirection == FlowDirection.Up) ? Vector3.up : Vector3.down;
            Vector3 end = origin + dir * 2f;

            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(origin, 0.1f);
            Gizmos.DrawWireSphere(end, 0.1f);
        }
    }
}