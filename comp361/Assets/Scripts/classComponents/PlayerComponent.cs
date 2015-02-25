using UnityEngine;
using System.Collections;

enum PlayerStatus {
	OFFLINE,
	ONLINE
}

public class PlayerComponent : MonoBehaviour {

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	int _losses;
	int _wins;
	string _password;
	string _userName;

	GameComponent _myGame;
	PlayerStatus _status;
	VillageComponent[] _villages;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

	int getLosses() {
		return _losses;
	}

	int getWins() {
		return _wins;
	}

	string getUserName() {
		return _userName;
	}

	PlayerComponent Player(string userName, string password) {
		/* TODO */
		return null;
	}

	PlayerStatus getStatus() {
		return _status;
	}

	void setStatus(PlayerStatus status) {
		_status = status;
	}

	VillageComponent[] getVillages() {
		return _villages;
	}

	/*********************
	 *      METHODS      *
	 ********************/

	void add(VillageComponent village) {
		/* TODO */
	}

	void beginTurn() {
		/* TODO */
	}

	void incrementLosses() {
		/* TODO */
	}

	void incrementWins() {
		/* TODO */
	}

	void login(string userName, string password) {
		/* TODO */
	}

	void remove(VillageComponent village) {
		/* TODO */
	}



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
