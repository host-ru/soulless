using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    // Coordinates of a tile
    public int x;
    public int y;
    public int tileType;

    public Unit associatedUnit = null;
}
