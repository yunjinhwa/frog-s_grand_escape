using UnityEngine;

public class FlowPlatformMovement : IPlatformMovement
{
    public enum FlowDirection
    {
        Up,
        Down
    }

    private readonly FlowDirection flowDirection;
    private Transform targetTransform;
    private bool useLocalPosition;
    private float moveSpeed;

    public Vector3 CurrentWorldPosition => targetTransform.position;

    public FlowPlatformMovement(FlowDirection flowDirection)
    {
        this.flowDirection = flowDirection;
    }

    public void Initialize(Transform targetTransform, bool useLocalPosition, float moveSpeed)
    {
        this.targetTransform = targetTransform;
        this.useLocalPosition = useLocalPosition;
        this.moveSpeed = moveSpeed;
    }

    public void Tick(float deltaTime)
    {
        Vector3 direction = flowDirection == FlowDirection.Up ? Vector3.up : Vector3.down;
        Vector3 delta = direction * moveSpeed * deltaTime;

        if (useLocalPosition)
            targetTransform.localPosition += delta;
        else
            targetTransform.position += delta;
    }
}