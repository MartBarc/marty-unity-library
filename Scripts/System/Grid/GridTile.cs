using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TILE_TYPE
{
    CREW,
    WEAPON_TURRET,
    EMPTY,
    UNKNOWN
}

public class GridTile : MonoBehaviour
{
    private bool DEBUG = false;
    private int uniqueId;

    //GridTileObject Properties
    [SerializeField] public int id; //ALL OBJ START AT 200
    [SerializeField] public string tileName;
    public string GetTileName() { return this.tileName; }

    [SerializeField] public bool impassable;
    [SerializeField] public TILE_TYPE type;

    public delegate void DragEndedDelegate(GridTile dragObject);
    public DragEndedDelegate dragEndedCallback;

    private bool enableDragging = true;
    private bool isDragged = false;
    private Vector3 mouseDragStartPosition;
    private Vector3 spriteDragStartPosition;

    public float health;

    private SpriteRenderer sprite;
    private Color curColor;
    private float redColor;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        curColor = sprite.color;

        redColor = sprite.color.r;
    }

    public GridTile()
    {
        uniqueId = -1;

        this.id = -1;
        this.tileName = "name";

        this.impassable = false;
        this.type = TILE_TYPE.UNKNOWN;

        health = 100f;
    }

    public int GetUniqueID()
    {
        return uniqueId;
    }

    public void SetUniqueID(int uniqueId)
    {
        this.uniqueId = uniqueId;
    }

    public bool IsImpassable()
    {
        return impassable;
    }

    public List<string> GetTilePropertiesString()
    {
        List<string> unitProperties = new List<string>();

        unitProperties.Add("ID: " + this.GetUniqueID().ToString());
        unitProperties.Add("Name: " + this.GetTileName());
        unitProperties.Add("Pos: " + this.transform.position);

        return unitProperties;
    }

    public void DebugTileProperties()
    {
        string log = "";
        List<string> tileString = this.GetTilePropertiesString();
        foreach (string strProp in tileString)
        {
            log += strProp + " | ";
        }
        Debug.Log("NOTE: " + log);
    }

    public void deltDamage(float damage)
    {
        health -= damage;

        redColor += damage / health;
        sprite.color = new Color(redColor, sprite.color.g, sprite.color.b);

        if (DEBUG) Debug.Log(".: Tile[" + tileName + "] delt [" + damage + "] damage");

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetObjectActive(bool active)
    {
        //this.gameObject.SetActive(false);
    }

    // PRIVATE

    private void OnMouseDown()
    {
        if (enableDragging)
        {
            isDragged = true;
            mouseDragStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spriteDragStartPosition = transform.localPosition;
        }
    }

    private void OnMouseDrag()
    {
        if (enableDragging && isDragged)
        {
            transform.localPosition = spriteDragStartPosition + (Camera.main.ScreenToWorldPoint(Input.mousePosition) - mouseDragStartPosition);
        }
    }

    private void OnMouseUp()
    {
        if (enableDragging)
        {
            isDragged = false;
            dragEndedCallback(this);
        }
    }
}
