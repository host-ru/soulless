using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    // Coordinates of a tile
    public int x;
    public int y;
    public int tileType;
    public bool isOccupied;

    private Unit associatedUnit = null;

    public Unit AssociatedUnit
    {
        get
        {
            return associatedUnit;
        }

        set
        {
            if (value == null)
                isOccupied = false;
            else
                isOccupied = true;
            associatedUnit = value;
        }
    }
}
