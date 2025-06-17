using UnityEngine;

namespace ProceduralGeneration
{
    public class GridManager
    {
        private Element[,] _grid;
        private int _width;
        private int _height;
        private Vector2Int _startPoint;
        private Vector2Int _endPoint;

        public Element[,] Grid => _grid;
        public Vector2Int StartPoint => _startPoint;
        public Vector2Int EndPoint => _endPoint;

        public bool InitializeGrid(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                Debug.LogError("Grid dimensions must be greater than zero.");
                return false;
            }

            _width = width;
            _height = height;

            _grid = new Element[_width, _height];

            return true;
        }

        public bool GenerateGrid()
        {
            ResetGrid();

            _startPoint = GetRandomBorderPoint();
            _endPoint = GetRandomBorderPoint();

            if (_startPoint == _endPoint) return false;

            return true;
        }

        public State PlaceTiles(bool needsJoint, Direction direction, Direction endDirection, Vector2Int currentPosition, Vector2Int firstCorridor, Vector2Int secondCorridor, Vector2Int nextPoint)
        {
            if (!IsInsideGrid(firstCorridor) || !IsInsideGrid(secondCorridor)) return State.Failed;
            if (!IsCellEmpty(firstCorridor) || !IsCellEmpty(secondCorridor)) return State.Failed;

            if (needsJoint && _grid[currentPosition.x, currentPosition.y] == Element.Joint)
                return State.Failed;

            if (secondCorridor == _endPoint)
            {
                if (direction != endDirection) return State.Failed;

                SetTiles(needsJoint, currentPosition, firstCorridor, secondCorridor);
                return State.Finished;
            }

            if (!IsInsideGrid(nextPoint)) return State.Failed;
            if (!IsCellEmpty(nextPoint)) return State.Failed;

            SetTiles(needsJoint, currentPosition, firstCorridor, secondCorridor);

            return State.InProgress;
        }

        private void SetTiles(bool needsJoint, Vector2Int currentPosition, Vector2Int firstCorridor, Vector2Int secondCorridor)
        {
            if (needsJoint) _grid[currentPosition.x, currentPosition.y] = Element.Joint;
            _grid[firstCorridor.x, firstCorridor.y] = Element.Corridor;
            _grid[secondCorridor.x, secondCorridor.y] = Element.Corridor;
        }

        public void Backtrack(bool needsJoint, Vector2Int currentPosition, Vector2Int firstCorridor, Vector2Int secondCorridor)
        {
            if (needsJoint) _grid[currentPosition.x, currentPosition.y] = Element.None;
            _grid[firstCorridor.x, firstCorridor.y] = Element.None;
            _grid[secondCorridor.x, secondCorridor.y] = Element.None;
        }

        private bool IsInsideGrid(Vector2Int point)
        {
            return point.x >= 0 && point.x < _width && point.y >= 0 && point.y < _height;
        }

        private bool IsCellEmpty(Vector2Int point)
        {
            return _grid[point.x, point.y] == Element.None;
        }

        private Vector2Int GetRandomBorderPoint()
        {
            int side = Random.Range(0, 4);

            return side switch
            {
                0 => new Vector2Int(Random.Range(0, _width), 0), // Bottom
                1 => new Vector2Int(Random.Range(0, _width), _height - 1), // Top
                2 => new Vector2Int(0, Random.Range(0, _height)), // Left
                3 => new Vector2Int(_width - 1, Random.Range(0, _height)), // Right
                _ => Vector2Int.zero
            };
        }

        private void ResetGrid()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _grid[x, y] = Element.None;
                }
            }
        }
    }
}