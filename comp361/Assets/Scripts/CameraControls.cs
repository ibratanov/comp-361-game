using UnityEngine;
using System.Collections;

public class CameraControls : MonoBehaviour {

	public float speed;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		moveCamera();
	}

	void moveCamera()
	{
		if (Input.GetKey (KeyCode.W))
		{
			transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z - speed);
		}
		else if (Input.GetKey (KeyCode.S))
		{
			transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + speed);
		}

		if (Input.GetKey (KeyCode.A))
		{
			transform.position = new Vector3 (transform.position.x + speed, transform.position.y, transform.position.z);
		}
		else if (Input.GetKey (KeyCode.D))
		{
			transform.position = new Vector3 (transform.position.x - speed, transform.position.y, transform.position.z);
		}
	}
}
