using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum LandType { 
	GRASS,
	MEADOW,
	FOREST,
	SEA
}

public enum OccupantType {
	NONE,
	UNIT,
	VILLAGE,
	STRUCTURE
}

public class TileComponent : MonoBehaviour {
	readonly static Color[] PLAYER_COLOURS = {	Color.white,
												new Color(1.0f,0.25f,0.25f), 
												new Color(0.25f,1.0f,0.25f), 
												new Color(0.25f,0.25f,1.0f), 
												new Color(0.25f,1.0f,1.0f), 
												new Color(1.0f,0.25f,1.0f), 
												new Color(1.0f,1.0f,0.25f), 
											};

	private GameObject _terrainGameObject;
	private GameObject _villageGameObject;
    private GameObject _tileGameObject;
	private AssetManager _assets;
	private GUIManager _menus;

	readonly static uint MEADOW_REVENUE = 2;
	readonly static uint FOREST_REVENUE = 0;
	readonly static uint LANDTYPE_REVENUE = 1;

	private bool _drawUpdated = true; //True when an update to the visuals need to be made. Use UpdateDraw() to turn on.

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	bool _hasRoad;
	int _initialPlayerIndex;

	GameComponent _game;
	LandType _landType;
	OccupantType _occupantType;
	StructureComponent _occupyingStructure;
	List<TileComponent> _neighbours;
	UnitComponent _occupyingUnit;
	VillageComponent _village;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

    public GameObject getTileGameObject()
    {
        return _tileGameObject;
    }

    public void setTileGameObject(ref GameObject tileGameObject)
    {
        _tileGameObject = tileGameObject;
    }

    public GameObject getTerrainGameObject()
    {
        return _terrainGameObject;
    }

    public void setTerrainGameObject(ref GameObject terrainGameObject)
    {
        _terrainGameObject = terrainGameObject;
    }

    public GameObject getVillageGameObject()
    {
        return _villageGameObject;
    }

    public void setVillageGameObject(ref GameObject villageGameObject)
    {
        _villageGameObject = villageGameObject;
    }

	public int getInitialPlayerIndex() {
		return _initialPlayerIndex;
	}

