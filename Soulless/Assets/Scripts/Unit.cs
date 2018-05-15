using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    public Tile destinationTile;
    public float speed = 2;
    public bool isMoving = false;
    public float moves = 5f;
    public float movesLeft = 5f;

    Vector3 destination;

	// Use this for initialization
	void Start () {
        destinationTile = GameObject.Find("tile_0_0").GetComponent<Tile>();
        destination = transform.position;
        destinationTile.associatedUnit = this;
	}
	
	// Update is called once per frame
	void Update () {

        destination = destinationTile.transform.position;

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
