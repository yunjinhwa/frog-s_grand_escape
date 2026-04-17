using UnityEngine;

public interface IPlatformMovement
{
    void Initialize(Transform targetTransform, bool useLocalPosition, float moveSpeed);
    void Tick(float deltaTime);
    Vector3 CurrentWorldPosition { get; }
}