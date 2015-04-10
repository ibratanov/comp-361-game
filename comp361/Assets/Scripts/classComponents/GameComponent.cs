using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.classComponents;

using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameComponent : GenericComponent
{
	private string _mapDirectory = "./maps/";
	/*********************
	 *     ATTRIBUTES    *
	 ********************/
	//	public static GameComponent instance;
	
	public PlayerManager _playerManager;
	public GUIManager _guiManager; 
	public string _currentMap;
	public Text _settingsButtonText;

	private MapGenerator _mapGenerator;
	private MapData _mapData;
	private bool _isNewGame = true;
	
	int _roundCount; 
	
	PlayerComponent _currentPlayer;
	PlayerComponent _user;
	PlayerComponent[] _participants;
	List<PlayerComponent> _remainingPlayers;
	TileComponent[,] _mapTiles;
	GameObject[,] _tileObjects;
	TileComponent _lastSelectedTile;
	UnitComponent _lastSelectedUnit;
    StructureComponent _lastSelectedStructure;
    UnitComponent _unitToMerge;
    UnitComponent _unitToMove;
	bool _moveStarted = false;
    bool _fireStarted = false;
    bool _merging = false;
    bool _attacking = false;
	
	Color _currentColour;
	int _currentPlayerIndex;
	
	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/
	public TileComponent GetTileByID(int id){
		foreach(TileComponent tile in _mapTiles){
			if(tile.getID() == id){
				return tile;
			}
		}
		return null;
	}

	public void isNewMap(bool isNew){
		_isNewGame = isNew;
	}

	public bool isMoveStarted()
	{
		return _moveStarted;
	}

    public bool isFireStarted()
    {
        return _fireStarted;
    }
	
	public PlayerComponent getCurrentPlayer()
	{
		return _currentPlayer;
	}

	public PlayerComponent getUser()
	{
		return _user;
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

    public void setLastSelectedStructure(StructureComponent lastSelectedStructure)
    {
        _lastSelectedStructure = lastSelectedStructure;
    }

	void setCurrentPlayer(int index) {
		_currentPlayer = _remainingPlayers[index];
		_currentColour = TileComponent.PLAYER_COLOURS[index+1];
		
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
	
    public void HighlightVillages()
    {
        UnHighlightAll();

        foreach (var village in _currentPlayer.getVillages())
        {

            village.GetComponent<TileComponent>().Highlight();
        }
    }

    public void HighlightUnitsWithAvailableActions()
    {
        UnHighlightAll();

        foreach (var village in _currentPlayer.getVillages())
        {
            foreach(var unit in village.getSupportingUnits())
            {
                if (unit.getCurrentAction() == ActionType.READY_FOR_ORDERS)
                {
                    unit.GetComponent<TileComponent>().Highlight();
                }               
            }
        }
        _guiManager.HideOtherActionPanels(String.Empty);
    }

    public void UnHighlightAll()
    {
        foreach (var tile in _mapTiles)
        {
            tile.Unhighlight();
        }
    }

	public void hireVillagerOnLastSelected(int unitType)
	{
		if(Network.isServer||Network.isClient){
			networkView.RPC("RPCHireVillagerOnLastSelected", RPCMode.Others, _lastSelectedTile.getID(), unitType);
		}
		_lastSelectedTile.getVillage().hireVillager((UnitType) unitType);
	}
	[RPC]
	public void RPCHireVillagerOnLastSelected(int tileID, int unitType)
	{
		TileComponent tc = GetTileByID(tileID);
		tc.getVillage().hireVillager((UnitType) unitType);
	}
	
	public void upgradeLastSelectedVillage()
	{
		if(Network.isServer || Network.isClient){
			networkView.RPC( "RPCUpgradeLastSelectedVillage", RPCMode.Others, _lastSelectedTile.getVillage().GetComponent<TileComponent>().getID() );
		}
		RPCUpgradeLastSelectedVillage(_lastSelectedTile.getVillage().GetComponent<TileComponent>().getID());
	}
	[RPC]
	public void RPCUpgradeLastSelectedVillage(int tileID)
	{
		TileComponent tc = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameComponent>().GetTileByID(tileID);
		VillageComponent vc = tc.GetComponent<VillageComponent>();
		vc.upgradeVillage();
	}

    public void upgradeLastSelectedUnit(int unitType)
    {
        _lastSelectedUnit.upgradeUnit((UnitType)unitType);
    }

    public void startMergeLastSelectedUnit(UnitComponent uc)
    {
        _lastSelectedUnit.CombineUnit(uc);
        _merging = false;
    }

    public void setMerge()
    {
        _merging = true;
    }

    public bool isMerging()
    {
        return _merging;
    }

    public void buildWatchtowerLastSelectedTile()
    {
        _lastSelectedTile.buildWatchtower();
    }

    public void startAttacking()
    {
        _attacking = true;
    }

    public bool isAttacking()
    {
        return _attacking;
    }

    public void watchTowerAttackLastSelectedTile()
    {
        _lastSelectedStructure.Attack(_lastSelectedTile);
        _attacking = false;
    }

    public void cultivateMeadowLastSelectedUnit()
    {
        _lastSelectedUnit.cultivateMeadow();
    }


    public void startMoveLastSelectedUnit()
    {
        _moveStarted = true;
        _unitToMove = _lastSelectedUnit;
    }

	
	public void moveLastSelectedUnit()
	{
		_moveStarted = true;
		//_lastSelectedTile.UnhighlightNeighbours();
		//_lastSelectedTile.UpdateDraw();
		if(Network.isServer || Network.isClient){
            int tileID_current = _unitToMove.GetComponent<TileComponent>().getID();
			int tileID_destination = _lastSelectedTile.getID();
			networkView.RPC("RPCMoveLastSelectedUnit", RPCMode.Others, tileID_current, tileID_destination);
		}
		_unitToMove.moveUnit(_lastSelectedTile);
        _moveStarted = false;
	}
	[RPC]
	public void RPCMoveLastSelectedUnit(int tileID_current, int tileID_destination)
	{
		TileComponent tile_current = GetTileByID(tileID_current);
		UnitComponent uc = tile_current.GetComponent<UnitComponent>();
		TileComponent tile_destination = GetTileByID(tileID_destination);
		uc.moveUnit(tile_destination);
	}
	
	public void finishMoveLastSelectedUnit()
	{
		_moveStarted = false;
	}
	
	public void buildRoadLastSelectedUnit()
	{
		_lastSelectedUnit.buildRoad();
	}

    public void fireCannonLastSelectedUnit()
    {
        _lastSelectedUnit.fireOnVillage(_lastSelectedTile);
        _fireStarted = false;
    }

    public void startFireCannon()
    {
        _fireStarted = true;
    }
	
	/// <summary>
	/// Creates a new game by generating a map and assigning players to tiles.
	/// </summary>
	/// <param name="participants">Participants.</param>
	public void newGame(List<PlayerComponent> participants) {
		NewGameInit();
		InitializePlayers();
		_mapGenerator.GenerateMap(_currentMap);
		GenerateRegions();
		BeginRound();
	}

	public void newLoadedGame(string mapName) {
		if(mapName.Equals("TestMap")){
			_playerManager.AddPlayer(new PlayerComponent("Rita", "rita"));
			_playerManager.AddPlayer(new PlayerComponent("Marc", "marc"));
			_playerManager.AddPlayer(new PlayerComponent("Ivo", "ivo"));
		}
		NewGameInit(); //Hide title panels (networked)
		InitializePlayers(); //load all profiles into remaining players (networked)
        if (mapName.Equals("InvadeVillage") || mapName.Equals("StarveVillage")
            || mapName.Equals("ThreewayMerge") || mapName.Equals("ThreewaySplit")
            || mapName.Equals("Combat") || mapName.Equals("Win"))
        {
            LoadScenario(mapName);
        }
        else
        {
            Load(mapName); //Get the mapData (server only)
        }

		
		//Remove players that have been loaded but aren't actually participating in the current map
		foreach(PlayerComponent participant in _playerManager.GetPlayers()){ //Iterate over each player loaded before game started
			bool matchFound = false;
			bool isValidPlayer = true;
			for(int i = 0; i < _mapData.players.Length; ++i){
				PlayerComponent player = _playerManager.GetPlayer(_mapData.players[i]); //Iterate over the players in the save file
				if(player != null){
					if( player.getUserName().Equals(participant.getUserName()) ){
						matchFound = true;
					}
					isValidPlayer = true;
				}
				else{
					isValidPlayer = false;
				}
			}
			if(!matchFound && isValidPlayer){
				removePlayer(participant); //(networked)
			}
		}
		SetRoundAndPlayer(_mapData.round, _mapData.currentPlayerIndex); //set whose turn it is (networked)
		
		//--- TileInfo ---//
		_mapGenerator.GenerateLoadedMap(_mapData); //Create the tile objects and components (networked)

		UpdateGeneratedMap(); //get the TileComponents that were just generated(networked)
		//--- create the villages, units, and structures (networked) ---//
		foreach(VillageData village in _mapData.villages){
			if(village.playerIndex-1 >= 0){
				string playerName = _playerManager.GetPlayer(village.playerIndex-1).getUserName();
				SetupLoadedVillage(village.occupyingTileID, village.villageType, playerName, village.goldStock, village.woodStock, village.remainingHealth);
			}
		}
		foreach(UnitData unit in _mapData.units){
			SetupLoadedUnit(unit.occupyingTileID, unit.homeVillageTileID, unit.unitType, unit.roundsCultivating, unit.currentAction); 
		}
		foreach(StructureData structure in _mapData.structures){
			SetupLoadedStructure(structure.occupyingTileID, structure.playerIndex, structure.structureType); 
		}
		_guiManager.DisplayRoundPanel(_roundCount);
		StartCoroutine("delaySetPlayer");
	}

	[RPC]
	private void generateNeighboursFromMapData(int i, int j, int neighbourID1, int neighbourID2, int neighbourID3, int neighbourID4, int neighbourID5, int neighbourID6)
	{
		List<TileComponent> n = new List<TileComponent>();
		if(neighbourID1 > -1){
			n.Add( GetTileByID(neighbourID1) );
		}
		if(neighbourID2 > -1){
			n.Add( GetTileByID(neighbourID2) );
		}
		if(neighbourID3 > -1){
			n.Add( GetTileByID(neighbourID3) );
		}
		if(neighbourID4 > -1){
			n.Add( GetTileByID(neighbourID4) );
		}
		if(neighbourID5 > -1){
			n.Add( GetTileByID(neighbourID5) );
		}
		if(neighbourID6 > -1){
			n.Add( GetTileByID(neighbourID6) );
		}
		_mapTiles[i,j].setNeighbours(n);
		_mapTiles[i,j].Unhighlight();
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
		_user = _remainingPlayers[0];
		_remainingPlayers.Sort(
			delegate(PlayerComponent p1, PlayerComponent p2) 
			{ 
			return p1.getUserName().CompareTo(p2.getUserName()); 
		});
		_currentPlayerIndex = 0;
	}

	private void NewGameInit(){
		if(Network.isServer){
			networkView.RPC("RPCNewGameInit", RPCMode.Others);
		}
		RPCNewGameInit();
	}

	[RPC]
	private void RPCNewGameInit(){
		if(Network.isServer || Network.isClient){
			_settingsButtonText.text = _playerManager.GetPlayer(0).getUserName();
		}
		_guiManager.HideLoadedProfilePanel();
	}

	
	private void SetRoundAndPlayer(int currentRound, int currentPlayerIndex){
		if(Network.isServer){
			networkView.RPC("RPCSetRoundAndPlayer", RPCMode.Others, currentRound, currentPlayerIndex);
		}
		RPCSetRoundAndPlayer(currentRound, currentPlayerIndex);
	}
	[RPC]
	private void RPCSetRoundAndPlayer(int currentRound, int currentPlayerIndex){
		_roundCount = currentRound;
		_currentPlayerIndex = currentPlayerIndex;
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
		bool firstTile = true;
		foreach (var tile in _mapTiles)
		{
			int playerIndex = tile.getPlayerIndex();
			
			if (playerIndex > 0 && !firstTile)
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
						VillageComponent newHovel = CreateVillage(tileWithVillage, VillageType.HOVEL, _remainingPlayers[playerIndex-1]);
						tileWithVillage.setOccupantType(OccupantType.VILLAGE);
						tileWithVillage.UpdateVillageReference();
						
						//PlayerComponent player = participants[playerIndex - 1];
						//player.add(newHovel);
						newHovel.associate(tileWithVillage);
						newHovel.addGold(50);
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
			if(firstTile){
				tile.setPlayerIndex(0);
				firstTile = false;
			}
			tile.UpdateDraw();
		}
	}
	
	public VillageComponent CreateVillage(TileComponent tc, VillageType vType, PlayerComponent pc){
		VillageComponent vc = tc.gameObject.AddComponent<VillageComponent>();
		vc.Initialize(vType, pc);
		tc.setLandType(LandType.GRASS);
		return vc;
	}
	
	private void UpdateGeneratedMap(){
		if(Network.isServer){
			networkView.RPC("RPCUpdateGeneratedMap", RPCMode.Others);
		}
		RPCUpdateGeneratedMap();
	}
	[RPC]
	private void RPCUpdateGeneratedMap(){
		_mapTiles =_mapGenerator.getLandTiles();
		//--- Connect each tile to its neighbour ---//
		int k = 0;
		for (int i = 0; i < _mapData.width; ++i)
		{
			for (int j = 0; j < _mapData.height; ++j)
			{
				if(Network.isServer){
					networkView.RPC("generateNeighboursFromMapData", RPCMode.Others, i, j, _mapData.tiles[k].neighbourIDs[0], _mapData.tiles[k].neighbourIDs[1], _mapData.tiles[k].neighbourIDs[2], _mapData.tiles[k].neighbourIDs[3], _mapData.tiles[k].neighbourIDs[4], _mapData.tiles[k].neighbourIDs[5]);
				}
				generateNeighboursFromMapData(i, j, _mapData.tiles[k].neighbourIDs[0], _mapData.tiles[k].neighbourIDs[1], _mapData.tiles[k].neighbourIDs[2], _mapData.tiles[k].neighbourIDs[3], _mapData.tiles[k].neighbourIDs[4], _mapData.tiles[k].neighbourIDs[5]);
				k++;
			}
		}
	}
	
	private void SetupLoadedVillage(int tileID, int villageType, string playerName, uint goldStock, uint woodStock, int healthRemaining){
		if(Network.isServer){
			networkView.RPC("RPCSetupLoadedVillage", RPCMode.Others, tileID, villageType, playerName, goldStock, woodStock, healthRemaining);
		}
		RPCSetupLoadedVillage(tileID, villageType, playerName, goldStock, woodStock, healthRemaining);
	}
	[RPC]
	private void RPCSetupLoadedVillage(int tileID, int villageType, string playerName, uint goldStock, uint woodStock, int healthRemaining){
		TileComponent tileWithVillage = GetTileByID(tileID);
		VillageComponent newVillage = CreateVillage(tileWithVillage, (VillageType)villageType, _playerManager.GetPlayer(playerName));
		tileWithVillage.setOccupantType(OccupantType.VILLAGE);
		tileWithVillage.RPCUpdateVillage(); //use the networked method since we're already in an RPC
		
		newVillage.associate(tileWithVillage);
		newVillage.addGold(goldStock);
		// add enough wood to be able to upgrade to village for demo, can remove later
		newVillage.addWood(woodStock);
		// add all other tiles in the bfs to this village's controlled region
		foreach (var controlledTile in tileWithVillage.breadthFS())
		{
			if (!newVillage.getControlledRegion().Contains(controlledTile))
			{
				newVillage.addToControlledRegion(controlledTile);
				controlledTile.UpdateVillageReference();
			}
		}
		int initialHealth = newVillage.GetHealthLeft();
		for(int i = initialHealth - healthRemaining; i > 0; --i){
			newVillage.DecrementHealth();
		}
	}

	private void SetupLoadedUnit(int tileID, int homeVillageTileID, int unitType, uint roundsCultivating, int currentAction){
		if(Network.isServer){
			networkView.RPC("RPCSetupLoadedUnit", RPCMode.Others, tileID, homeVillageTileID, unitType, roundsCultivating, currentAction);
		}
		RPCSetupLoadedUnit(tileID, homeVillageTileID, unitType, roundsCultivating, currentAction);
	}
	[RPC]
	private void RPCSetupLoadedUnit(int tileID, int homeVillageTileID, int unitType, uint roundsCultivating, int currentAction){
		UnitType uType = (UnitType)unitType;
		TileComponent tc = GetTileByID(tileID);
		UnitComponent uc = tc.gameObject.AddComponent<UnitComponent>();
		uc.Initialize(uType);
		uc.setCurrentAction((ActionType)currentAction);
		uc.setRoundsCultivating(roundsCultivating);
		tc.setOccupantType(OccupantType.UNIT);
		tc.setOccupyingUnit(uc);
		VillageComponent vc = tc.getVillage();
		vc.associate(uc);
	}

	private void SetupLoadedStructure(int tileID, int playerIndex, int structureType){
		if(Network.isServer){
			networkView.RPC("RPCSetupLoadedStructure", RPCMode.Others, tileID, playerIndex, structureType);
		}
		RPCSetupLoadedStructure(tileID, playerIndex, structureType);
	}
	[RPC]
	private void RPCSetupLoadedStructure(int tileID, int playerIndex, int structureType){
		TileComponent tc = GetTileByID(tileID);
		StructureComponent sc = tc.gameObject.AddComponent<StructureComponent>();
		sc.setLocation(tc);
		sc.RPCCreateStructure(structureType); //use the networked version because we're already in an RPC
		tc.setOccupantType(OccupantType.STRUCTURE);
		tc.setOccupyingStructure(sc);
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
		if(_isNewGame){
//			if( _currentMap.Equals("Preset1") ){
//				List<PlayerComponent> players = _playerManager.GetPlayers();
//				newGame(players);
//			}
//			else if( _currentMap.Equals("TestMap") ){
//				TestMapGeneration();
//			}
			List<PlayerComponent> players = _playerManager.GetPlayers();
			newGame(players);
		}
		else{
			newLoadedGame(_currentMap);
		}
	}
	
	#region EveryRound

	public void BeginRound(){
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCBeginRound", RPCMode.Others);
		}
		RPCBeginRound();
	}

	[RPC]
	private void RPCBeginRound()
	{
		_roundCount++;
		_guiManager.DisplayRoundPanel(_roundCount);
		StartCoroutine("delaySetPlayer");
		
		if (_roundCount > 1)
		{
			if(!Network.isClient){ //Random actions should only be handled by the server
				TreeGrowthPhase();
			}
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
				if (UnityEngine.Random.value < .5f)
				{
					tiles.Add (tc);
					tiles.AddRange (tc.getNeighbours());
					TileComponent randomNeighbour = tc.getNeighbours()[UnityEngine.Random.Range (0, tc.getNeighbours().Count)];
					
					if (randomNeighbour.getLandType() != LandType.FOREST &&
					    !randomNeighbour.hasRoad() &&
					    randomNeighbour.getOccupyingStructure() == null &&
					    randomNeighbour.getOccupantType() == OccupantType.NONE && 
                        randomNeighbour.getLandType() != LandType.SEA)
					{
						randomNeighbour.setLandType(LandType.FOREST);
						randomNeighbour.Unhighlight();
					}
				}
			}
		}
	}
	
	public void PlayerPhase(PlayerComponent player)
	{
		foreach (VillageComponent vc in player.getVillages()) {
			// tombstone phase
			foreach (TileComponent tc in vc.getControlledRegion()) {
				if (tc.getOccupantType () == OccupantType.STRUCTURE) {
					// tombstones turn into forests
					if (tc.getOccupyingStructure ().getStructureType () == StructureType.TOMBSTONE) {
						tc.getOccupyingStructure ().DestroyStructureGameObject ();
						tc.setLandType (LandType.FOREST);
						tc.setOccupyingStructure (null);
						tc.setOccupantType (OccupantType.NONE);
						tc.UpdateDraw ();
					}
				}
			}
			
			// build phase
			vc.produceMeadows ();
			vc.produceRoads ();
			
			// income phase
//			foreach (TileComponent tc in vc.getControlledRegion ())
//			{
//				vc.addGold (tc.getRevenue());
//			}
			vc.updateGoldStock ();

			// payment phase
			vc.payWages ();
			
			//Reset the ActionType for each unit based on its current ActionType
			List<UnitComponent> units = vc.getSupportingUnits ();
			foreach (UnitComponent unit in units) 
            {

				if (unit.getCurrentAction() == ActionType.EXPANDING_REGION || 
                    unit.getCurrentAction() == ActionType.GATHERING_WOOD || 
                    unit.getCurrentAction() == ActionType.BUILDING_ROAD || 
                    unit.getCurrentAction() == ActionType.ATTACKING ||
                    unit.getCurrentAction() == ActionType.ALREADY_MOVED ||
                    unit.getCurrentAction() == ActionType.EXPANDING_REGION) 
                {
					unit.setCurrentAction(ActionType.READY_FOR_ORDERS);
				} 
			}

			// move & purchase phase begin when function returns
		}
	}
	
	#endregion

	#region CheckPanelUpdates

	public void UpdateVillageActionPanel()
	{
		_guiManager.HideVillageActions();
		_guiManager.DisplayVillageActions(_lastSelectedTile.getVillage ());
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
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCEndTurn", RPCMode.Others);
		}
		RPCEndTurn();
	}

	[RPC]
	private void RPCEndTurn(){
        _moveStarted = false;
		_currentPlayerIndex = (_currentPlayerIndex + 1) % _remainingPlayers.Count;
		
		if (_lastSelectedTile != null && _lastSelectedTile.isSelected)
		{
			_lastSelectedTile.Deselect();
		}
		
		if (_currentPlayerIndex == 0)
		{
			if(!Network.isClient){ //Only the server should call BeginRound, otherwise it'll happen twice for a client
				BeginRound();
			}
		}
		else
		{
			StartCoroutine("delaySetPlayer");
		}
	}
	
	IEnumerator delaySetPlayer()
	{
		yield return new WaitForSeconds(_guiManager._fadeSpeed);
		setCurrentPlayer (_currentPlayerIndex);
	}

	public void endGame() {
		foreach (TileComponent tile in _mapTiles) {
			Destroy(tile.gameObject);
		}
		Save(_currentMap);

		PlayerComponent player = _remainingPlayers[0];
		_remainingPlayers.Clear();
		_remainingPlayers.Add(player);

		List<PlayerComponent> players = _playerManager.GetPlayers();
		players.Clear();
		players.Add(player);
	}

	
	public void removePlayer(PlayerComponent player) {
		_remainingPlayers.Remove(player);
	}
	
	void Awake()
	{
		_roundCount = 0;
		//		instance = this;
		_mapGenerator = this.GetComponent<MapGenerator>();

		if(!Directory.Exists(_mapDirectory))//if it doesn't, create it
		{    
			Directory.CreateDirectory(_mapDirectory);
		}
	}

	//reference: http://zerosalife.github.io/blog/2014/08/09/persistent-data-in-unity/
	public void Save(string mapName) {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Open(_mapDirectory + mapName + "Info.dat", FileMode.OpenOrCreate);
		
		int i = 0;
		MapData mapData = new MapData();
		//--- PlayerInfo ---//
		mapData.round = _roundCount;
		mapData.players = new string[_playerManager.GetPlayers().Count];
		for(i = 0; i < mapData.players.Length; ++i){
			mapData.players[i] = _playerManager.GetPlayer(i).getUserName();
		}
		mapData.remainingPlayers = new string[_remainingPlayers.Count];
		for(i = 0; i < mapData.remainingPlayers.Length; ++i){
			mapData.remainingPlayers[i] = _remainingPlayers[i].getUserName();
		}
		mapData.currentPlayerIndex = _currentPlayerIndex;
		
		//--- TileInfo ---//
		mapData.tiles = new TileData[_mapTiles.GetLength(0)*_mapTiles.GetLength(1)];
		mapData.height = _mapTiles.GetLength(0);
		mapData.width = _mapTiles.GetLength(1);
		i = 0;
		List<VillageComponent> savedVillages = new List<VillageComponent>();
		List<UnitComponent> savedUnits = new List<UnitComponent>();
		List<StructureComponent> savedStructures = new List<StructureComponent>();
		foreach(TileComponent tile in _mapTiles){
			mapData.tiles[i] = new TileData();
			mapData.tiles[i].x = tile.transform.position.x;
			mapData.tiles[i].y = tile.transform.position.y;
			mapData.tiles[i].z = tile.transform.position.z;
			mapData.tiles[i].tileID = tile.getID();
			mapData.tiles[i].playerIndex = tile.getPlayerIndex();
			mapData.tiles[i].hasRoad = tile.hasRoad();
			mapData.tiles[i].landType = (int)tile.getLandType();
			mapData.tiles[i].occupantType = (int)tile.getOccupantType();
			mapData.tiles[i].neighbourIDs = new int[6];
			List<TileComponent> neighbours = tile.getNeighbours();
			int j = 0;
			for(; j < tile.getNeighbours().Count; ++j){
				mapData.tiles[i].neighbourIDs[j] = neighbours[j].getID();
			}
			for(; j < 6; ++j){
				mapData.tiles[i].neighbourIDs[j] = -1; //Fill the rest of the neighbour slots with dummy values
			}
			if(tile.getVillage() != null){
				mapData.tiles[i].homeVillageID = tile.getVillage().GetComponent<TileComponent>().getID();
			}
			else{
				mapData.tiles[i].homeVillageID = -1;
			}
			
			switch(tile.getOccupantType()){
			case(OccupantType.VILLAGE):
				savedVillages.Add(tile.GetComponent<VillageComponent>());
				break;
			case(OccupantType.UNIT):
				savedUnits.Add(tile.GetComponent<UnitComponent>());
				break;
			case(OccupantType.STRUCTURE):
				savedStructures.Add(tile.GetComponent<StructureComponent>());
				break;
			}
			++i;
		}
		
		//--- VillageInfo ---//
		mapData.villages = new VillageData[savedVillages.Count];
		for(i = 0; i < mapData.villages.Length; ++i){
			VillageComponent village = savedVillages[i];
			mapData.villages[i] = new VillageData();
			mapData.villages[i].occupyingTileID = village.GetComponent<TileComponent>().getID();
			mapData.villages[i].playerIndex = village.GetComponent<TileComponent>().getPlayerIndex();
			mapData.villages[i].goldStock = village.getGoldStock();
			mapData.villages[i].woodStock = village.getWoodStock();
			List<TileComponent> region = village.getControlledRegion();
			mapData.villages[i].controlledRegionIDs = new int[region.Count];
			for(int j = 0; j < region.Count; ++j){
				mapData.villages[i].controlledRegionIDs[j] = region[j].getID();
			}
			List<UnitComponent> support = village.getSupportingUnits();
			mapData.villages[i].supportingUnitIDs = new int[support.Count];
			for(int j = 0; j < support.Count; ++j){
				mapData.villages[i].supportingUnitIDs[j] = support[j].GetComponent<TileComponent>().getID();
			}
			mapData.villages[i].villageType = (int)village.getVillageType();
			mapData.villages[i].remainingHealth = village.GetHealthLeft();
		}
		
		//--- UnitInfo ---//
		mapData.units = new UnitData[savedUnits.Count];
		for(i = 0; i < mapData.units.Length; ++i){
			UnitComponent unit = savedUnits[i];
			mapData.units[i] = new UnitData();
			mapData.units[i].occupyingTileID = unit.GetComponent<TileComponent>().getID();
			mapData.units[i].playerIndex = unit.GetComponent<TileComponent>().getPlayerIndex();
			mapData.units[i].roundsCultivating = unit.getRoundsCultivating();
			mapData.units[i].upkeep = unit.getUpkeep();
			mapData.units[i].currentAction = (int)unit.getCurrentAction();
			mapData.units[i].unitType = (int)unit.getUnitType();
			mapData.units[i].homeVillageTileID = unit.getVillage().GetComponent<TileComponent>().getID();
		}
		
		//--- StructureInfo ---//
		mapData.structures = new StructureData[savedStructures.Count];
		for(i = 0; i < mapData.structures.Length; ++i){
			StructureComponent structure = savedStructures[i];
			mapData.structures[i] = new StructureData();
			mapData.structures[i].occupyingTileID = structure.GetComponent<TileComponent>().getID();
			mapData.structures[i].playerIndex = structure.GetComponent<TileComponent>().getPlayerIndex();
			mapData.structures[i].structureType = (int)structure.getStructureType();
		}
		
		bf.Serialize(file, mapData);
		file.Close();
		Debug.Log("Saved: " + _mapDirectory + mapName + "Info.dat");
	}
	
	public void Load(string mapName) {
		if(File.Exists(_mapDirectory + mapName + "Info.dat")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(_mapDirectory + mapName + "Info.dat", FileMode.Open);
			_mapData = (MapData)bf.Deserialize(file);
			file.Close();
		}
	}

    public void LoadScenario(string mapName)
    {
        string mapDirectory = "./SavedScenarios/";
        if (File.Exists(mapDirectory + mapName + ".dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(mapDirectory + mapName + ".dat", FileMode.Open);
            _mapData = (MapData)bf.Deserialize(file);
            file.Close();
        }
    }
}

