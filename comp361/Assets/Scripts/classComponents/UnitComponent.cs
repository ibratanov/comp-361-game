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
	public int getUpkeep() {
		return _upkeep;
	}

	public int getRoundsCultivating() {
		return _roundsCultivating;
	}

	public ActionType getCurrentAction() {
		return _currentAction;
	}

	public void setCurrentAction(ActionType currentAction) {
		_currentAction = currentAction;
	}

	public TileComponent getLocation() {
		return _location;
	}

	public void setLocation(TileComponent location) {
		_location = location;
	}

	public UnitType getUnitType() {
		return _unitType;
	}

	public void setUnitType(UnitType unitType) {
		_unitType = unitType;
	}

	public VillageComponent getVillage() {
		return _village;
	}

	public void setVillage(VillageComponent village) {
		_village = village;
	}


	/*********************
	 *      METHODS      *
	 ********************/

	public static int calculateCost(UnitType u1, UnitType u2) {
		/* TODO */
		return 0;
	}

	public bool isContested(TileComponent destination) {
		/* TODO */
		return true;
	}

	public bool upgradeUnit(UnitType newLevel) {
		/* TODO */
		return true;
	}

	public void associate(TileComponent tile) {
		/* TODO */
	}

	public void buildRoad() {
		/* TODO */
	}

	public void Create(UnitType unitType) {
		/* TODO */
	}

	public void cultivate() {
		/* TODO */
	}

	public void die() {
		/* TODO */
	}

	public void moveUnit(TileComponent destination) {
		/* TODO */
	}

	void resetRoundsCultivating() {
		/* TODO */
	}

	public void takeOverTile(TileComponent destination) {
		/* TODO */
	}



	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}
}
