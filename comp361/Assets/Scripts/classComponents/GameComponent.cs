using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.classComponents;

[SerializePrivateVariables]
public class GameComponent : GenericComponent
{
	private Color[] _playerColours = {Color.red, Color.green, Color.blue, Color.yellow};

	/*********************
	 *     ATTRIBUTES    *
	 ********************/
//	public static GameComponent instance;

	public PlayerManager _playerManager;
	public GUIManager _guiManager; 
	public string _currentMap;
	public Text _settingsButtonText;

	private MapGenerator _mapGenerator;

	int _roundCount; 

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

		_guiManager.SwitchCameras();
		_guiManager.UpdateGamePanels (_currentPlayer, _currentColour);
	}

	public List<PlayerComponent> getRemainingPlayers() {
		return _remainingPlayers;
	}
	
/*	void setRemainingPlayers(List<PlayerComponent> remainingPlayers) {
		_remainingPlayers = remainingPlayers;
	}
	*/
	

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

    public void buildRoadLastSelectedUnit()
    {
        _lastSelectedUnit.buildRoad();
    }

    public void fireOnLastSelectedTile()
    {
        _lastSelectedUnit.fireOnVillage(_lastSelectedTile);
    }

	/// <summary>
	/// Creates a new game by generating a map and assigning players to tiles.
	/// </summary>
	/// <param name="participants">Participants.</param>
	public void newGame(List<PlayerComponent> participants) {
		InitializePlayers();
		BeginRound();
		StartCoroutine("delaySetPlayer");
		//setCurrentPlayer(_currentPlayerIndex); //this is done in delaySetPlayer()

		if(Network.isServer){
			networkView.RPC("NewGameInit", RPCMode.All);
		}

		_mapGenerator.GenerateMap();
		//UpdateMapRegions();

		/*
		for(int i = 0; i < _mapTiles.GetLength(0); ++i){
			for(int j = 0; j < _mapTiles.GetLength(1); ++j){
				int randIndex = Random.Range(0, participants.Count + 1);

			}
		}
		foreach (var tile in _mapTiles)
		{
			int randIndex = Random.Range(0, participants.Count + 1);
			tile.setPlayerIndex(randIndex);
		}
		*/

		GenerateRegions();
	}

	//Players will increment their turns in alphabetical order to maintain network consistency 
	//(Note, if we want to enforce randomness, int array can be converted to string and passed by RPC). Stretch goal
	private void InitializePlayers(){
		if(Network.isServer){
			networkView.RPC("RPCInitializePlayers", RPCMode.Others);
		}
		RPCInitializePlayers();
	}
	
	[RPC]
	private void RPCInitializePlayers() {
		_remainingPlayers = _playerManager.GetPlayers();
		_remainingPlayers.Sort(
			delegate(PlayerComponent p1, PlayerComponent p2) 
			{ 
			return p1.getUserName().CompareTo(p2.getUserName()); 
		});
		_currentPlayerIndex = 0;
	}

	[RPC]
	private void NewGameInit(){
		_settingsButtonText.text = _playerManager.GetPlayer(0).getUserName();
		_guiManager.HideLoadedProfilePanel();
	}

	private void GenerateRegions(){
		if(Network.isServer){
			networkView.RPC("RPCGenerateRegions", RPCMode.Others);
		}
		RPCGenerateRegions();
	}

	[RPC]
	private void RPCGenerateRegions()
	{
		_mapTiles =_mapGenerator.getLandTiles();
		foreach (var tile in _mapTiles)
		{
			int playerIndex = tile.getPlayerIndex();
			
			if (playerIndex > 0)
			{
				var region = tile.breadthFS();
				if (region.Count < 3)
				{
					foreach (var rTile in region)
					{
						rTile.setPlayerIndex(0);
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
						tileWithVillage.UpdateVillageReference();

						//PlayerComponent player = participants[playerIndex - 1];
						//player.add(newHovel);
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
								controlledTile.UpdateVillageReference();
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
	}

	#region EveryRound

    public void BeginRound()
    {
		print ("new round");
		_roundCount++;
		_guiManager.DisplayRoundPanel(_roundCount);
		StartCoroutine("delaySetPlayer");
	
		if (_roundCount > 1)
		{
			TreeGrowthPhase();
			foreach (PlayerComponent p in _remainingPlayers)
			{
				PlayerPhase(p);
			}
		}
//        foreach(var player in _remainingPlayers)
//        {
//            player.beginTurn();
//        }
    }

	public void TreeGrowthPhase()
	{
		List<TileComponent> tiles = new List<TileComponent>();
		foreach (TileComponent tc in _mapTiles)
		{
			// a forest has 50% chance of spawning another forest on a neighbouring tile
			if (tc.getLandType() == LandType.FOREST)
			{
				if (tiles.Contains (tc))
				{
					continue;
				}
				if (Random.value < .5f)
				{
					tiles.Add (tc);
					tiles.AddRange (tc.getNeighbours());
					TileComponent randomNeighbour = tc.getNeighbours()[Random.Range (0, tc.getNeighbours().Count)];
					
					if (randomNeighbour.getLandType() != LandType.FOREST)
					{
						randomNeighbour.setLandType(LandType.FOREST);
					}
				}
			}
		}
	}

	public void PlayerPhase(PlayerComponent player)
	{
		foreach (VillageComponent vc in player.getVillages())
		{
			// tombstone phase
			foreach (TileComponent tc in vc.getControlledRegion())
			{
				if (tc.getOccupantType() == OccupantType.STRUCTURE)
				{
					// tombstones turn into forests
					if (tc.GetComponent<StructureComponent>().getStructureType() == StructureType.TOMBSTONE)
					{
						Destroy (tc.GetComponent<StructureComponent>());
						tc.setLandType(LandType.FOREST);
					}
				}
			}

			// build phase
			vc.produceMeadows();
			vc.produceRoads();

			// income phase
			foreach (TileComponent tc in vc.getControlledRegion ())
			{
				vc.addGold (tc.getRevenue());
			}

			// payment phase
			vc.payWages ();

			// move & purchase phase begin when function returns
		}
	}

	#endregion


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

		if (_lastSelectedTile != null && _lastSelectedTile.isSelected)
		{
			_lastSelectedTile.Deselect();
		}
		
		if (_currentPlayerIndex == 0)
		{
			BeginRound();

		}
		else
		{
			setCurrentPlayer (_currentPlayerIndex);
		}
    }

	IEnumerator delaySetPlayer()
	{
		yield return new WaitForSeconds(_guiManager._fadeSpeed);
		setCurrentPlayer (_currentPlayerIndex);
	}

	public void endGame() {
        /* TODO */
	}

	public void removePlayer(PlayerComponent player) {
		_remainingPlayers.Remove(player);
	}

	void Awake()
	{
		_roundCount = 0;
//		instance = this;
		_mapGenerator = this.GetComponent<MapGenerator>();
	}
	
}