[Serializable]
public class MapData {
	// Add new variables for loading and saving here.
	public int round;
	public string[] players; //just used to check if the players playing match the ones saved in the game
	public string[] remainingPlayers;
	public int currentPlayerIndex;
	public int height; //the number of rows
	public int width; //the number of columns
	public TileData[] tiles;
	public VillageData[] villages;
	public UnitData[] units;
	public StructureData[] structures;
}

[Serializable]
public class TileData {
	// Add new variables for loading and saving here.
	public float x,y,z;
	public int tileID;
	public int playerIndex;
	public bool hasRoad;
	public int landType;
	public int occupantType;
	public int[] neighbourIDs;
	public int homeVillageID;
}

[Serializable]
public class VillageData {
	// Add new variables for loading and saving here.
	public int occupyingTileID;
	public int playerIndex;
	public uint goldStock;
	public uint woodStock;
	public int[] controlledRegionIDs;
	public int[] supportingUnitIDs;
	public int villageType;
	public int remainingHealth;
}

[Serializable]
public class UnitData {
	// Add new variables for loading and saving here.
	public int occupyingTileID;
	public int playerIndex;
	public uint roundsCultivating;
	public uint upkeep;
	public int currentAction;
	public int unitType;
	public int homeVillageTileID;
}

[Serializable]
public class StructureData {
	// Add new variables for loading and saving here.
	public int occupyingTileID;
	public int playerIndex;
	public int structureType;
}
