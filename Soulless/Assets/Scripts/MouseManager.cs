using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour {

    public Map map;
    public GameObject linePrefab;
    public GameObject highlightMovesPrefab;
    public GameObject highlightUnitPrefab;

    public Unit selectedUnit;
    bool waitingUnitToStop = false;
    List<GameObject> highlightMoves = new List<GameObject>();
    List<Tile> availableTiles = new List<Tile>();
    GameObject highlightUnit = null;

    // For mousing over, reduces number of draws
    Tile prevTile = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        // Check if we hit UI element
        //if (EventSystem.current.IsPointerOverGameObject())
        //{
        //    return;
        //}

        // Getting info about what object we're looking at
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layer_mask = LayerMask.GetMask("Map"); // We're only hitting tiles here
        float maxDistance = 1000f;
        RaycastHit hitInfo;


        if (Physics.Raycast(ray, out hitInfo, maxDistance, layer_mask))
        {
            // Identify the object we hit
            GameObject hitObject = hitInfo.collider.transform.parent.gameObject;

            // Check what kind of object this is
            if (hitObject.GetComponent<Map>() != null)
            {
                // We are over a mesh
                Vector3 meshBorder = map.GetMeshBorder();
                Vector3 meshWorldSize = map.GetMeshWorldSize();
                Vector3 hitCoord = hitInfo.transform.InverseTransformPoint(hitInfo.point + meshWorldSize / 2);

                // Check if we're hitting an actual tile
                if (hitCoord.x >= 0 && hitCoord.x <= meshWorldSize.x - meshBorder.x * 2 && hitCoord.z >= 0 && hitCoord.z <= meshWorldSize.z - meshBorder.z * 2)
                {
                    Tile hitTile = map.GetTileByCoord(hitCoord);
                    MouseOverTile(hitTile, prevTile);
                }
            }
        }
    }

    void MouseOverTile(Tile hitTile)
    {
        MouseOverTile(hitTile, null);
    }

    void MouseOverTile(Tile hitTile, Tile prevTile)
    {
        
            
        if (Input.GetMouseButtonDown(0))
        {
            // We clicked at a tile, do something...

            if (selectedUnit == null || !selectedUnit.isMoving)
            {
                selectedUnit = hitTile.AssociatedUnit;
                if (selectedUnit != null)
                {
                    if (highlightUnit != null)
                        Destroy(highlightUnit);
                    highlightUnit = (GameObject)Instantiate(highlightUnitPrefab, map.GetTileWorldPosition(selectedUnit.DestinationTile), Quaternion.identity, selectedUnit.transform);
                    HighlightAvailableTiles();
                }
                else
                {
                    if (highlightUnit != null)
                        Destroy(highlightUnit);
                    ClearHighlightedTiles();
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (selectedUnit != null && !waitingUnitToStop && hitTile.AssociatedUnit == null && hitTile.isAvailableToMoveOn)
            {
                StartCoroutine("MoveUnitTo", hitTile);
            }
        }
        else if (hitTile.isOccupied)
        {
            GameObject oldPath = GameObject.Find("Path");
            Destroy(oldPath);
        }
        else if (selectedUnit != null && !selectedUnit.isMoving && hitTile.isAvailableToMoveOn)
        {
            GameObject oldPath = GameObject.Find("Path");
            Destroy(oldPath);

            if (prevTile == null || hitTile != prevTile)
            {
                List<Tile> currentPath = map.GeneratePath(selectedUnit.DestinationTile, hitTile);
                List<GameObject> lines = map.ShowPath(currentPath);
                prevTile = hitTile;
            }
        }
    }

    IEnumerator MoveUnitTo(Tile tileToMoveOn)
    {
        waitingUnitToStop = true;
        GameObject oldPath = GameObject.Find("Path");
        
        Destroy(oldPath);
        List<Tile> currentPath = map.GeneratePath(selectedUnit.DestinationTile, tileToMoveOn);
        List<GameObject> lines = map.ShowPath(currentPath);

        currentPath.RemoveAt(0);
        for (int i = 0; i < currentPath.Count; i++)
        {

            Destroy(lines[i]);

            selectedUnit.movesLeft -= map.CostToEnterTile(currentPath[i]);
            selectedUnit.DestinationTile = currentPath[i];

            ClearHighlightedTiles(); 

            while (!selectedUnit.isMoving)
                yield return new WaitForFixedUpdate();
            while (selectedUnit.isMoving)
            {
                //Vector3 desiredDirection = new Vector3(currentPath[i].transform.position.x, 0, currentPath[i].transform.position.z) - positions[i];
                //lines[i].transform.localScale = new Vector3(0, 0.2f, 0);
                yield return new WaitForFixedUpdate();
            }

        }
        lines.Clear();
        Destroy(GameObject.Find("Path"));
        currentPath = null;
        waitingUnitToStop = false;


        // Temporary
        if (selectedUnit.movesLeft == 0)
            selectedUnit.movesLeft = selectedUnit.moves;


        HighlightAvailableTiles();
    }

    void HighlightAvailableTiles()
    {
        if (highlightMoves.Count > 0)
        {
            ClearHighlightedTiles();
        }
        availableTiles = map.GetAvailableTiles(selectedUnit.DestinationTile, selectedUnit.DestinationTile, selectedUnit.movesLeft);
        availableTiles.RemoveAt(0);
        for (int i = 0; i < availableTiles.Count; i++)
        {
            GameObject newMove = (GameObject)Instantiate(highlightMovesPrefab, map.GetTileWorldPosition(availableTiles[i]), Quaternion.identity, GameObject.Find("HighlightMoves").transform);
            newMove.transform.localScale = map.GetTileWorldSize() * 0.9f + Vector3.up;
            highlightMoves.Add(newMove);
            availableTiles[i].isAvailableToMoveOn = true;
        }
    }

    void ClearHighlightedTiles()
    {
        for (int j = 0; j < highlightMoves.Count; j++)
        {
            Destroy(highlightMoves[j]);
            availableTiles[j].isAvailableToMoveOn = false;
        }
        highlightMoves.Clear();
        availableTiles.Clear();
    }
}
