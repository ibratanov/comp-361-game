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

	public GameObject _terrainGameObject;
	public GameObject _villageGameObject;
	private AssetManager _assets;
	private GUIManager _menus;

	readonly static int MEADOW_REVENUE = 2;
	readonly static int FOREST_REVENUE = 0;
	readonly static int LANDTYPE_REVENUE = 1;

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
	TileComponent[] _neighbours;
	UnitComponent _occupyingUnit;
	VillageComponent _village;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

	public int getInitialPlayerIndex() {
		return _initialPlayerIndex;
	}

	public void setInitialPlayerIndex(int initialPlayerIndex) {
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
		_village = village;
	}

	public void setVillage(VillageComponent village, bool isOccupying) {
		_village = village;
		if(isOccupying){
			_landType = LandType.GRASS;
			_terrainGameObject = _assets.getVillageGameObject(village.getVillageType());
		}
	}

	public VillageComponent getVillage() {
		return _village;
	}

	public OccupantType getOccupantType() {
		return _occupantType;
	}

	public void setOccupantType(OccupantType occupantType) {
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

	public StructureComponent getOccupyingStructure() {
		return _occupyingStructure;
	}

	public void setOccupyingStructure(StructureComponent occupyingStructure) {
		_occupyingStructure = occupyingStructure;
	}

	public UnitComponent getOccupyingUnit() {
		return _occupyingUnit;
	}

	public void setOccupyingUnit(UnitComponent occupyingUnit) {
		_occupyingUnit = occupyingUnit;
	}

    public TileComponent[] getNeighbours()
    {
        return _neighbours;
    }

    public void setNeighbours(TileComponent[] neighbours)
    {
        _neighbours = neighbours;
    }

	/*********************
	 *      METHODS      *
	 ********************/

	public bool hasRoad() {
		return _hasRoad;
	}

	public int getRevenue() {
		switch (_landType) {
			case LandType.MEADOW:
				return MEADOW_REVENUE;
			case LandType.FOREST:
				return FOREST_REVENUE;
			default:
				return LANDTYPE_REVENUE;
		}
	}

	public GameObject getGameObject() {
		return _terrainGameObject;
	}

	public TileComponent(int initialOwner) {
        _initialPlayerIndex = initialOwner;
        _hasRoad = false;

        _landType = LandType.GRASS;
        _occupantType = OccupantType.NONE;
        _occupyingStructure = new StructureComponent(StructureType.NONE, this);
        _neighbours = new TileComponent[6];
	}

	public TileComponent[] breadthFS() {
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
		return regionTiles.ToArray();
	}

	public void connectRegions() {
		/* TODO */
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
		_landType = LandType.GRASS;
		_occupantType = OccupantType.NONE;
		_occupyingUnit = null;
		_occupyingStructure = null;
		_village = null;
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
		Debug.Log("None");
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
		TileComponent[] neighbours = this.getNeighbours();
		foreach(TileComponent tile in neighbours){
			tile.Highlight();
		}
	}
	
	public void UnhighlightNeighbours(){
		TileComponent[] neighbours = this.getNeighbours();
		foreach(TileComponent tile in neighbours){
			tile.Unhighlight();
		}
	}
	
	public void HighlightRegion(){
		TileComponent[] region = this.breadthFS();
		foreach(TileComponent tile in region){
			tile.Highlight();
		}
	}

	public void UnhighlightRegion(){
		TileComponent[] region = this.breadthFS();
		foreach(TileComponent tile in region){
			tile.Unhighlight();
		}
	}
}
