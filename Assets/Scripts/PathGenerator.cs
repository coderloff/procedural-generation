using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGeneration
{
    public class PathGenerator : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private int attempts = 1000;

        [Header("Visualization Objects")]
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject corridorPrefab;
        [SerializeField] private GameObject jointPrefab;

        private GridManager _gridManager;
        private DirectionManager _directionManager;

        private void Start()
        {
            _gridManager = new GridManager();
            _directionManager = new DirectionManager();

            GeneratePath();
        }

        private void GeneratePath()
        {
            // Initialize the grid
            if (!_gridManager.InitializeGrid(gridWidth, gridHeight)) return;

            // Procedural generation loop
            for (int i = 0; i < attempts; i++)
            {
                if (!_gridManager.GenerateGrid()) continue;

                _directionManager.GenerateDirections(_gridManager.StartPoint, _gridManager.EndPoint, gridWidth);

                Vector2Int firstCorridor = _gridManager.StartPoint;
                Vector2Int secondCorridor = _gridManager.StartPoint + _directionManager.StartVector;
                Vector2Int nextPoint = _gridManager.StartPoint + _directionManager.StartVector * 2;

                State state = _gridManager.PlaceTiles(false, _directionManager.StartDirection, _directionManager.EndDirection, _gridManager.StartPoint, firstCorridor, secondCorridor, nextPoint);
                if (state == State.Failed) continue;

                if (GenerateFrom(nextPoint, _directionManager.StartDirection, 0))
                {
                    VisualizePath();
                    Debug.Log("Path generated successfully!");
                    Debug.Log($"Start Point: {_gridManager.StartPoint}, Start Direction: {_directionManager.StartDirection}\n End Point: {_gridManager.EndPoint}, End Direction: {_directionManager.EndDirection}");
                    return;
                }
            }

            // Throw an error if path generation fails after multiple attempts
            Debug.LogWarning("Failed to generate path after multiple attempts.");
        }

        private bool GenerateFrom(Vector2Int currentPosition, Direction previousDirection, int depth)
        {
            // Check if algorithm have been running longer than tile count to prevent loops
            if (depth > gridWidth * gridHeight) return false;

            List<Direction> randomDirections = _directionManager.GetRandomDirections();

            foreach (Direction direction in randomDirections)
            {
                if (_directionManager.IsOpposite(previousDirection, direction)) continue;

                bool needsJoint = direction != previousDirection;

                Vector2Int directionVector = _directionManager.DirectionToVector(direction);

                Vector2Int firstCorridor = needsJoint ? currentPosition + directionVector : currentPosition;
                Vector2Int secondCorridor = needsJoint ? currentPosition + directionVector * 2 : currentPosition + directionVector;
                Vector2Int nextPoint = needsJoint ? currentPosition + directionVector * 3 : currentPosition + directionVector * 2;

                State state = _gridManager.PlaceTiles(needsJoint, direction, _directionManager.EndDirection, currentPosition, firstCorridor, secondCorridor, nextPoint);
                if(state == State.Failed)
                    continue;
                else if (state == State.Finished)
                    return true;

                if (GenerateFrom(nextPoint, direction, depth + 1))
                    return true;

                _gridManager.Backtrack(needsJoint, currentPosition, firstCorridor, secondCorridor);                
            }

            return false;
        }

        private void VisualizePath()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 position = new Vector3(x, y, 0);
                    GameObject prefab = _gridManager.Grid[x,y] switch
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
}
