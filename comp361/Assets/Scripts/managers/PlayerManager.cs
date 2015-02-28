using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {
	ArrayList _players = new ArrayList();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void AddPlayer(GameObject playerProfile){
		_players.Add(playerProfile);
	}

	public GameObject GetPlayer(int index){
		return (GameObject)_players[index];
	}
}
