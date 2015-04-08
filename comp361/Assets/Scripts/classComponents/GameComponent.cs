using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.classComponents;

[SerializePrivateVariables]
public class GameComponent : GenericComponent
{
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
		_lastSelectedTile.getVillage().upgradeVillage();
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
		_mapGenerator.GenerateMap();
		GenerateRegions();
		BeginRound();
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
			tile.UpdateDraw();
		}
	}
	
	public VillageComponent CreateVillage(TileComponent tc, VillageType vType, PlayerComponent pc){
		VillageComponent vc = tc.gameObject.AddComponent<VillageComponent>();
		vc.Initialize(VillageType.HOVEL, pc);
		tc.setLandType(LandType.GRASS);
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
				if (Random.value < .5f)
				{
					tiles.Add (tc);
					tiles.AddRange (tc.getNeighbours());
					TileComponent randomNeighbour = tc.getNeighbours()[Random.Range (0, tc.getNeighbours().Count)];
					
					if (randomNeighbour.getLandType() != LandType.FOREST &&
					    !randomNeighbour.hasRoad() &&
					    randomNeighbour.getOccupyingStructure() == null &&
					    randomNeighbour.getOccupantType() == OccupantType.NONE)
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
			foreach (UnitComponent unit in units) {

				if (unit.getCurrentAction() == ActionType.EXPANDING_REGION || unit.getCurrentAction() == ActionType.GATHERING_WOOD) {
					unit.setCurrentAction(ActionType.READY_FOR_ORDERS);
				} else if (unit.getCurrentAction() == ActionType.ATTACKING) {
				//TODO: reset for other ActionTypes
				}
			}

			// move & purchase phase begin when function returns
		}
	}
	
	#endregion

	#region CheckPanelUpdates

	public void checkVillageActions()
	{
		_guiManager.DisplayVillageActions(_lastSelectedTile.GetComponent<VillageComponent>());
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
	}

}
