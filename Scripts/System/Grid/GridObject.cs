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

    public List<Vector3> snapPoints;
    public List<GridTile> draggableObjects;
    //sensitivity of snap (100f is further, 1f is closer)
    private float snapRange = 1f; 

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
        snapPoints = new List<Vector3>();
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
        if (id < -1) return -1; //null tile is -1

        GridNode gridNode = grid.GetGridObject(x, y);
        if (gridNode == null) return -1;

        if (gridNode.GetGameObject() != null)
        {
            if (DEBUG) Debug.Log("NOTE: Tile[" + x + ", " + y + "] exists! Deleting!");
            this.snapPoints.Remove(gridNode.GetGameObject().transform.position);
            Destroy(gridNode.GetGameObject());
        }

        Vector3 gridRealLocation = GetCellCenter(x, y);
        GameObject newGameObject = tileCollection.CreateTilePrefabFromID(id, gridRealLocation);
        if (newGameObject != null)
        {
            gridNode.SetGameObject(newGameObject);
            gridNode.SetTile(gridNode.GetGameObject().GetComponent<GridTile>());
            //newGameObject.transform.parent = this.transform;

            tile = gridNode.GetTile();
            if (tile.type == TILE_TYPE.EMPTY)
            {
                tile.SetObjectActive(false);
            }

            this.snapPoints.Add(new Vector3(x + cellOffset.x, y + cellOffset.y));// tile.transform.position);

            //tile.transform.parent = gridNodeObj;
            if (DEBUG) Debug.Log("NOTE: GridObject[" + x + ", " + y + "] created at: " + gridNode.GetGameObject().transform.position.ToString());
        }

        if (newGameObject == null || tile == null)
        {
            if (DEBUG) Debug.Log("ERR: GridObject[" + x + ", " + y + "] failed to be created");
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
        return new Vector3(x_axis + 1.0f , y_axis + 1.0f); //may have to remove 1.0f
    }

    public int AddDraggableObject(GridTile tile)
    {
        if (tile == null) return -1;

        draggableObjects.Add(tile);
        tile.dragEndedCallback = OnDragEnded;
        //foreach (GridObject draggable in draggableObjects)
        //{
        //    draggable.dragEndedCallback = OnDragEnded;
        //}

        return 0;
    }

    //PRIVATE

    private int FillNullTiles()
    {
        int NULL_TILE_ID = -1;
        for (int x = 0; x < this.gridX; x++)
        {
            for (int y = 0; y < this.gridY; y++)
            {
                GridNode gridNodeObj = grid.GetGridObject(x, y);
                if (gridNodeObj == null) return -1;

                if (gridNodeObj.GetGameObject() == null)
                {
                    if (DEBUG) Debug.Log("NOTE: GridTile[" + x + ", " + y + "] is NULL! Filling in!");
                    AddTile(NULL_TILE_ID, x, y, out GridTile emptyGridTile);
                    emptyGridTile.transform.parent = this.transform;
                    AddDraggableObject(emptyGridTile);
                }
            }
        }
        return 0;
    }

    private void OnDragEnded(GridTile tile) //Draggable draggable)
    {
        float closestDistance = 10000f;
        Vector3 closestSnapPoint = tile.transform.localPosition;

        foreach (Vector3 snapPoint in snapPoints) //search through all possible snap points
        {
            //float currentDistance = Vector2.Distance(tile.transform.localPosition, snapPoint.localPosition);
            float currentDistance = Vector3.Distance(tile.transform.localPosition, snapPoint);
            if (currentDistance < closestDistance)
            {
                closestSnapPoint = snapPoint;
                closestDistance = currentDistance;
            }
        }
        if (closestSnapPoint != null && closestDistance <= snapRange)
        {
            Debug.Log(closestDistance);
            Debug.Log(closestSnapPoint);
            tile.transform.localPosition = closestSnapPoint;

            //// removes the current object
            //GridNode gridNode = grid.GetGridObject(tile.transform.localPosition);
            //if (gridNode.GetGameObject() != null)
            //{
            //    this.snapPoints.Remove(gridNode.GetGameObject().transform);
            //    Destroy(gridNode.GetGameObject());
            //}
            //gridNode.SetTile(tile);
            //this.snapPoints.Add(tile.transform);
            //FillNullTiles();
        }
        //else
        //{
        //    Destroy(tile);
        //}
    }
}
