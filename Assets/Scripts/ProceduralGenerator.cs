using UnityEngine;

enum CorridorDirection
{
    Left,
    Right,
    Up,
    Down,
}

enum RotationDirection
{
    Left,
    Right
}

enum Element
{
    None,
    Corridor,
    Rotation
}

public class ProceduralGenerator : MonoBehaviour
{
    [SerializeField] private int horizontal;
    [SerializeField] private int vertical;

    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject corridorPrefab;
    [SerializeField] private GameObject rotationPrefab;

    private Element[,] _grid;
    private int _x = 0;
    private int _y = 0;
    private bool _isFinished = false;
    private Element _element;

    private CorridorDirection _corridorDirection;
    private RotationDirection _rotationDirection;

    public static T GetRandomEnumValue<T>(T[] enumValues) where T : System.Enum
    {
        int randomIndex = Random.Range(0, enumValues.Length);
        return enumValues[randomIndex];
    }


    private void Start()
    {
        _grid = new Element[vertical, horizontal];
        ResetGrid();
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (horizontal < 3)
        {
            _corridorDirection = CorridorDirection.Down;
            _grid[_y, _x] = _grid[_y + 1, _x] = Element.Corridor;
            _y += 2;
        }
        else
        {
            _corridorDirection = CorridorDirection.Right;
            _grid[_y, _x] = _grid[_y, _x + 1] = Element.Corridor;
            _x += 2;
        }

        CheckFinish();

        while (!_isFinished)
        {
            _element = GetRandomEnumValue(new[] { Element.Corridor, Element.Rotation });
            if (_element == Element.Corridor)
            {
                if(_corridorDirection.Equals(CorridorDirection.Right))
                {
                    if (_x + 1 <= horizontal - 1 && _grid[_y, _x] == Element.None && _grid[_y, _x + 1] == Element.None)
                    {
                        _grid[_y, _x] = _grid[_y, _x + 1] = Element.Corridor;
                        _x += 2;
                    }
                }
                else if (_corridorDirection.Equals(CorridorDirection.Left))
                {
                    if (_x - 1 >= 0 && _grid[_y, _x] == Element.None && _grid[_y, _x - 1] == Element.None)
                    {
                        _grid[_y, _x] = _grid[_y, _x - 1] = Element.Corridor;
                        _x -= 2;
                    }
                }
                else if (_corridorDirection.Equals(CorridorDirection.Up))
                {
                    if (_y - 1 >= 0 && _grid[_y, _x] == Element.None && _grid[_y - 1, _x] == Element.None)
                    {
                        _grid[_y, _x] = _grid[_y - 1, _x] = Element.Corridor;
                        _y -= 2;
                    }
                }
                else
                {
                    if (_y + 1 <= vertical - 1 && _grid[_y, _x] == Element.None && _grid[_y + 1, _x] == Element.None)
                    {
                        _grid[_y, _x] = _grid[_y + 1, _x] = Element.Corridor;
                        _y += 2;
                    }
                }
            }
            else
            {
                if(_grid[_y, _x] != Element.None)
                {
                    Debug.LogWarning($"Position ({_x}, {_y}) is already occupied. Skipping rotation placement.");
                    ResetGrid();
                    GenerateGrid();
                    continue;
                }
                _grid[_y, _x] = Element.Rotation;
                _rotationDirection = GetRandomEnumValue(new[] { RotationDirection.Left, RotationDirection.Right });
                switch (_rotationDirection)
                {
                    case RotationDirection.Left:
                        _corridorDirection = _corridorDirection switch
                        {
                            CorridorDirection.Right => CorridorDirection.Up,
                            CorridorDirection.Left => CorridorDirection.Down,
                            CorridorDirection.Up => CorridorDirection.Left,
                            CorridorDirection.Down => CorridorDirection.Right,
                            _ => _corridorDirection
                        };
                        break;
                    case RotationDirection.Right:
                        _corridorDirection = _corridorDirection switch
                        {
                            CorridorDirection.Right => CorridorDirection.Down,
                            CorridorDirection.Left => CorridorDirection.Up,
                            CorridorDirection.Up => CorridorDirection.Right,
                            CorridorDirection.Down => CorridorDirection.Left,
                            _ => _corridorDirection
                        };
                        break;
                }
                switch (_corridorDirection)
                {
                    case CorridorDirection.Right:
                        _x++;
                        break;
                    case CorridorDirection.Left:
                        _x--;
                        break;
                    case CorridorDirection.Up:
                        _y--;
                        break;
                    case CorridorDirection.Down:
                        _y++;
                        break;
                }
            }
            //Debug.Log($"Current Position: ({ _x}, {_y})\n Element: + {_element}\n Corridor direction: {_corridorDirection}");

            CheckFinish();
        }

        VisualizeGrid();
    }

    private void CheckFinish()
    {
        if( _x < 0 || _x >= horizontal || _y < 0 || _y >= vertical)
        {
            _isFinished = true;
        }
    }

    private void VisualizeGrid()
    {
        for (int i = 0; i < vertical; i++)
        {
            string s = string.Empty;
            for (int j = 0; j < horizontal; j++)
            {
                if (_grid[i, j] == Element.Corridor)
                {
                    s += "=";
                    Instantiate(corridorPrefab, new Vector3(j, i, 0), Quaternion.identity, gridParent);
                }
                else if (_grid[i, j] == Element.Rotation)
                {
                    s += "*";
                    Instantiate(rotationPrefab, new Vector3(j, i, 0), Quaternion.identity, gridParent);
                }
                else
                {
                    s += "o";
                    Instantiate(tilePrefab, new Vector3(j, i, 0), Quaternion.identity, gridParent);
                }
            }
            Debug.Log(s);
        }
    }

    private void ResetGrid()
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
