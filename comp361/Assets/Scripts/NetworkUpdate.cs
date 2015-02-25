using UnityEngine;
using System.Collections;

public class NetworkUpdate : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

/*	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncColorVector = Vector3.zero;
		Color color = Color.red;
		if (stream.isWriting)
		{
			color = renderer.material.GetColor("_Color");
			syncColorVector.Set(color.r, color.g, color.b);
			stream.Serialize(ref syncColorVector);
		}
		else
		{
			stream.Serialize(ref syncColorVector);
			color.r = syncColorVector.x;
			color.g = syncColorVector.y;
			color.b = syncColorVector.z;
			renderer.material.SetColor("_Color", color);
		}
	}
*/

	void OnMouseDown() {
		networkView.RPC ("ToggleColours", RPCMode.All);
	}

	[RPC]
	void ToggleColours(){
		if( renderer.materials[2].GetColor("_Color") == Color.red){
			renderer.materials[2].SetColor("_Color", Color.white);
		}
		else{
			renderer.materials[2].SetColor("_Color", Color.red);
		}
		
		Debug.Log("changing colors");
	}
}
