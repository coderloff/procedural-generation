using UnityEngine;

enum PathDirection
{
    Horizontal,
    Vertical
}

enum Element
{
    None,
    Corridor,
    Rotation
}

public class PathGenerator : MonoBehaviour
{
    [SerializeField] private int horizontal;
    [SerializeField] private int vertical;

    [SerializeField] private Transform pathParent;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject corridorPrefab;
    [SerializeField] private GameObject rotationPrefab;

    private Element[,] _grid;
    private int _verticalCorridorCount;
    private int _horizontalCorridorCount;
    private PathDirection _pathDirection;

    private void Start()
    {
        _grid = new Element[vertical, horizontal];
        ResetPath();
        CalculatePath();
    }

    private void CalculatePath()
    {
        if (horizontal < 3 && vertical < 3)
        {
            Debug.LogError("Invalid dungeon dimensions.");
            return;
        }

        if(horizontal < 3)
        {
            _verticalCorridorCount = (vertical - 1) / 2;
            _horizontalCorridorCount = (horizontal - 2) / 2;
            _pathDirection = PathDirection.Vertical;
        }
        else
        {
            _verticalCorridorCount = (vertical - 2) / 2;
            _horizontalCorridorCount = (horizontal - 1) / 2;
            _pathDirection = PathDirection.Horizontal;
        }

        Debug.Log($"Horizontal Corridor Count: {_horizontalCorridorCount}");
        Debug.Log($"Vertical Corridor Count: {_verticalCorridorCount}");

        GeneratePath();
    }

    private void GeneratePath()
    {
        if (_pathDirection.Equals(PathDirection.Vertical))
        {
            // Place vertical corridors
            for (int i = 0; i < _verticalCorridorCount; i++)
            {
                _grid[2 * i, 0] = _grid[2 * i + 1, 0] = Element.Corridor; // First row
                if (horizontal >= 2)
                {
                    _grid[2 * i, _horizontalCorridorCount * 2 + 1] = _grid[2 * i + 1, _horizontalCorridorCount * 2 + 1] = Element.Corridor; // Last row
                }
            }

            //Debug.Log($"{_grid[0, 2]}");
            // Place rotation joints
            _grid[_verticalCorridorCount * 2, 0] = Element.Rotation;
            if (horizontal >= 2)
            {
                _grid[_verticalCorridorCount * 2, _horizontalCorridorCount * 2 + 1] = Element.Rotation;
            }

            // Place horizontal corridors
            for (int i = 0; i < _horizontalCorridorCount; i++)
            {
                _grid[_verticalCorridorCount * 2, 2 * i + 1] = _grid[_verticalCorridorCount * 2, 2 * i + 2] = Element.Corridor;
            }
        }
        else
        {
            // Place horizontal corridors
            for (int i = 0; i < _horizontalCorridorCount; i++)
            {
                _grid[0, 2 * i] = _grid[0, 2 * i + 1] = Element.Corridor; // First column
                if(vertical >= 2)
                {
                    _grid[_verticalCorridorCount * 2 + 1, 2 * i] = _grid[_verticalCorridorCount * 2 + 1, 2 * i + 1] = Element.Corridor; // Last column
                }
            }

            // Place rotation joints
            _grid[0, _horizontalCorridorCount * 2] = Element.Rotation;
            if (vertical >= 2)
            {
                _grid[_verticalCorridorCount * 2 + 1, _horizontalCorridorCount * 2] = Element.Rotation;
            }

            // Place vertical corridors
            for (int i = 0; i < _verticalCorridorCount; i++)
            {
                _grid[2 * i + 1, _horizontalCorridorCount * 2] = _grid[2 * i + 2, _horizontalCorridorCount * 2] = Element.Corridor;
            }
        }

        VisualizePath();
    }

    private void VisualizePath()
    {
        for (int i = 0; i < vertical; i++)
        {
            string s = "";
            for (int j = 0; j < horizontal; j++)
            {
                switch (_grid[i, j])
                {
                    case Element.Corridor:
                        s += "=";
                        Instantiate(corridorPrefab, new Vector3(j, i, 0), Quaternion.identity, pathParent);
                        break;
                    case Element.Rotation:
                        s += "*";
                        Instantiate(rotationPrefab, new Vector3(j, i, 0), Quaternion.identity, pathParent);
                        break;
                    default:
                        s += "o";
                        Instantiate(tilePrefab, new Vector3(j, i, 0), Quaternion.identity, pathParent);
                        break;
                }
            }
            Debug.Log(s);
        }
    }

    private void ResetPath()
    {
        for (int i = 0; i < vertical; i++)
        {
            for (int j = 0; j < horizontal; j++)
            {
                _grid[i, j] = Element.None;
            }
        }
    }
}
