using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject mapPrefab;
    public GameObject unitPrefab;

	// Use this for initialization
	void Start () {
        Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        Instantiate(unitPrefab, Vector3.zero, Quaternion.identity);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
