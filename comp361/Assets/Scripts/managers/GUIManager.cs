using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	public GameObject game;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI(){
		if(GUI.Button(new Rect(300,100,250,100), "GenerateMap")){
			MapGenerator mg = game.GetComponent<MapGenerator>();
			mg.GenerateMap();
		}
	}
}
