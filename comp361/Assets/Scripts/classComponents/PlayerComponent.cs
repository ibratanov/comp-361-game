﻿using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.classComponents;

using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public enum PlayerStatus {
	OFFLINE,
	ONLINE
}

public class PlayerComponent : GenericComponent
{
	string _directory = "./profiles/";
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

	//reference: http://zerosalife.github.io/blog/2014/08/09/persistent-data-in-unity/
	public void Save(string username) {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Open(_directory + username + "Info.dat", FileMode.OpenOrCreate);
		
		PlayerData data = new PlayerData();
		data.losses = _losses;
		data.wins = _wins;
		data.userName = _userName;
		int[] villageTileIDs = new int[_villages.Count];
		for(int i = 0; i < _villages.Count; ++i){
			villageTileIDs[i] = _villages[i].GetComponent<TileComponent>().getID();
		}
		data.villageTileIDs = villageTileIDs;
		
		bf.Serialize(file, data);
		file.Close();
	}
	
	public void Load(string username) {
		if(File.Exists(_directory + username + "Info.dat")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(_directory + username + "Info.dat", FileMode.Open);
			PlayerData data = (PlayerData)bf.Deserialize(file);
			file.Close();
			
			_losses = data.losses;
			_wins = data.wins;
			_userName = data.userName;
			int[] villageTileIDs = data.villageTileIDs;
			_villages.Clear();
			for(int i = 0; i < _villages.Count; ++i){
				GameComponent game = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameComponent>();
				VillageComponent village = game.GetTileByID(villageTileIDs[i]).GetComponent<VillageComponent>();
				_villages.Add(village);
			}
		}
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	}
}

[Serializable]
class PlayerData {
	// Add new variables for loading and saving here.
	public int losses;
	public int wins;
	public string userName;
	public int[] villageTileIDs;
}