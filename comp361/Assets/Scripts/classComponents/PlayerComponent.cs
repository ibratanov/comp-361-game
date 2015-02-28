using UnityEngine;
using System.Collections;

public enum PlayerStatus {
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

	public int getLosses() {
		return _losses;
	}

	public int getWins() {
		return _wins;
	}

	public string getUserName() {
		return _userName;
	}

	public PlayerComponent Player(string userName, string password) {
		/* TODO */
		return null;
	}

	public PlayerStatus getStatus() {
		return _status;
	}

	public void setStatus(PlayerStatus status) {
		_status = status;
	}

	public VillageComponent[] getVillages() {
		return _villages;
	}

	/*********************
	 *      METHODS      *
	 ********************/

	public void add(VillageComponent village) {
		/* TODO */
	}

	public void beginTurn() {
		/* TODO */
	}

	public void incrementLosses() {
		/* TODO */
	}

	public void incrementWins() {
		/* TODO */
	}

	public void login(string userName, string password) {
		/* TODO */
	}

	public void remove(VillageComponent village) {
		/* TODO */
	}



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
