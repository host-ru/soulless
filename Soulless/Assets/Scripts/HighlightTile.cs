using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightTile : MonoBehaviour {

    Map map;
    Color startColor;
    List<GameObject> lines = new List<GameObject>();

	// Use this for initialization
	void Start () {

         map = transform.GetComponentInParent<Map>();

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnMouseEnter()
    {
        startColor = GetComponent<Renderer>().material.color;
        GetComponent<Renderer>().material.color = Color.green;

        Tile thisTile = transform.parent.GetComponent<Tile>();
        if (thisTile.isAvailableToMoveOn)
        {
            Unit selectedUnit = GameObject.Find("MouseManager").GetComponent<MouseManager>().selectedUnit;

            List<Tile> desiredPath = map.GeneratePath(selectedUnit.DestinationTile, thisTile);
            lines = map.ShowPath(desiredPath);
        }
    }

    private void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = startColor;
        if (lines.Count != 0)
        {
            if (lines[0] == null)
                lines.Clear();
            else
                Destroy(lines[0].transform.parent.gameObject);
        }
        lines.Clear();
    }
}
