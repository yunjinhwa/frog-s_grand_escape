using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class GridCollisionChecker
    {
        private readonly LayerMask obstacleLayer;
        private readonly LayerMask waterLayer;
        private readonly LayerMask rideableLayer;
        private readonly Vector2 checkBoxSize;
        private readonly GridPositionHelper positionHelper;

        public GridCollisionChecker(
            LayerMask obstacleLayer,
            LayerMask waterLayer,
            LayerMask rideableLayer,
            Vector2 checkBoxSize,
            GridPositionHelper positionHelper)
        {
            this.obstacleLayer = obstacleLayer;
            this.waterLayer = waterLayer;
            this.rideableLayer = rideableLayer;
            this.checkBoxSize = checkBoxSize;
            this.positionHelper = positionHelper;
        }

        public bool IsBlocked(Vector3 worldPosition)
        {
            Vector3 foot = positionHelper.GetFootPosition(worldPosition);
            Collider2D hit = Physics2D.OverlapBox(foot, checkBoxSize, 0f, obstacleLayer);
            return hit != null;
        }

        public bool IsWater(Vector3 worldPosition)
        {
            Vector3 foot = positionHelper.GetFootPosition(worldPosition);
            Collider2D hit = Physics2D.OverlapBox(foot, checkBoxSize, 0f, waterLayer);
            return hit != null;
        }

        public bool HasRideableObject(Vector3 worldPosition)
        {
            Vector3 foot = positionHelper.GetFootPosition(worldPosition);
            Collider2D hit = Physics2D.OverlapBox(foot, checkBoxSize, 0f, rideableLayer);
            return hit != null;
        }

        public MovingPlatformVertical GetCurrentPlatform(Vector3 worldPosition)
        {
            Vector3 foot = positionHelper.GetFootPosition(worldPosition);
            Collider2D hit = Physics2D.OverlapBox(foot, checkBoxSize, 0f, rideableLayer);
            return hit != null ? hit.GetComponent<MovingPlatformVertical>() : null;
        }

        public bool CanStandOn(Vector3 worldPosition)
        {
            if (IsBlocked(worldPosition))
                return false;

            if (IsWater(worldPosition) && !HasRideableObject(worldPosition))
                return false;

            return true;
        }

        public bool IsObstacleTouching(Vector3 worldPosition)
        {
            Vector3 foot = positionHelper.GetFootPosition(worldPosition);
            Collider2D hit = Physics2D.OverlapBox(foot, checkBoxSize, 0f, obstacleLayer);
            return hit != null;
        }
    }
}