using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHandler : MonoBehaviour
{
    public static GridHandler Instance { private set; get; }
    private bool DEBUG = true;
    public bool IsDebug() { return this.DEBUG; }

    // ------ UI OBJECTS ------
    private Camera mainCamera;

    // ------ SETTINGS ------
    private bool init = false;
    public bool IsInit() { return init; }

    // ------ PUBLIC PROPERTIES ------
    [SerializeField] public GridObject gridObject;
    [SerializeField] public GridTileCollection gridTileCollection;

    private GridHandlerProperties PROP;
    public GridHandlerProperties GetGridProperties() { return PROP; }

    private GridObject grid;
    public GridObject GetTileGrid() { return this.grid; }

    // ------ MonoBehavior Functions ------
    private void Awake()
    {
        Instance = this;
    }

    public int Init()
    {
        grid = Instantiate(gridObject, this.transform.position, Quaternion.identity);
        int status = grid.Init(PROP.GetGridSize(), PROP.GetGridSize(), PROP.GetCellSize(), gridTileCollection);
        if (status == 0)
        {
            grid.transform.parent = this.transform;
        }

        return status;
    }
}
