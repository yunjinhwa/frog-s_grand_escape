using UnityEngine;

public class PingPongPlatformMovement : IPlatformMovement
{
    private readonly float moveDistance;
    private int direction;
    private Transform targetTransform;
    private bool useLocalPosition;
    private float moveSpeed;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    public int Direction => direction;
    public Vector3 CurrentWorldPosition => targetTransform.position;

    public PingPongPlatformMovement(float moveDistance, bool startGoingUp)
    {
        this.moveDistance = moveDistance;
        direction = startGoingUp ? 1 : -1;
    }

    public void Initialize(Transform targetTransform, bool useLocalPosition, float moveSpeed)
    {
        this.targetTransform = targetTransform;
        this.useLocalPosition = useLocalPosition;
        this.moveSpeed = moveSpeed;

        startPosition = useLocalPosition ? targetTransform.localPosition : targetTransform.position;
        UpdateNextTarget();
    }

    public void Tick(float deltaTime)
    {
        if (useLocalPosition)
        {
            targetTransform.localPosition = Vector3.MoveTowards(
                targetTransform.localPosition,
                targetPosition,
                moveSpeed * deltaTime
            );

            if (Vector3.Distance(targetTransform.localPosition, targetPosition) <= 0.001f)
            {
                direction *= -1;
                UpdateNextTarget();
            }
        }
        else
        {
            targetTransform.position = Vector3.MoveTowards(
                targetTransform.position,
                targetPosition,
                moveSpeed * deltaTime
            );

            if (Vector3.Distance(targetTransform.position, targetPosition) <= 0.001f)
            {
                direction *= -1;
                UpdateNextTarget();
            }
        }
    }

    private void UpdateNextTarget()
    {
        Vector3 basePosition = startPosition;
        Vector3 offset = Vector3.up * moveDistance * direction;
        targetPosition = basePosition + offset;
    }
}