using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    private Tile destinationTile;
    public float speed = 2;
    public bool isMoving = false;
    public float moves = 5f;
    public float movesLeft = 5f;
    public Map map;

    Vector3 destination;
    Vector3 offset = new Vector3(0, 5, 0);
    //Map map;

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
        int layer_mask = LayerMask.GetMask("Map"); // We're only hitting tiles here
        float maxDistance = 1000f;
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, maxDistance, layer_mask))
        {
            // Identify the object we hit
            GameObject hitObject = hitInfo.collider.transform.parent.gameObject;

            // Check what kind of object this is
            if (hitObject.GetComponent<Map>() != null)
            {
                // We are over a mesh
                map = hitObject.GetComponent<Map>();
                Vector3 meshBorder = map.GetMeshBorder();
                Vector3 meshWorldSize = map.GetMeshWorldSize();
                Vector3 hitCoord = hitInfo.transform.InverseTransformPoint(hitInfo.point + meshWorldSize / 2 - meshBorder);

                // Check if we're hitting an actual tile
                if (hitCoord.x >= 0 && hitCoord.x <= meshWorldSize.x - meshBorder.x * 2 && hitCoord.z >= 0 && hitCoord.z <= meshWorldSize.z - meshBorder.z * 2)
                {
                    DestinationTile = map.GetTileByCoord(hitCoord);
                    destination = map.GetTileWorldPosition(DestinationTile) + offset;
                    DestinationTile.AssociatedUnit = this;
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update () {

        destination = map.GetTileWorldPosition(DestinationTile) + offset;

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