	public void setInitialPlayerIndex(int initialPlayerIndex) {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCsetInitialPlayerIndex", RPCMode.All, initialPlayerIndex);
		}
		else{
			_initialPlayerIndex = initialPlayerIndex;
		}
	}

	[RPC]
	private void RPCsetInitialPlayerIndex(int initialPlayerIndex){
		_initialPlayerIndex = initialPlayerIndex;
	}


	public void setLandType(LandType landType) {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCsetLandType", RPCMode.All, (int)landType);
		}
		else{
			_landType = landType;
			_terrainGameObject = _assets.getTerrainGameObject(landType);
		}
	}
	
	[RPC]
	public void RPCsetLandType(int landTypeIndex) {
		_landType = (LandType)landTypeIndex;
		_terrainGameObject = _assets.getTerrainGameObject((LandType)landTypeIndex);
	}

	public LandType getLandType() {
		return _landType;
	}


	public void setVillage(VillageComponent village) {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCsetVillage", RPCMode.All,(int)village.getVillageType() ); //TODO: Pass the appropriate parameters
		}
		else{
			_village = village;

			if (_occupantType == OccupantType.VILLAGE) {
				_landType = LandType.GRASS;
				_terrainGameObject = _assets.getVillageGameObject(village.getVillageType());
			}
		}
	}

	[RPC]
	public void RPCsetVillage(int villageTypeIndex) {
		//TODO: Need to figure out a strategy to update the village across the network. 
		//	Suggestion: (1)Find string/int representations of the data. 
		//				(2)Learn about proper serialization over the network
		//				(3)Perhaps the other player doesn't need to know the data for the Village if it doesn't affect them

		//_village = village;
		
		if (_occupantType == OccupantType.VILLAGE) {
			_landType = LandType.GRASS;
			_terrainGameObject = _assets.getVillageGameObject((VillageType)villageTypeIndex);
		}
	}


	public VillageComponent getVillage() {
		return _village;
	}

	public OccupantType getOccupantType() {
		return _occupantType;
	}

	public void setOccupantType(OccupantType occupantType) {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCsetOccupantType", RPCMode.All, (int)occupantType);
		}
		else{
			_occupantType = occupantType;
			//Remove any links to previous occupants
			if(occupantType == OccupantType.VILLAGE){
				_occupyingStructure = null;
				_occupyingUnit = null;
			}
			else if(occupantType == OccupantType.STRUCTURE){
				_occupyingUnit = null;
			}
			else if(occupantType == OccupantType.UNIT){
				_occupyingStructure = null;
			}
		}
	}

	[RPC]
	public void RPCsetOccupantType(int occupantTypeIndex) {
		_occupantType = (OccupantType)occupantTypeIndex;
		//Remove any links to previous occupants
		if(_occupantType == OccupantType.VILLAGE){
			_occupyingStructure = null;
			_occupyingUnit = null;
		}
		else if(_occupantType == OccupantType.STRUCTURE){
			_occupyingUnit = null;
		}
		else if(_occupantType == OccupantType.UNIT){
			_occupyingStructure = null;
		}
	}


	public void setOccupyingStructure(StructureComponent occupyingStructure) {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCsetOccupyingStructure", RPCMode.All); //TODO: Pass the appropriate parameters
		}
		else{
			_occupyingStructure = occupyingStructure;
		}
	}

	[RPC]
	public void RPCsetOccupyingStructure() {
		//TODO: Need to figure out a strategy to update the village across the network. 
		//	Suggestion: (1)Find string/int representations of the data. 
		//				(2)Learn about proper serialization over the network
		//				(3)Perhaps the other player doesn't need to know the data for the Village if it doesn't affect them
	
		//_occupyingStructure = occupyingStructure;
	}

	public StructureComponent getOccupyingStructure() {
		return _occupyingStructure;
	}


	public void setOccupyingUnit(UnitComponent occupyingUnit) {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCsetOccupyingUnit", RPCMode.All); //TODO: Pass the appropriate parameters
		}
		else{
			_occupyingUnit = occupyingUnit;
		}
	}

	[RPC]
	public void RPCsetOccupyingUnit() {
		//TODO: Need to figure out a strategy to update the village across the network. 
		//	Suggestion: (1)Find string/int representations of the data. 
		//				(2)Learn about proper serialization over the network
		//				(3)Perhaps the other player doesn't need to know the data for the Village if it doesn't affect them

		//_occupyingUnit = occupyingUnit;
	}

	public UnitComponent getOccupyingUnit() {
		return _occupyingUnit;
	}

	
    public void setNeighbours(List<TileComponent> neighbours)
    {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCsetNeighbours", RPCMode.All); //TODO: Pass the appropriate parameters
		}
		else{
        	_neighbours = neighbours;
		}
    }

	[RPC]
	public void RPCsetNeighbours()
	{
		//TODO: Need to figure out a strategy to update the village across the network. 
		//	Suggestion: (1)Find string/int representations of the data. 
		//				(2)Learn about proper serialization over the network
		//				(3)Perhaps the other player doesn't need to know the data for the Village if it doesn't affect them

		//_neighbours = neighbours;
	}

	public List<TileComponent> getNeighbours()
	{
		return _neighbours;
	}


	/*********************
	 *      METHODS      *
	 ********************/

	public static bool containsVillage(List<TileComponent> region) {
		bool containsVillage = false;

		foreach (TileComponent tile in region) {
			OccupantType occupantType = tile.getOccupantType();
			if (occupantType == OccupantType.VILLAGE) {
				containsVillage = true;
				break;
			}
		}

		return containsVillage;
	}

	public bool hasRoad() {
		return _hasRoad;
	}

	public uint getRevenue() {
		switch (_landType) {
			case LandType.MEADOW:
				return MEADOW_REVENUE;
			case LandType.FOREST:
				return FOREST_REVENUE;
			default:
				return LANDTYPE_REVENUE;
		}
	}

	public TileComponent(int initialOwner) {
        _initialPlayerIndex = initialOwner;
        _hasRoad = false;

        _landType = LandType.GRASS;
        _occupantType = OccupantType.NONE;
        _occupyingStructure = new StructureComponent(StructureType.NONE, this);
        _neighbours = new List<TileComponent>(6);
	}

	public List<TileComponent> breadthFS() {
		List<TileComponent> regionTiles = new List<TileComponent>();
		Queue<TileComponent> q = new Queue<TileComponent>();
		TileComponent t = this.GetComponent<TileComponent>();
		
		q.Enqueue(t);
		regionTiles.Add(t);
		while(q.Count > 0){
			t = q.Dequeue();
			foreach (var neighbour in t.getNeighbours())
			{
				if (neighbour.getInitialPlayerIndex() == this.getInitialPlayerIndex() && !regionTiles.Contains(neighbour))
				{
					q.Enqueue(neighbour);
					regionTiles.Add(neighbour);
				}            
			}
		}
		return regionTiles;
	}

	public void connectRegions() {
		foreach (TileComponent tile in _neighbours) {
			VillageComponent neighbourVillage = tile.getVillage();

			if (neighbourVillage && neighbourVillage != _village && neighbourVillage.getPlayer() == _village.getPlayer()) {
				VillageComponent strongerVillage;
				VillageComponent weakerVillage;
				VillageType neighbourVillageType = neighbourVillage.getVillageType();
				VillageType myVillageType = _village.getVillageType();

				if (neighbourVillageType > myVillageType) {
					strongerVillage = neighbourVillage;
					weakerVillage = _village;
				} else if (neighbourVillageType < myVillageType) {
					strongerVillage = _village;
					weakerVillage = neighbourVillage;
				} else {
					int neighbourRegionSize = neighbourVillage.getControlledRegion().Count;
					int myRegionSize = _village.getControlledRegion().Count;

					if (neighbourRegionSize > myRegionSize) {
						strongerVillage = neighbourVillage;
						weakerVillage = _village;
					} else {
						strongerVillage = _village;
						weakerVillage = neighbourVillage;
					}
				}

				strongerVillage.mergeWith(weakerVillage);
			}
		}
	}

	public void createRoad() {
        _hasRoad = true;
	}

	public void randomizeTile() {
		/* TODO */
	}

	/*********************
	 *   UNITY METHODS   *
	 ********************/
	
	// Use this for initialization
	void Start() {
        _terrainGameObject = (GameObject)Instantiate(_terrainGameObject, this.transform.position, Quaternion.identity);
		_terrainGameObject.transform.parent = this.transform;
	}

	void Awake(){
		_assets = GameObject.FindObjectOfType<AssetManager>();
		_menus = GameObject.FindObjectOfType<GUIManager>();
	}

	// Update is called once per frame
	void Update() {

		//Calling Select() from any other method seems to break the prefab's connection to its materials, wrecking all the colours.
		// Therefore, call the "Draw()" function to update the selection.
		if(_drawUpdated){
			Highlight();
			_drawUpdated = false;
		}
	}

	/***********************
	 *   SELECTION EVENTS  *
	 **********************/

	
	void OnMouseDown() {
		if(Network.isServer || Network.isClient){
			networkView.RPC ("ToggleColours", RPCMode.All);
		}
		else{
			Deselect();
		}
	}

	void OnMouseUp(){
		Select();
	}

	/// <summary>
	/// Trigger a draw update on the next call to Update()
	/// </summary>
	public void UpdateDraw(){
		_drawUpdated = true;
	}

	public void Select(){
		HighlightRegion();
		if(_occupantType == OccupantType.UNIT){
			Debug.Log("Unit");
			_menus.DisplayUnitActions();
		}
		else if(_occupantType == OccupantType.VILLAGE){
			Debug.Log("Village");
			_menus.DisplayVillageActions();
		}
		else{
			Debug.Log("None");
		}
	}

	public void Deselect(){
		UnhighlightRegion();
	}

	/*****************************
	 *   Colours and Highlights  *
	 ****************************/

	public void Highlight(){
		Color colour = PLAYER_COLOURS[_initialPlayerIndex];
		Color newColour = new Color(0,0,0);
		newColour.r = Mathf.Min(colour.r + 0.5f, 1.0f);
		newColour.g = Mathf.Min(colour.g + 0.5f, 1.0f);
		newColour.b = Mathf.Min(colour.b + 0.5f, 1.0f);
		_terrainGameObject.renderer.materials[2].SetColor("_Color", newColour);
	}
	
	public void Unhighlight(){
		_terrainGameObject.renderer.materials[2].SetColor("_Color", PLAYER_COLOURS[_initialPlayerIndex]);
	}

	public void HighlightNeighbours(){
		List<TileComponent> neighbours = this.getNeighbours();
		foreach(TileComponent tile in neighbours){
			tile.Highlight();
		}
	}
	
	public void UnhighlightNeighbours(){
		List<TileComponent> neighbours = this.getNeighbours();
		foreach(TileComponent tile in neighbours){
			tile.Unhighlight();
		}
	}
	
	public void HighlightRegion(){
		List<TileComponent> region = this.breadthFS();
		foreach(TileComponent tile in region){
			tile.Highlight();
		}
	}

	public void UnhighlightRegion(){
		List<TileComponent> region = this.breadthFS();
		foreach(TileComponent tile in region){
			tile.Unhighlight();
		}
	}
}
