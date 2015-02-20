using UnityEngine;
using System.Collections;

enum LandType { 
	GRASS,
	MEADOW,
	FOREST,
	VILLAGE,
	SEA
}

enum OccupantType {
	NONE,
	UNIT,
	VILLAGE,
	STRUCTURE
}

public class TileComponent : MonoBehaviour {

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
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	bool hasRoad() {
		return _hasRoad;
	}

	int getInitialPlayerIndex() {
		return _initialPlayerIndex;
	}

	void setInitialPlayerIndex(int initialPlayerIndex) {
		_initialPlayerIndex = initialPlayerIndex;
	}

	LandType getLandType() {
		return _landType;
	}

	void setLandType(LandType landType) {
		_landType = landType;
	}

	OccupantType getOccupantType() {
		return _occupantType;
	}

	void setOccupantType(OccupantType occupantType) {
		_occupantType = occupantType;
	}

	StructureComponent getOccupyingStructure() {
		return _occupyingStructure;
	}

	void setOccupyingStructure(StructureComponent occupyingStructure) {
		_occupyingStructure = occupyingStructure;
	}

	UnitComponent getOccupyingUnit() {
		return _occupyingUnit;
	}

	void setOccupyingUnit(UnitComponent occupyingUnit) {
		_occupyingUnit = occupyingUnit;
	}

	VillageComponent getVillage() {
		return _village;
	}

	void setVillage(VillageComponent village) {
		_village = village;
	}

	TileComponent[] getNeighbours() {
		return _neighbours;
	}

	int getRevenue() {
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
