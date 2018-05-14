using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightTile : MonoBehaviour {

    Color startColor;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnMouseEnter()
    {
        startColor = GetComponent<Renderer>().material.color;
        GetComponent<Renderer>().material.color = Color.green;
    }

    private void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = startColor;
    }
}
