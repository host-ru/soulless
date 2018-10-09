using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float panSpeed = 20f;
    public float borderThickness = 10f;
    public Vector2 panLimit;
    public float scrollSpeed = 20f;
    public float scrollAngle = 1f;
    public float minY = 20f;
    public float maxY = 120f;
    public float smoothSpeed = 10f;

    Vector3 initialPosition;
    Quaternion initialRotation;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        transform.position = new Vector3(transform.position.x / 2, transform.position.y, transform.position.z / 2);
    }

    // Update is called once per frame
    void Update () {

        Vector3 desiredPosition = transform.position;
        float panSpeedByAxis = panSpeed / Mathf.Sqrt(2);

		if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - borderThickness && Input.mousePosition.y <= Screen.height)
        {
            desiredPosition.z += panSpeedByAxis * Time.deltaTime;
            desiredPosition.x += panSpeedByAxis * Time.deltaTime;
        }
        else if (Input.GetKey("s") || Input.mousePosition.y <= borderThickness && Input.mousePosition.y >= 0)
        {
            desiredPosition.z -= panSpeedByAxis * Time.deltaTime;
            desiredPosition.x -= panSpeedByAxis * Time.deltaTime;
        }
        else if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - borderThickness && Input.mousePosition.x <= Screen.width)
        {
            desiredPosition.z -= panSpeedByAxis * Time.deltaTime;
            desiredPosition.x += panSpeedByAxis * Time.deltaTime;
        }
        else if (Input.GetKey("a") || Input.mousePosition.x <= borderThickness && Input.mousePosition.x >= 0)
        {
            desiredPosition.z += panSpeedByAxis * Time.deltaTime;
            desiredPosition.x -= panSpeedByAxis * Time.deltaTime;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        desiredPosition.y -= scroll * scrollSpeed * 100f * Time.deltaTime;

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, initialPosition.x, (float) panLimit.x / 2 - initialPosition.x);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        desiredPosition.z = Mathf.Clamp(desiredPosition.z, initialPosition.z, (float) panLimit.y / 2 - initialPosition.z);

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        Quaternion desiredRotation = Quaternion.Euler(initialRotation.eulerAngles.x - (initialPosition.y - desiredPosition.y) * scrollAngle,  initialRotation.eulerAngles.y, initialRotation.eulerAngles.z);
        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed * Time.deltaTime);
        transform.rotation = smoothedRotation;
        //transform.LookAt(GameObject.Find("Unit1").transform);
    }
}
