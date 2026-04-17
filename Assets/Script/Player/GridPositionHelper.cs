using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class GridPositionHelper
    {
        private readonly float cellSize;
        private readonly Vector2 footOffset;

        public GridPositionHelper(float cellSize, Vector2 footOffset)
        {
            this.cellSize = cellSize;
            this.footOffset = footOffset;
        }

        public Vector3 SnapToGrid(Vector3 position)
        {
            float half = cellSize * 0.5f;
            float x = Mathf.Round((position.x - half) / cellSize) * cellSize + half;
            float y = Mathf.Round((position.y - half) / cellSize) * cellSize + half;
            return new Vector3(x, y, position.z);
        }

        public Vector3 GetFootPosition(Vector3 worldPosition)
        {
            return worldPosition + (Vector3)footOffset;
        }

        public Vector3 GetMoveDestination(Vector3 currentTargetPosition, Vector2 direction, int distanceInCells)
        {
            return currentTargetPosition + (Vector3)(direction * cellSize * distanceInCells);
        }
    }
}