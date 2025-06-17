using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGeneration
{
    public class DirectionManager
    {
        private Direction _startDirection;
        private Direction _endDirection;
        private Vector2Int _startVector;

        public Direction StartDirection => _startDirection;
        public Direction EndDirection => _endDirection;
        public Vector2Int StartVector => _startVector;

        public void InitializeDirections(Direction startDirection, Direction endDirection)
        {
            _startDirection = startDirection;
            _endDirection = endDirection;
        }

        public void GenerateDirections(Vector2Int startPoint, Vector2Int endPoint, int width)
        {
            _startDirection = GetStartDirection(startPoint, width);
            _endDirection = GetEndDirection(endPoint, width);

            _startVector = DirectionToVector(_startDirection);
        }

        private Direction GetStartDirection(Vector2Int point, int width)
        {
            if (point.x == 0) return Direction.Right;
            if (point.x == width - 1) return Direction.Left;
            if (point.y == 0) return Direction.Up;
            return Direction.Down;
        }

        private Direction GetEndDirection(Vector2Int point, int width)
        {
            if (point.x == 0) return Direction.Left;
            if (point.x == width - 1) return Direction.Right;
            if (point.y == 0) return Direction.Down;
            return Direction.Up;
        }

        // Converts a Direction enum to a Vector2Int for movement calculations
        public Vector2Int DirectionToVector(Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Vector2Int(0, 1),
                Direction.Down => new Vector2Int(0, -1),
                Direction.Left => new Vector2Int(-1, 0),
                Direction.Right => new Vector2Int(1, 0),
                _ => Vector2Int.zero
            };
        }

        public bool IsOpposite(Direction firstDirection, Direction secondDirection)
        {
            return (firstDirection == Direction.Up && secondDirection == Direction.Down) ||
                   (firstDirection == Direction.Down && secondDirection == Direction.Up) ||
                   (firstDirection == Direction.Left && secondDirection == Direction.Right) ||
                   (firstDirection == Direction.Right && secondDirection == Direction.Left);
        }

        public List<Direction> GetRandomDirections()
        {
            List<Direction> directions = new List<Direction>
            {
                Direction.Up,
                Direction.Down,
                Direction.Left,
                Direction.Right
            };

            return Shuffle(directions);
        }

        // Fisher-Yates shuffle algorithm to randomize the directions, loved that!
        private List<T> Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int k = Random.Range(0, i + 1);
                (list[i], list[k]) = (list[k], list[i]);
            }
            return list;
        }
    }
}