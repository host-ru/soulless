using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour {

    Unit selectedUnit;

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
            GameObject hitObject = hitInfo.collider.transform.parent.gameObject; 
            
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
        MeshRenderer mr = hitObject.GetComponentInChildren<MeshRenderer>();

        if (Input.GetMouseButtonDown(0))
        {
            // We clicked at a tile, do something...
            mr.material.color = Color.red;
            if (hitObject.GetComponent<Tile>().associatedUnit != null)
            {
                selectedUnit = hitObject.GetComponent<Tile>().associatedUnit;
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (selectedUnit != null)
            {
                selectedUnit.destinationTile.associatedUnit = null;
                selectedUnit.destinationTile = hitObject.GetComponent<Tile>();
                hitObject.GetComponent<Tile>().associatedUnit = selectedUnit;
            }
        }
    }
}
