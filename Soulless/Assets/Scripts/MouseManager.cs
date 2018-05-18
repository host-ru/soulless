using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour {

    public Map map;
    public GameObject linePrefab;
    public GameObject highlightMovesPrefab;

    public Unit selectedUnit;
    //List<Tile> currentPath = null;
    GameObject hitObject = null;
    bool waitingUnitToStop = false;
    List<GameObject> highlightMoves = new List<GameObject>();
    List<Tile> availableTiles = new List<Tile>();

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
        int layer_mask = LayerMask.GetMask("Tiles"); // We're only hitting tiles here
        float maxDistance = 1000f;
        RaycastHit hitInfo;


        if (Physics.Raycast(ray, out hitInfo, maxDistance, layer_mask))
        {
            // Identify the object we hit
            hitObject = hitInfo.collider.transform.parent.gameObject;

            // Check what kind of object this is
            if (hitObject.GetComponent<Tile>() != null)
            {
                // We are over a tile
                MouseOverTile(hitObject);
            }
        }
    }

    void MouseOverTile(GameObject hitObject)
    {
        if (Input.GetMouseButtonDown(0))
        {
            // We clicked at a tile, do something...

            if (selectedUnit == null || !selectedUnit.isMoving)
            {
                selectedUnit = hitObject.GetComponent<Tile>().AssociatedUnit;
                if (selectedUnit != null)
                    HighlightAvailableTiles();
                else
                    ClearHighlightedTiles();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (selectedUnit != null && !waitingUnitToStop && hitObject.GetComponent<Tile>().AssociatedUnit == null && hitObject.GetComponent<Tile>().isAvailableToMoveOn)
            {
                StartCoroutine("MoveUnitTo");
            }
        }
    }

    IEnumerator MoveUnitTo()
    {
        waitingUnitToStop = true;
        GameObject oldPath = GameObject.Find("Path");
        
        Destroy(oldPath);
        List<Tile> currentPath = map.GeneratePath(selectedUnit.DestinationTile, hitObject.GetComponent<Tile>());
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
            highlightMoves.Add((GameObject)Instantiate(highlightMovesPrefab, availableTiles[i].transform.position, Quaternion.identity, GameObject.Find("HighlightMoves").transform));
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
