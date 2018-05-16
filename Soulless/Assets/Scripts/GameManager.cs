using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject mapPrefab;
    public GameObject unitPrefab;

	// Use this for initialization
	void Start () {
        CreateMap("Map");
        CreateUnit("Unit1", unitPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        CreateUnit("Unit2", unitPrefab, new Vector3(5, 1, 5), Quaternion.identity);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void CreateUnit(string name, GameObject unitPrefab, Vector3 position, Quaternion rotation)
    {
        GameObject obj;
        obj = (GameObject)Instantiate(unitPrefab, position, rotation, GameObject.Find("Units").transform);
        obj.name = name;
    }

    void CreateMap(string name)
    {
        GameObject obj;
        obj = (GameObject)Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        obj.name = name;
    }
}
