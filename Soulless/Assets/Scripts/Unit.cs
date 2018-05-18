using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Unit : MonoBehaviour {

    private Tile destinationTile;
    public float speed = 2;
    public bool isMoving = false;
    public float moves = 5f;
    public float movesLeft = 5f;

    Vector3 destination;
    Vector3 offset = new Vector3(0, 1, 0);

    public Tile DestinationTile
    {
        get
        {
            return destinationTile;
        }

        set
        {
            if (destinationTile != null)
                destinationTile.AssociatedUnit = null;
            destinationTile = value;
            destinationTile.AssociatedUnit = this;
        }
    }

    // Use this for initialization
    void Start () {

        Ray ray = new Ray(transform.position, Vector3.down);
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
                DestinationTile = hitObject.GetComponent<Tile>();
                destination = DestinationTile.transform.position + offset;
                DestinationTile.AssociatedUnit = this;
            }
        }

        //destinationTile = GameObject.Find("tile_0_0").GetComponent<Tile>();
        //destination = transform.position;
        //destinationTile.associatedUnit = this;
	}
	
	// Update is called once per frame
	void Update () {

        destination = DestinationTile.transform.position + offset;

        // Move towards destination
        Vector3 direction = destination - transform.position;
        Vector3 velocity = direction.normalized * speed * Time.deltaTime;

        // Make sure velocity doesn't exceed destination
        velocity = Vector3.ClampMagnitude(velocity, direction.magnitude);

        if (velocity.magnitude != 0)
            isMoving = true;
        else
            isMoving = false;

        transform.Translate(velocity);
	}
}
