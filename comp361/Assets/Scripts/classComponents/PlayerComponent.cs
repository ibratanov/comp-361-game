using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.classComponents;

public enum PlayerStatus {
	OFFLINE,
	ONLINE
}

public class PlayerComponent : GenericComponent
{

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	int _losses;
	int _wins;
	string _password;
	string _userName;

	GameComponent _myGame;
	PlayerStatus _status;
	List<VillageComponent> _villages = new List<VillageComponent>();

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

	public PlayerComponent(string username, string password){
		_losses = 0;
		_wins = 0;
		_userName = username;
		_password = password;
	}

	public PlayerComponent(string username, int nWins, int nLosses){
		_losses = nLosses;
		_wins = nWins;
		_userName = username;
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

	public GameComponent getGame() {
		return _myGame;
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

	public void addVillage(VillageComponent village) {
        _villages.Add(village);
	}

	public void beginTurn() {
		foreach (VillageComponent village in _villages) {
            //village.produceMeadows();
            //village.replaceTombstonesByForest();
            //village.produceRoads();
            //village.updateGoldStock();
            //village.payWages();
		}
	}

	public void incrementLosses() {
		++_losses;
	}

	public void incrementWins() {
		++_wins;
	}

	public void login(string userName, string password) {
		/* TODO */
	}

	public void remove(VillageComponent village) {
		village.gameObject.GetComponent<TileComponent>().setOccupantType (OccupantType.NONE);
		_villages.Remove(village);
		Destroy (village.getVillageGameObject ());
		Destroy (village);
		// TODO: Do we need to handle the whole region removal as well?
	}



	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
}
