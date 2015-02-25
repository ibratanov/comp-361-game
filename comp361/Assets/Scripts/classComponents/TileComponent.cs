using UnityEngine;
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

	bool _hasRoad;
	int _initialPlayerIndex;

	GameComponent _game;
	LandType _landType;
	OccupantType _occupantType;
	StructureComponent _occupyingStructure;
	TileComponent[] _neighbours;
	UnitComponent _occupyingUnit;
	VillageComponent _village;


	// Use this for initialization
	void Start () {
		_landType = LandType.GRASS;
		_occupantType = OccupantType.NONE;
		_occupyingUnit = null;
		_occupyingStructure = null;
		_village = null;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setGameObject(GameObject gameObject){
		if(_terrainGameObject){
			Destroy(_terrainGameObject);
		}
		_terrainGameObject = gameObject;
	}

	public GameObject getGameObject(){
		return _terrainGameObject;
	}

	public bool hasRoad() {
		return _hasRoad;
	}

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



//+ Create(int initialOwner): Tile

//+ createRoad(): void

//+ randomizeTile(): void
//+ breadthFS(): CollectionOfTiles
//+ connectRegions(): void
}
