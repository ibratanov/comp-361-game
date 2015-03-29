using UnityEngine;
using System.Collections;

public class CameraControls : MonoBehaviour {

	float speed;
	public float minSpeed;
	public float maxSpeed;

	GameComponent _game;
	Vector3 initPos;

	void OnEnable () {
		speed = minSpeed;
		_game = GameObject.FindObjectOfType<GameComponent>();
		TileComponent currentTile = _game.getLastSelectedTile();

		if (currentTile != null && currentTile.isSelected)
		{
			transform.position = new Vector3 (currentTile.transform.position.x, transform.position.y, currentTile.transform.position.z);
		}
	}

	void Update () {
		moveCamera();
		adjustSpeed();
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

	void adjustSpeed()
	{
		if (Input.GetKey(KeyCode.LeftShift))
		{
			speed = maxSpeed;
		}
		if (Input.GetKeyUp(KeyCode.LeftShift))
		{
			speed = minSpeed;
		}
	}
}
