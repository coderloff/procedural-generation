using System.Collections.Generic;
using UnityEngine;

public class ProceduralPathGenerator : MonoBehaviour
{
    private enum Element
    {
        None,
        Corridor,
        Joint
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;

    [Header("Visualization Objects")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject corridorPrefab;
    [SerializeField] private GameObject jointPrefab;

    private Element[,] _grid;
    private Vector2Int _startPoint;
    private Vector2Int _endPoint;
    private Direction _startDirection;
    private Direction _endDirection;

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (gridWidth <= 0 || gridHeight <= 0)
        {
            Debug.LogError("Grid dimensions must be greater than zero.");
            return;
        }

        _grid = new Element[gridWidth, gridHeight];

        bool pathFound = false;

        while (!pathFound)
        {
            ResetGrid();

            _startPoint = GetRandomBorderPoint();
            _endPoint = GetRandomBorderPoint();

            if (_startPoint == _endPoint) continue;

            _startDirection = GetStartDirection(_startPoint);
            _endDirection = GetEndDirection(_endPoint);

            Vector2Int direction = DirectionToVector(_startDirection);

            Vector2Int firstCorridor = _startPoint;
            Vector2Int secondCorridor = _startPoint + direction;
            Vector2Int nextPoint = _startPoint + direction * 2;

            if (!IsInsideGrid(secondCorridor) || !IsInsideGrid(nextPoint)) continue;
            if (!IsCellEmpty(secondCorridor) || !IsCellEmpty(nextPoint)) continue;

            _grid[firstCorridor.x, firstCorridor.y] = Element.Corridor;
            _grid[secondCorridor.x, secondCorridor.y] = Element.Corridor;

            if (GenerateFrom(nextPoint, _startDirection))
            {
                pathFound = true;
                VisualizeGrid();
                Debug.Log("Path generated successfully!");
                Debug.Log($"Start: {_startPoint}, End: {_endPoint}");
                return;
            }
        }

        Debug.LogWarning("Failed to generate path after multiple attempts.");
    }

    bool GenerateFrom(Vector2Int currentPosition, Direction previousDirection)
    {
        List<Direction> directions = new List<Direction>
        {
            Direction.Up,
            Direction.Down,
            Direction.Left,
            Direction.Right
        };
        List<Direction> randomDirections = Shuffle(directions);
        
        foreach (Direction direction in randomDirections)
        {
            if (IsOpposite(previousDirection, direction)) continue;

            Vector2Int directionVector = DirectionToVector(direction);

            bool needsJoint = direction != previousDirection;

            Vector2Int firstCorridor = needsJoint ? currentPosition + directionVector : currentPosition;
            Vector2Int secondCorridor = needsJoint ? currentPosition + directionVector * 2 : currentPosition + directionVector;
            Vector2Int nextPosition = needsJoint ? currentPosition + directionVector * 3 : currentPosition + directionVector * 2;

            if (!IsInsideGrid(firstCorridor) || !IsInsideGrid(secondCorridor)) continue;
            if (!IsCellEmpty(firstCorridor) || !IsCellEmpty(secondCorridor)) continue;
            
            if (needsJoint && _grid[currentPosition.x, currentPosition.y] == Element.Joint)
                continue;
            
            if (secondCorridor == _endPoint)
            {
                if (direction != _endDirection) continue;

                if (needsJoint) _grid[currentPosition.x, currentPosition.y] = Element.Joint;
                _grid[firstCorridor.x, firstCorridor.y] = Element.Corridor;
                _grid[secondCorridor.x, secondCorridor.y] = Element.Corridor;
                return true;
            }

            if (!IsInsideGrid(nextPosition)) continue;
            if (!IsCellEmpty(nextPosition)) continue;

            if (needsJoint) _grid[currentPosition.x, currentPosition.y] = Element.Joint;
            _grid[firstCorridor.x, firstCorridor.y] = Element.Corridor;
            _grid[secondCorridor.x, secondCorridor.y] = Element.Corridor;

            if (GenerateFrom(nextPosition, direction))
                return true;

            // Backtrack
            _grid[firstCorridor.x, firstCorridor.y] = Element.None;
            _grid[secondCorridor.x, secondCorridor.y] = Element.None;
            if (needsJoint) _grid[currentPosition.x, currentPosition.y] = Element.None;
        }

        return false;
    }

    private Vector2Int GetRandomBorderPoint()
    {
        int side = Random.Range(0, 4);

        return side switch
        {
            0 => new Vector2Int(Random.Range(0, gridWidth), 0), // Bottom
            1 => new Vector2Int(Random.Range(0, gridWidth), gridHeight - 1), // Top
            2 => new Vector2Int(0, Random.Range(0, gridHeight)), // Left
            3 => new Vector2Int(gridWidth - 1, Random.Range(0, gridHeight)), // Right
            _ => Vector2Int.zero
        };
    }

    private Direction GetStartDirection(Vector2Int point)
    {
        if (point.x == 0) return Direction.Right;
        if (point.x == gridWidth - 1) return Direction.Left;
        if (point.y == 0) return Direction.Up;
        return Direction.Down;
    }

    private Direction GetEndDirection(Vector2Int point)
    {
        if (point.x == 0) return Direction.Left;
        if (point.x == gridWidth - 1) return Direction.Right;
        if (point.y == 0) return Direction.Down;
        return Direction.Up;
    }

    private Vector2Int DirectionToVector(Direction direction)
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

    private bool IsInsideGrid(Vector2Int point)
    {
        return point.x >= 0 && point.x < gridWidth && point.y >= 0 && point.y < gridHeight;
    }

    private bool IsCellEmpty(Vector2Int point)
    {
        return _grid[point.x, point.y] == Element.None;
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

    private bool IsOpposite(Direction firstDirection, Direction secondDirection)
    {
        return (firstDirection == Direction.Up && secondDirection == Direction.Down) ||
               (firstDirection == Direction.Down && secondDirection == Direction.Up) ||
               (firstDirection == Direction.Left && secondDirection == Direction.Right) ||
               (firstDirection == Direction.Right && secondDirection == Direction.Left);
    }

    private void ResetGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                _grid[x, y] = Element.None;
            }
        }
    }

    private void VisualizeGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject prefab = _grid[x, y] switch
                {
                    Element.Corridor => corridorPrefab,
                    Element.Joint => jointPrefab,
                    _ => tilePrefab
                };
                Instantiate(prefab, position, Quaternion.identity, gridParent);
            }
        }
    }
}
