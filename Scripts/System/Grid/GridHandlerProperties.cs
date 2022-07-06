using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains the properties of a grid
/// </summary>
public class GridHandlerProperties : MonoBehaviour
{
    // STATIC
    private static float CELLSIZE = 10.0f;
    public float GetCellSize() { return CELLSIZE; }

    private static float DEFAULT_ANGLE = 0.0f;//90.0f;
    public float GetDefaultAngle() { return DEFAULT_ANGLE; }

    //DYNAMIC CORE
    private string NAME;
    public string GetName() { return NAME; }
    public void SetName(string name) { NAME = name; }

    //DYNAMIC REACTIVE (TO DYNAMIC CORE)
    private int GRIDSIZE; // depends on CLASS (default = 96)
    public int GetGridSize() { return GRIDSIZE; }

    //STATS
    private int HEALTH;
    public int GetGridHealth() { return HEALTH; }
    public void SetGridHealth(int health) { HEALTH = health; }

    public GridHandlerProperties()
    {
        NAME = "Odysseus";
    }
}
