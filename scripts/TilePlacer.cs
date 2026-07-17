using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class TilePlacer : MonoBehaviour
{
    private GameControls controls;
    private bool isPlaced = false;
    private Camera mainCamera;
    private TileData tileData;
    public static bool isGameEnd = false;
    private GameManager gameManager => GameManager.instance;
    private ChurchChecker churchChecker => ChurchChecker.instance;

    private static readonly Dictionary<Direction, Vector3> DirectionOffsets = new()
    {
        { Direction.North, new Vector3(0, 0, -1) },
        { Direction.East,  new Vector3(-1, 0, 0) },
        { Direction.South, new Vector3(0, 0, 1) },
        { Direction.West,  new Vector3(1, 0, 0) }
    };

    void Awake()
    {
        controls = new GameControls();
        mainCamera = Camera.main;
        tileData = GetComponent<TileData>();
    }

    void OnEnable()
    {
        controls.TilePlacement.Enable();
    }

    void OnDisable()
    {
        controls.TilePlacement.Disable();
    }

    void Update()
    {
        if (controls.TilePlacement.DiscardTile.WasPressedThisFrame()) //Discards current tile
        {
            DiscardTile();
        }

        //Player input behavior
        if (!isPlaced)
        {
            FollowMouse();
            if (controls.TilePlacement.Click.WasPressedThisFrame()) //Tile placement, also executes tile related scripts
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    Debug.Log("Clicked on UI, ignoring game world interaction.");
                    return;
                }

                if (TryPlaceTile(tileData)) //if tile was placed check for rest of game logic
                {

                    if (IsChurch(tileData)) //checks if selected tile is a church and checks if its surrounded after being placed or it surrounds another church
                    {
                        churchChecker.AddChurch(new ChurchFeature(transform.position));
                        ChurchChecker.IsChurchEncircled(transform.position, GridManager.Instance.gridSize);
                    }
                    else //checks if placed tile surrounds Church
                    {
                        if (churchChecker.activeChurches != null)
                        {
                            churchChecker.CheckActiveChurches(GridManager.Instance.gridSize);
                        }
                        
                    }
                    if (tileData.isSplitTile)
                    {
                        //Debug.Log("Handling split city tile");
                        CityManager.HandleSplitCity(transform.position, GridManager.Instance.gridSize);
                    }
                    else if (TileData.IsCityTile(tileData))
                    {
                        var city = CityManager.TraverseAndMergeCity(transform.position, GridManager.Instance.gridSize);
                        if (city.isComplete)
                        {
                            //Debug.Log($"Completed city! Size: {city.citySize}");

                        }
                    }
                    if (TileData.IsRoadTile(tileData))
                    {
                        var roadEdges = tileData.edges.FindAll(e => e.type == EdgeType.Road);
                        if (roadEdges.Count <= 2)
                        {
                            var road = RoadManager.TraverseRoad(transform.position, GridManager.Instance.gridSize);
                        }
                        else
                        {
                            RoadManager.HandleJunction(transform.position, GridManager.Instance.gridSize);
                        }

                    }
                }

            }
            int rotationdirection = 0; //rotation dissabled
            if (controls.TilePlacement.RotateRight.WasPressedThisFrame()) //sets clockwise rotation
            {
                rotationdirection = 1;
            }
            else if (controls.TilePlacement.RotateLeft.WasPressedThisFrame()) //sets counterclockwise rotation
            {
                rotationdirection = -1;   
            }

            if (rotationdirection != 0) //executes rotation if it was iniciated
            {
                transform.Rotate(0, 90 * rotationdirection, 0);
                tileData.RotateTileEdges(rotationdirection);
            }
            
        }
    }

    void FollowMouse()
    {
        Vector2 mousePosition = controls.TilePlacement.Point.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Vector3 snappedPosition = GridManager.Instance.GetSnappedPosition(hitInfo.point);
            transform.position = snappedPosition;
        }
    }

    //tries to place tile on non-ocupied position and according to rules
    private bool TryPlaceTile(TileData tileData) 
    {
        Vector3 snappedPos = transform.position;
        
        bool isValidPlacement = false; // becomes true if all adjacent match
        if (tileData.isStartingTile) //starting tile skips the checking validation
        {
            GridManager.Instance.OccupyPosition(snappedPos, this.gameObject);
            isPlaced = true;

            PlacementQueueManager.Instance.NotifyTilePlaced(this);
            Debug.Log("Starting tile placed at " + snappedPos);
            return isValidPlacement;
        }

        if (GridManager.Instance.IsPositionOccupied(snappedPos)) // Check if spot is empty
        {
            Debug.Log("Position already occupied!");
            SimpleMessageUI.instance.Show($"Position already occupied!");
            return isValidPlacement;
        }

        foreach (var edge in tileData.edges)
        {
            Vector3 offset = DirectionOffsets[edge.direction] * GridManager.Instance.gridSize;
            Vector3 neighborPos = snappedPos + offset;

            GameObject neighborObj = GridManager.Instance.GetTileAtPosition(neighborPos);
            if (neighborObj == null) continue; 

            TileData neighborData = neighborObj.GetComponent<TileData>();
            if (neighborData == null) continue;

            // Find the edge on the neighbor that faces this tile
            Direction oppositeDir = GetOppositeDirection(edge.direction);
            Edge neighborEdge = neighborData.edges.Find(e => e.direction == oppositeDir);

            if (neighborEdge == null)
            {
                Debug.Log($"Invalid: Neighbor at {neighborPos} has no edge facing this tile.");
                continue;
            }

            if (edge.type != neighborEdge.type)
            {
                Debug.Log($"Invalid: Edge type mismatch at {snappedPos} and neighbor at {neighborPos}: {edge.type} vs {neighborEdge.type}");
                SimpleMessageUI.instance.Show($"adjacing edges do not match!");
                isValidPlacement = false;
                return isValidPlacement; // illegal placement
            }

            isValidPlacement = true; // found all valid matches
        }

        if (!isValidPlacement)
        {
            Debug.Log("Invalid: No matching adjacent edges.");
            SimpleMessageUI.instance.Show($"Tile must be placed next to another tile!");
            return isValidPlacement;
        }

        // Passed all checks — place the tile
        GridManager.Instance.OccupyPosition(snappedPos, gameObject);
        isPlaced = true;
        gameManager.SetCurrentTile(tileData, snappedPos);

        return isValidPlacement;
    }

    public void DiscardTile()
    {
        if (tileData.isStartingTile || isPlaced)
        {
            Debug.Log("Cannot discard starting tile");
            return;
        }
        Destroy(gameObject);
        PlacementQueueManager.Instance.NotifyTilePlaced(this);
    }

    private bool IsChurch(TileData tileData)
    {
        if (tileData != null && tileData.isChurch)
        {
            return true;
        }
        return false;
    }

    private static Direction GetOppositeDirection(Direction dir)
    {
        return (Direction)(((int)dir + 2) % 4);
    }
    public bool getIsPlaced()
    {
        return isPlaced;
    }
}
