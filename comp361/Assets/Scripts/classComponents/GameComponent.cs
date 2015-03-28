using UnityEngine;
using System.Collections.Generic;

public class GameComponent : MonoBehaviour {
	private Color[] _playerColours = {Color.red, Color.green, Color.blue, Color.yellow};

	/*********************
	 *     ATTRIBUTES    *
	 ********************/
//	public static GameComponent instance;

	public PlayerManager _playerManager;
	public GUIManager _guiManager; 
	public string _currentMap;

	PlayerComponent _currentPlayer;
	PlayerComponent[] _participants;
	List<PlayerComponent> _remainingPlayers;
	TileComponent[,] _mapTiles;
    GameObject[,] _tileObjects;
    TileComponent _lastSelectedTile;
    UnitComponent _lastSelectedUnit;
    bool _moveStarted = false;

	Color _currentColour;
	int _currentPlayerIndex;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/
    public bool isMoveStarted()
    {
        return _moveStarted;
    }

	public PlayerComponent getCurrentPlayer()
	{
		return _currentPlayer;
	}

    public TileComponent getLastSelectedTile()
    {
        return _lastSelectedTile;
    }
    
    public UnitComponent getLastSelectedUnit()
    {
        return _lastSelectedUnit;
    }

    public void setLastSelectedTile(TileComponent lastSelectedTile)
    {
        _lastSelectedTile = lastSelectedTile;
    }

    public void setLastSelectedUnit(UnitComponent lastSelectedUnit)
    {
        _lastSelectedUnit = lastSelectedUnit;
    }

	void setCurrentPlayer(int index) {
		_currentPlayer = _remainingPlayers[index];
		_currentColour = _playerColours [index];
		_guiManager.UpdateGamePanels (_currentPlayer, _currentColour);
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

    public void startMoveLastSelectedUnit()
    {
        _moveStarted = true;
        //_lastSelectedTile.HighlightNeighbours();
        //_lastSelectedTile.UpdateDraw();
        _lastSelectedUnit.moveUnit(_lastSelectedTile);
    }

    public void moveLastSelectedUnit()
    {
        //_lastSelectedTile.UnhighlightNeighbours();
        //_lastSelectedTile.UpdateDraw();
        _lastSelectedUnit.moveUnit(_lastSelectedTile);
    }

    public void finishMoveLastSelectedUnit()
    {
        _moveStarted = false;
    }

	/// <summary>
	/// Creates a new game by generating a map and assigning players to tiles.
	/// </summary>
	/// <param name="participants">Participants.</param>
	public void newGame(List<PlayerComponent> participants) {
		_currentPlayerIndex = 0;
		setRemainingPlayers(participants);
		setCurrentPlayer(_currentPlayerIndex);
		
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
                    }
                    if (!regionContainsVillage)
                    {
                        TileComponent tileWithVillage = region[0];
						VillageComponent newHovel = CreateVillage(tileWithVillage, VillageType.HOVEL, _currentPlayer);
						tileWithVillage.setOccupantType(OccupantType.VILLAGE);
						tileWithVillage.setVillage(newHovel);
						/*
                        GameObject go = new GameObject();
                        go.AddComponent<VillageComponent>().InstantiateVillage(VillageType.HOVEL, _currentPlayer);
                        var newHovel = go.GetComponent<VillageComponent>();
                        newHovel.setOccupyingTile(tileWithVillage);
                        tileWithVillage.setOccupantType(OccupantType.VILLAGE);
                        tileWithVillage.setVillage(newHovel);
						*/

                        PlayerComponent player = participants[playerIndex - 1];
						player.add(newHovel);
						newHovel.associate(tileWithVillage);
						newHovel.addGold(7);
                        // add enough wood to be able to upgrade to village for demo, can remove later
						newHovel.addWood(24);
                        // add all other tiles in the bfs to this village's controlled region
                        foreach (var controlledTile in tileWithVillage.breadthFS())
                        {
							if (!newHovel.getControlledRegion().Contains(controlledTile))
                            {
								newHovel.addToControlledRegion(controlledTile);
								controlledTile.setVillage(newHovel);
                            }
                        }
                    }
                }
            }
            tile.UpdateDraw();
		}
	}

	public VillageComponent CreateVillage(TileComponent tc, VillageType vType, PlayerComponent pc){
		VillageComponent vc = tc.gameObject.AddComponent<VillageComponent>();
		vc.Initialize(VillageType.HOVEL, pc);
		return vc;
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
        BeginRound();
	}

    private void BeginRound()
    {
//        foreach(var player in _remainingPlayers)
//        {
//            player.beginTurn();
//        }
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
		_currentPlayerIndex = (_currentPlayerIndex + 1) % _remainingPlayers.Count;
		setCurrentPlayer (_currentPlayerIndex);

        BeginRound();
    }

	public void endGame() {
        /* TODO */
	}

	public void removePlayer(PlayerComponent player) {
		_remainingPlayers.Remove(player);
	}

	void Awake()
	{
//		instance = this;
	}
	
}
