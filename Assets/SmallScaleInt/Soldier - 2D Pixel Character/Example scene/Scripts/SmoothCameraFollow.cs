using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class SmoothCameraFollow : MonoBehaviour
    {
        public Transform target; // 카메라가 따라갈 대상
        public float smoothTime = 0.3f; // 위치를 부드럽게 보정하는 데 걸리는 시간
        public Vector3 offset; // 대상과의 위치 오프셋

        private Vector3 velocity = Vector3.zero; // SmoothDamp에서 사용할 속도 값

        void LateUpdate()
        {
            if (target == null) return;

            // 카메라가 이동하려는 목표 위치 계산
            Vector3 targetPosition = target.position + offset;

            // 해당 위치를 향해 카메라를 부드럽게 이동
            Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            // 새 위치를 카메라에 적용. z축 위치는 고정된 오프셋을 유지하도록 강제
            transform.position = new Vector3(newPosition.x, newPosition.y, offset.z);
        }
    }
}