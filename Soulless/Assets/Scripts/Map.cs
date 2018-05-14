using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

    public GameObject tilePrefab;

    // Size of the map (tiles)
    public int width = 12;
    public int height = 12;
    public float zOffset = -0.5f;

	// Use this for initialization
	void Start () {

        for (int j = 0; j < height; j++)
        {
            // Setting up rows (don't really know why I need them)
            GameObject rowGameObject = new GameObject();
            rowGameObject.name = "Row_" + j;
            rowGameObject.transform.parent = this.transform;
            rowGameObject.transform.position = new Vector3(0, 0, j);

            for (int i = 0; i < width; i++)
            {
                // Instantiating tiles from prefab
                Vector3 position = new Vector3(i, zOffset, j);
                GameObject tileGameObject = (GameObject)Instantiate(tilePrefab, position, Quaternion.identity);
                tileGameObject.name = "tile_" + i + "_" + j;
                tileGameObject.GetComponent<Tile>().x = i;
                tileGameObject.GetComponent<Tile>().y = j;
                tileGameObject.transform.parent = this.transform.Find("Row_" + j);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
