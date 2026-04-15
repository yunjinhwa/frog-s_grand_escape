using UnityEngine;

public class MovingPlatformVertical : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveDistance = 2f;   // 시작 위치 기준 위/아래 이동 거리
    [SerializeField] private float moveSpeed = 2f;      // 이동 속도
    [SerializeField] private bool startGoingUp = true;  // 시작 방향

    [Header("Optional")]
    [SerializeField] private bool useLocalPosition = false;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private int direction = 1;

    private void Start()
    {
        startPosition = useLocalPosition ? transform.localPosition : transform.position;
        direction = startGoingUp ? 1 : -1;
        SetNextTarget();
    }

    private void Update()
    {
        MovePlatform();
    }

    private void MovePlatform()
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
                SetNextTarget();
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
                SetNextTarget();
            }
        }
    }

    private void SetNextTarget()
    {
        Vector3 offset = Vector3.up * moveDistance * direction;

        if (useLocalPosition)
            targetPosition = startPosition + offset;
        else
            targetPosition = startPosition + offset;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 origin = Application.isPlaying
            ? startPosition
            : (useLocalPosition ? transform.localPosition : transform.position);

        Vector3 top = origin + Vector3.up * moveDistance;
        Vector3 bottom = origin - Vector3.up * moveDistance;

        Gizmos.DrawLine(top, bottom);
        Gizmos.DrawWireSphere(top, 0.1f);
        Gizmos.DrawWireSphere(bottom, 0.1f);
    }
}