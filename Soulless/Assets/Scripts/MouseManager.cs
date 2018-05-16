using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour {

    public Map map;
    public GameObject linePrefab;
    public GameObject highlightClicksPrefab;
    public GameObject highlightMovesPrefab;

    public Unit selectedUnit;
    List<Tile> currentPath = null;
    GameObject hitObject = null;
    bool waitingUnitToStop = false;
    List<GameObject> highlightMoves = new List<GameObject>();

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

        // Highlight all available tiles to move
        if (selectedUnit != null && highlightMoves.Count == 0 && (waitingUnitToStop == false))
        {
            HighlightAvailableTiles();
        }

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
            if (selectedUnit != null && !waitingUnitToStop && hitObject.GetComponent<Tile>().AssociatedUnit == null)
            {
                StartCoroutine("WaitUntilUnitStops");
            }
        }
    }

    IEnumerator WaitUntilUnitStops()
    {
        waitingUnitToStop = true;
        currentPath = map.GeneratePath(selectedUnit.destinationTile, hitObject.GetComponent<Tile>());

        GameObject path = new GameObject();
        path.name = "Path";

        selectedUnit.destinationTile.AssociatedUnit = null;
        hitObject.GetComponent<Tile>().AssociatedUnit = selectedUnit;
        Vector3[] positions = new Vector3[currentPath.Count];
        for (int i = 0; i < currentPath.Count; i++)
        {
            positions[i] = new Vector3(currentPath[i].transform.position.x, 0, currentPath[i].transform.position.z);
        }
        List<GameObject> lines = new List<GameObject>();
        for (int i = 1; i < currentPath.Count; i++)
        {
            GameObject pathLine = linePrefab;
            Vector3 desiredDirection = new Vector3(currentPath[i].transform.position.x, 0, currentPath[i].transform.position.z) - positions[i - 1];
            desiredDirection = desiredDirection.normalized;
            Quaternion q = Quaternion.Euler(90 * desiredDirection.z, 90 * desiredDirection.y, - 90 * desiredDirection.x);
            lines.Add((GameObject)Instantiate(pathLine, positions[i - 1], q, path.transform));
        }

        currentPath.RemoveAt(0);
        for (int i = 0; i < currentPath.Count; i++)
        {

            Destroy(lines[i]);

            selectedUnit.destinationTile = currentPath[i];
            selectedUnit.movesLeft -= map.CostToEnterTile(currentPath[i]);

            while (!selectedUnit.isMoving)
                yield return new WaitForFixedUpdate();
            while (selectedUnit.isMoving)
            {
                ClearHighlightedTiles(); 
                //Vector3 desiredDirection = new Vector3(currentPath[i].transform.position.x, 0, currentPath[i].transform.position.z) - positions[i];
                //lines[i].transform.localScale = new Vector3(0, 0.2f, 0);
                yield return new WaitForFixedUpdate();
            }

        }
        lines.Clear();
        Destroy(path);
        currentPath = null;
        waitingUnitToStop = false;

        // Temporary
        if (selectedUnit.movesLeft < 0)
            selectedUnit.movesLeft = selectedUnit.moves;
    }

    void HighlightAvailableTiles()
    {
        if (highlightMoves.Count > 0)
        {
            ClearHighlightedTiles();
        }
        List<Tile> availableTiles = map.GetAvailableTiles(selectedUnit.destinationTile, selectedUnit.destinationTile, selectedUnit.movesLeft);
        availableTiles.RemoveAt(0);
        for (int i = 0; i < availableTiles.Count; i++)
        {
            highlightMoves.Add((GameObject)Instantiate(highlightMovesPrefab, availableTiles[i].transform.position, Quaternion.identity, GameObject.Find("HighlightMoves").transform));
        }
    }

    void ClearHighlightedTiles()
    {
        for (int j = 0; j < highlightMoves.Count; j++)
        {
            Destroy(highlightMoves[j]);
        }
        highlightMoves.Clear();
    }
}
