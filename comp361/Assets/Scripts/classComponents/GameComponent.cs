﻿using UnityEngine;
using System.Collections;

public class GameComponent : MonoBehaviour {
	private Color[] _playerColours = {Color.red, Color.green, Color.blue, Color.yellow};

	/*********************
	 *     ATTRIBUTES    *
	 ********************/
	public PlayerManager _playerManager;
	public string _currentMap;

	PlayerComponent _currentPlayer;
	PlayerComponent[] _participants;
	PlayerComponent[] _remainingPlayers;
	TileComponent[,] _mapTiles;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

	void setCurrentPlayer(PlayerComponent currentPlayer) {
		_currentPlayer = currentPlayer;
	}

	public PlayerComponent[] getRemainingPlayers() {
		return _remainingPlayers;
	}

	void setRemainingPlayers(PlayerComponent[] remainingPlayers) {
		_remainingPlayers = remainingPlayers;
	}

	/*********************
	 *      METHODS      *
	 ********************/

	/// <summary>
	/// Creates a new game by generating a map and assigning players to tiles.
	/// </summary>
	/// <param name="participants">Participants.</param>
	public void newGame(PlayerComponent[] participants) {
		var firstPlayer = participants[0];
		setRemainingPlayers(participants);
		setCurrentPlayer(firstPlayer);
		
		MapGenerator m = this.GetComponent<MapGenerator>();
		m.GenerateMap();
		
		_mapTiles = m.getLandTiles();
		
		foreach (var tile in _mapTiles)
		{
			int randIndex = Random.Range(0, participants.Length*7);
			tile.setInitialPlayerIndex(randIndex%participants.Length); //TODO: Verify there is no off-by-one error
		}
		
		foreach (var tile in _mapTiles)
		{
			int playerIndex = tile.getInitialPlayerIndex();
			//tile.getGameObject().GetComponent<Renderer>().materials[2].color = Color.white;
			
			if (playerIndex < participants.Length)
			{
				var region = tile.breadthFS();
				if (region.Length < 3)
				{
					foreach (var rTile in region)
					{
						rTile.setInitialPlayerIndex(participants.Length);
					}
				}
				else
				{
					bool regionContainsVillage = false;
					foreach (var rTile in region)
					{
						var regOccupant = rTile.getOccupantType();
						if(regOccupant == OccupantType.VILLAGE){
							regionContainsVillage = true;
						}
						//rTile.getGameObject().GetComponent<Renderer>().materials[2].color = _playerColours[playerIndex];
					}
					if (!regionContainsVillage)
					{
						TileComponent tileWithVillage = region[0];
						var newHovel = new VillageComponent(VillageType.HOVEL);

						tileWithVillage.setOccupantType(OccupantType.VILLAGE);
						tileWithVillage.setVillage(newHovel, true);

					   	var player = participants[playerIndex];
	                   	player.add(newHovel);

						newHovel.associate(tileWithVillage);
						newHovel.addGold(7);
//	                   	UnitComponent newPeasant = newHovel.hireVillager(UnitType.PEASANT);
//	                   	TileComponent villagerTile = region[1];
//	                   	newPeasant.associate(villagerTile);
					}
				}
			}
		}
	}

	/// <summary>
	/// Allows menu buttons to declare which map to generate on BeginGame()
	/// </summary>
	/// <param name="mapName">Map name.</param>
	public void SetMap(string mapName){
		_currentMap = mapName;
	}
	
	/// <summary>
	/// Generate a map based on the current selection.
	/// </summary>
	public void BeginGame(){
		if( _currentMap.Equals("Preset1") ){
			PlayerComponent[] players = _playerManager.GetPlayers();
			newGame(players);
		}
		else if( _currentMap.Equals("TestMap") ){
			TestMapGeneration();
		}
	}

	/// <summary>
	/// Debugging map with predefined players.
	/// </summary>
	public void TestMapGeneration(){
		_playerManager.AddPlayer(new PlayerComponent("Rita", "rita"));
		_playerManager.AddPlayer(new PlayerComponent("Rita2", "rita2"));
		_playerManager.AddPlayer(new PlayerComponent("Rita3", "rita3"));
		newGame(_playerManager.GetPlayers());
	}

	public void endGame() {
		/* TODO */
	}

	public void removePlayer(PlayerComponent player) {
		/* TODO */
	}
}
