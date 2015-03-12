using UnityEngine;
using System.Collections.Generic;

public class GameComponent : MonoBehaviour {
	private Color[] _playerColours = {Color.red, Color.green, Color.blue, Color.yellow};

	/*********************
	 *     ATTRIBUTES    *
	 ********************/
	public PlayerManager _playerManager;
	public string _currentMap;

	PlayerComponent _currentPlayer;
	PlayerComponent[] _participants;
	List<PlayerComponent> _remainingPlayers;
	TileComponent[,] _mapTiles;
    GameObject[,] _tileObjects;
    TileComponent _lastSelectedTile;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

    public void setLastSelectedTile(TileComponent lastSelectedTile)
    {
        _lastSelectedTile = lastSelectedTile;
    }

	void setCurrentPlayer(PlayerComponent currentPlayer) {
		_currentPlayer = currentPlayer;
	}

	public List<PlayerComponent> getRemainingPlayers() {
		return _remainingPlayers;
	}

	void setRemainingPlayers(List<PlayerComponent> remainingPlayers) {
		_remainingPlayers = remainingPlayers;
	}

	/*********************
	 *      METHODS      *
	 ********************/

    public void hireVillagerOnLastSelected(int unitType)
    {
        _lastSelectedTile.getVillage().hireVillager((UnitType) unitType);
    }

    public void upgradeLastSelectedVillage()
    {
        _lastSelectedTile.getVillage().upgradeVillage();
    }

	/// <summary>
	/// Creates a new game by generating a map and assigning players to tiles.
	/// </summary>
	/// <param name="participants">Participants.</param>
	public void newGame(List<PlayerComponent> participants) {
		var firstPlayer = participants[0];
		setRemainingPlayers(participants);
		setCurrentPlayer(firstPlayer);
		
		MapGenerator m = this.GetComponent<MapGenerator>();
		m.GenerateMap();
		
		_mapTiles = m.getLandTiles();
		
		foreach (var tile in _mapTiles)
		{
			int randIndex = Random.Range(0, participants.Count + 1);
			tile.setInitialPlayerIndex(randIndex);
		}
		
		foreach (var tile in _mapTiles)
		{
            int playerIndex = tile.getInitialPlayerIndex();
            //tile.getGameObject().GetComponent<Renderer>().materials[2].color = Color.white;

            if (playerIndex > 0)
            {
                var region = tile.breadthFS();
                if (region.Count < 3)
                {
                    foreach (var rTile in region)
                    {
                        rTile.setInitialPlayerIndex(0);
                    }
                }
                else
                {
                    bool regionContainsVillage = false;
                    foreach (var rTile in region)
                    {
                        var regOccupant = rTile.getOccupantType();
                        if (regOccupant == OccupantType.VILLAGE)
                        {
                            regionContainsVillage = true;
                        }
                        //rTile.getGameObject().GetComponent<Renderer>().materials[2].color = _playerColours[playerIndex];
                    }
                    if (!regionContainsVillage)
                    {
                        TileComponent tileWithVillage = region[0];
                        var newHovel = new VillageComponent(VillageType.HOVEL, _currentPlayer);
                        newHovel.setOccupyingTile(tileWithVillage);
                        tileWithVillage.setOccupantType(OccupantType.VILLAGE);
                        tileWithVillage.setVillage(newHovel);

                        var player = participants[playerIndex - 1];
                        player.add(newHovel);
                        newHovel.associate(tileWithVillage);
                        newHovel.addGold(7);
                        // add enough wood to be able to upgrade to village for demo, can remove later
                        newHovel.addWood(8);
                        //UnitComponent newPeasant = newHovel.hireVillager(UnitType.PEASANT);
                        //TileComponent villagerTile = region[1];
                        //newPeasant.associate(villagerTile);
                        // add all other tiles in the bfs to this village's controlled region
                        foreach (var controlledTile in tileWithVillage.breadthFS())
                        {
                            if (!newHovel.getControlledRegion().Contains(controlledTile))
                            {
                                newHovel.addToControlledRegion(controlledTile);
                            }
                        }

                    }
                }
            }
            tile.UpdateDraw();
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
			List<PlayerComponent> players = _playerManager.GetPlayers();
			newGame(players);
		}
		else if( _currentMap.Equals("TestMap") ){
			TestMapGeneration();
		}
        //GameLoop();
	}

    private void BeginTurn()
    {

    }

    /// <summary>
    /// Main loop for the game
    /// </summary>
    public void GameLoop()
    {
        while (true) // I know this is terrible practice, should be replaced with while(nobodyWins()) when we have that
        {
            BeginTurn();
        }
    }

	/// <summary>
	/// Debugging map with predefined players.
	/// </summary>
	public void TestMapGeneration(){
		_playerManager.AddPlayer(new PlayerComponent("Rita", "rita"));
		_playerManager.AddPlayer(new PlayerComponent("Marc", "marc"));
		_playerManager.AddPlayer(new PlayerComponent("Ivo", "ivo"));
		newGame(_playerManager.GetPlayers());
	}

    public void endTurn()
    {
        for (int i = 0; i < _remainingPlayers.Count; i++)
        {
            if (_remainingPlayers[i] == _currentPlayer)
            {
                _currentPlayer = _remainingPlayers[(i + 1) % _remainingPlayers.Count];
            }
        }
    }

	public void endGame() {
        /* TODO */
	}

	public void removePlayer(PlayerComponent player) {
		_remainingPlayers.Remove(player);
	}
}
