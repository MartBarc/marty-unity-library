using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    public static GridObject Instance { private set; get; }
    private bool DEBUG = false;

    private Grid<GridNode> grid;
    public Grid<GridNode> GetGrid() { return this.grid; }

    private GridTileCollection tileCollection;

    public List<Transform> snapPoints;
    public List<GridTile> draggableObjects;
    private float snapRange = 10f;

    private bool init = false;

    // ------ GRID SETTINGS ------
    //Ex. 475 = (480, 480):(middle of a 96x96)
    //          so (-5, -5):(middle tile size 10fx10f)
    private int gridX;
    private int gridY;
    private Vector3 gridOffset;

    private float cellSize;
    private Vector3 cellOffset;

    private void Awake()
    {
        Instance = this;
    }

    // ------ PUBLIC FUNCTIONS ------
    public int Init(int x, int y, float cellSize, GridTileCollection tileCollection)
    {
        snapPoints = new List<Transform>();
        draggableObjects = new List<GridTile>();

        this.gridX = x;
        this.gridY = y;
        this.cellSize = cellSize;

        this.gridOffset = new Vector3(x * cellSize / 2.0f, y * cellSize / 2.0f);
        this.cellOffset = new Vector3(cellSize / 2.0f, cellSize / 2.0f);

        this.transform.position -= gridOffset; //move to offset location
        this.grid = new Grid<GridNode>(x, y, cellSize, this.transform.position, (Grid<GridNode> grid, int x, int y) => new GridNode(grid, x, y));

        this.tileCollection = tileCollection;
        if (this.tileCollection == null) { return -1; }

        if (DEBUG) grid.DrawDebugLines(Color.cyan);

        FillNullTiles();

        init = true;

        return 0;
    }

    public int AddTile(int id, int x, int y, out GridTile tile)
    {
        tile = null;
        if (id < 0) return -1;

        GridNode gridNodeObj = grid.GetGridObject(x, y);
        if (gridNodeObj == null) return -1;

        if (gridNodeObj.GetGameObject() != null)
        {
            if (DEBUG) Debug.Log("NOTE: Tile[" + x + ", " + y + "] exists! Deleting!");
            this.snapPoints.Remove(gridNodeObj.GetGameObject().transform);
            Destroy(gridNodeObj.GetGameObject());
        }

        Vector3 gridRealLocation = GetCellCenter(x, y);
        GameObject newGameObject = tileCollection.CreateTilePrefabFromID(id, gridRealLocation);
        if (newGameObject != null)
        {
            gridNodeObj.SetGameObject(newGameObject);
            gridNodeObj.SetTile(gridNodeObj.GetGameObject().GetComponent<GridTile>());
            //newGameObject.transform.parent = this.transform;

            tile = gridNodeObj.GetTile();
            if (tile.type == TILE_TYPE.EMPTY)
            {
                tile.SetObjectActive(false);
            }

            this.snapPoints.Add(tile.transform);

            //tile.transform.parent = gridNodeObj;
            if (DEBUG) Debug.Log("NOTE: ShipObject[" + x + ", " + y + "] created at: " + gridNodeObj.GetGameObject().transform.position.ToString());
        }

        if (newGameObject == null || tile == null)
        {
            if (DEBUG) Debug.Log("ERR: ShipObject[" + x + ", " + y + "] failed to be created at: " + gridNodeObj.GetGameObject().transform.position.ToString());
            return -1;
        }

        return 0;
    }

    public int AddTile(string tileName, int x, int y, out GridTile tile)
    {
        tile = null;
        GridNode gridNode = grid.GetGridObject(x, y);
        if (gridNode == null) return -1;

        if (gridNode.GetGameObject() != null)
        {
            Destroy(gridNode.GetGameObject());
        }

        Vector3 gridRealLocation = GetCellCenter(x, y);
        GameObject newGameObject = tileCollection.CreateTilePrefabFromName(tileName, gridRealLocation);
        if (newGameObject != null)
        {
            gridNode.SetGameObject(newGameObject);
            gridNode.SetTile(gridNode.GetGameObject().GetComponent<GridTile>());

            if (DEBUG) Debug.Log("NOTE: Tile[" + gridRealLocation.ToString() + "] created at: " + gridNode.GetTile().transform.position.ToString());

            newGameObject.transform.parent = this.transform;
        }

        return 0;
    }

    public void GetMapSettings(out int x, out int y, out float size)
    {
        x = this.gridX;
        y = this.gridY;
        size = this.cellSize;
    }

    public Vector3 GetCellCenter(int x, int y)
    {
        float x_axis = (x * grid.GetCellSize() - cellOffset.x) + this.transform.position.x;
        float y_axis = (y * grid.GetCellSize() - cellOffset.y) + this.transform.position.y;
        return new Vector3(x_axis, y_axis);
    }

    public int AddDraggableObject(GridTile tile)
    {
        if (tile == null) return -1;

        draggableObjects.Add(tile);
        tile.dragEndedCallback = OnDragEnded;
        //foreach (ShipObject draggable in draggableObjects)
        //{
        //    draggable.dragEndedCallback = OnDragEnded;
        //}

        return 0;
    }

    //PRIVATE

    private int FillNullTiles()
    {
        int NULL_TILE_ID = 100;
        for (int x = 0; x < this.gridX; x++)
        {
            for (int y = 0; y < this.gridY; y++)
            {
                GridNode gridNodeObj = grid.GetGridObject(x, y);
                if (gridNodeObj == null) return -1;

                if (gridNodeObj.GetGameObject() == null)
                {
                    if (DEBUG) Debug.Log("NOTE: GridTile[" + x + ", " + y + "] is NULL! Filling in!");
                    AddTile(NULL_TILE_ID, x, y, out GridTile emptyShipObj);
                }
            }
        }
        return 0;
    }

    private void OnDragEnded(GridTile tile) //Draggable draggable)
    {
        float closestDistance = -1;
        Transform closestSnapPoint = null;

        foreach (Transform snapPoint in snapPoints)
        {
            float currentDistance = Vector2.Distance(tile.transform.localPosition, snapPoint.localPosition);

            if (closestSnapPoint == null || currentDistance < closestDistance)
            {
                closestSnapPoint = snapPoint;
                closestDistance = currentDistance;
            }
        }
        if (closestSnapPoint != null && closestDistance <= snapRange)
        {
            tile.transform.localPosition = closestSnapPoint.localPosition;

            //
            GridNode gridNodeObj = grid.GetGridObject(tile.transform.localPosition);
            if (gridNodeObj.GetGameObject() != null)
            {
                this.snapPoints.Remove(gridNodeObj.GetGameObject().transform);
                Destroy(gridNodeObj.GetGameObject());
            }
            gridNodeObj.SetTile(tile);
            this.snapPoints.Add(tile.transform);
            FillNullTiles();
        }
        else
        {
            Destroy(tile);
        }
    }
}
