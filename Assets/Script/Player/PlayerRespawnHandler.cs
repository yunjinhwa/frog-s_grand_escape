using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerRespawnHandler
    {
        private readonly GridAnimationController animationController;

        public PlayerRespawnHandler(GridAnimationController animationController)
        {
            this.animationController = animationController;
        }

        public void RespawnFromWater(
            Transform playerTransform,
            ref Vector3 targetPosition,
            ref MovingPlatformVertical currentPlatform,
            Vector3 startPosition)
        {
            GameState.Instance.DecreaseHP(1);

            playerTransform.position = startPosition;
            targetPosition = startPosition;
            currentPlatform = null;

            animationController.EndMoveAnimation();
            Debug.Log("강에 빠짐! 시작 위치로 복귀");
        }

        public void RespawnFromObstacle(
            Transform playerTransform,
            ref Vector3 targetPosition,
            ref MovingPlatformVertical currentPlatform,
            Vector3 startPosition)
        {
            GameState.Instance.DecreaseHP(1);

            playerTransform.position = startPosition;
            targetPosition = startPosition;
            currentPlatform = null;

            animationController.EndMoveAnimation();
            Debug.Log("장애물에 부딪힘! 시작 위치로 복귀");
        }
    }
}