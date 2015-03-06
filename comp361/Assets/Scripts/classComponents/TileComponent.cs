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
	public GameObject _terrainGameObject;

	readonly static int MEADOW_REVENUE = 2;
	readonly static int FOREST_REVENUE = 0;
	readonly static int LANDTYPE_REVENUE = 1;

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

	public LandType getLandType() {
		return _landType;
	}

	public void setLandType(LandType landType) {
		_landType = landType;
	}

	public OccupantType getOccupantType() {
		return _occupantType;
	}

	public void setOccupantType(OccupantType occupantType) {
		_occupantType = occupantType;
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

	public VillageComponent getVillage() {
		return _village;
	}

	public void setVillage(VillageComponent village) {
		_village = village;
	}

	public TileComponent[] getNeighbours() {
        
		return _neighbours;
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

	public void setGameObject(AssetManager assets, LandType landType){
		_terrainGameObject = assets.getTerrainGameObject(landType);
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
        Stack<TileComponent> s = new Stack<TileComponent>();
        foreach (var neighbour in this.getNeighbours())
        {
            if (neighbour._initialPlayerIndex == this._initialPlayerIndex)
            {
                s.Push(neighbour);
                regionTiles.Add(neighbour);
            }            
        }
        while (s.Count > 0)
        {
            var t = s.Pop();
            foreach (var n in t.getNeighbours())
            {
                if (n._initialPlayerIndex == t._initialPlayerIndex)
                {
                    s.Push(n);
                    regionTiles.Add(n);
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

	// Update is called once per frame
	void Update() {

	}

	void OnMouseDown() {
		if(Network.isServer || Network.isClient){
			networkView.RPC ("ToggleColours", RPCMode.All);
		}
		else{
			ToggleColours ();
		}
	}
	
	[RPC]
	void ToggleColours(){

		if( _terrainGameObject.renderer.materials[2].GetColor("_Color") == Color.red){
			_terrainGameObject.renderer.materials[2].SetColor("_Color", Color.white);
		}
		else{
			_terrainGameObject.renderer.materials[2].SetColor("_Color", Color.red);
		}
		
		Debug.Log("changing colors");
	}
}
