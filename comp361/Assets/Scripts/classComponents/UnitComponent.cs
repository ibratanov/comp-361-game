using UnityEngine;
using System.Collections;

public enum ActionType {
	READY_FOR_ORDERS,
	GATHERING_WOOD,
	CLEARING_TOMBSTONE,
	CULTIVATING_MEADOW,
	BUILDING_ROAD,
	ATTACKING,
	EXPANDING_REGION
}

public enum UnitType {
	PEASANT,
	INFANTRY,
	SOLDIER,
	KNIGHT
}

public class UnitComponent : MonoBehaviour {

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	static UnitType[] _upgradeCosts;

	int _roundsCultivating;
	int _upkeep;
	ActionType _currentAction;
	TileComponent _location;
	UnitType _unitType;
	VillageComponent _village;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/
	int getUpkeep() {
		return _upkeep;
	}

	int getRoundsCultivating() {
		return _roundsCultivating;
	}

	ActionType getCurrentAction() {
		return _currentAction;
	}

	void setCurrentAction(ActionType currentAction) {
		_currentAction = currentAction;
	}

	TileComponent getLocation() {
		return _location;
	}

	void setLocation(TileComponent location) {
		_location = location;
	}

	UnitType getUnitType() {
		return _unitType;
	}

	void setUnitType(UnitType unitType) {
		_unitType = unitType;
	}

	VillageComponent getVillage() {
		return _village;
	}

	void setVillage(VillageComponent village) {
		_village = village;
	}


	/*********************
	 *      METHODS      *
	 ********************/

	bool isContested(TileComponent destination) {
		/* TODO */
		return true;
	}

	bool upgradeUnit(UnitType newLevel) {
		/* TODO */
		return true;
	}

	int calculateCost(UnitType u1, UnitType u2) {
		/* TODO */
		return 0;
	}

	void associate(TileComponent tile) {
		/* TODO */
	}

	void buildRoad() {
		/* TODO */
	}

	void Create(UnitType unitType) {
		/* TODO */
	}

	void cultivate() {
		/* TODO */
	}

	void die() {
		/* TODO */
	}

	void moveUnit(TileComponent destination) {
		/* TODO */
	}

	void resetRoundsCultivating() {
		/* TODO */
	}

	void takeOverTile(TileComponent destination) {
		/* TODO */
	}



	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}
}
