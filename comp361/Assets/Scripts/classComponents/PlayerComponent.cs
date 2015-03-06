using UnityEngine;
using System.Collections.Generic;

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
	List<VillageComponent> _villages;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

	public PlayerComponent(string username, string password){
		_losses = 0;
		_wins = 0;
		_userName = username;
		_password = password;
	}

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

	public List<VillageComponent> getVillages() {
		return _villages;
	}

	/*********************
	 *      METHODS      *
	 ********************/

	public void add(VillageComponent village) {
        _villages.Add(village);
	}

	public void beginTurn() {
		foreach (VillageComponent village in _villages) {
			village.produceMeadows();
			village.replaceTombstonesByForest();
			village.produceRoads();
			village.updateGoldStock();
			village.payWages();
		}
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
