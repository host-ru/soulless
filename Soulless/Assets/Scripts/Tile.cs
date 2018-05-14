using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    // Coordinates of a tile
    public int x;
    public int y;

    public Tile[] GetNeighbors()
    {
        // Find all the neighbors
        GameObject leftNeighbor = GameObject.Find("Tile_" + (x - 1) + "_" + y);
        GameObject rightNeighbor = GameObject.Find("Tile_" + (x + 1) + "_" + y);
        GameObject upNeighbor = GameObject.Find("Tile_" + x + "_" + (y + 1));
        GameObject downNeighbor = GameObject.Find("Tile_" + x + "_" + (y - 1));

        Tile[] neighbors = { leftNeighbor.GetComponent<Tile>(), rightNeighbor.GetComponent<Tile>(), upNeighbor.GetComponent<Tile>(), downNeighbor.GetComponent<Tile>() };

        return neighbors;
    }
	
}
