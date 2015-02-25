using UnityEngine;
using System.Collections;

public enum VillageType {
	HOVEL,
	TOWN,
	FORT
}

public class VillageComponent : MonoBehaviour {

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	int _goldStock;
	int _woodStock;
	PlayerComponent _player;
	TileComponent[] _controlledRegion;
	UnitComponent[] _supportingUnits;
	VillageType _villageType;
	VillageType[] _upgradeCosts;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

	int getGoldStock() {
		return _goldStock;
	}

	int getWoodStock() {
		return _woodStock;
	}

	int getWages() {
		/* TODO */
		return 0;
	}

	PlayerComponent getPlayer() {
		return _player;
	}

	void setPlayer(PlayerComponent player) {
		_player = player;
	}

	TileComponent[] getControlledRegion() {
		return _controlledRegion;
	}

	UnitComponent[] getSupportingUnits() {
		return _supportingUnits;
	}

	VillageType getVillageType() {
		return _villageType;
	}

	void setVillageType(VillageType villageType) {
		_villageType = villageType;
	}

	/*********************
	 *      METHODS      *
	 ********************/

	bool payWages() {
		/* TODO */
		return true;
	}

	int calculateCost(VillageType villageType, VillageType newLevel) {
		/* TODO */
		return 0;
	}

	UnitComponent hireVillager(UnitType unitType) {
		/* TODO */
		return null;
	}

	void addGold(int amount) {
		_goldStock += amount;
	}

	void addWood(int amount) {
		_woodStock += amount;
	}

	void associate(TileComponent tile) {
		/* TODO */
	}

	void associate(UnitComponent unit) {
		/* TODO */
	}

	void killVillagers() {
		/* TODO */
	}

	void mergeWith(VillageComponent village) {
		/* TODO */
	}

	void produceMeadows() {
		/* TODO */
	}

	void produceRoads() {
		/* TODO */
	}

	void removeGold(int amount) {
		/* TODO */
		_goldStock -= amount;
	}

	void removeWood(int amount) {
		/* TODO */
		_woodStock -= amount;
	}

	void replaceTombstonesByForest() {
		/* TODO */
	}

	void resetGold() {
		/* TODO */
	}

	void updateGoldStock() {
		/* TODO */
	}



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}