﻿using UnityEngine;
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

	uint _goldStock;
	uint _woodStock;
	PlayerComponent _player;
	TileComponent[] _controlledRegion;
	UnitComponent[] _supportingUnits;
	VillageType _villageType;
	VillageType[] _upgradeCosts;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

	public uint getGoldStock() {
		return _goldStock;
	}

	public uint getWoodStock() {
		return _woodStock;
	}

	uint getWages() {
		uint wages = 0;

		foreach (UnitComponent unit in _supportingUnits) {
			wages += unit.getUpkeep();
		}

		return wages;
	}

	public PlayerComponent getPlayer() {
		return _player;
	}

	public void setPlayer(PlayerComponent player) {
		_player = player;
	}

	public TileComponent[] getControlledRegion() {
		return _controlledRegion;
	}

	public UnitComponent[] getSupportingUnits() {
		return _supportingUnits;
	}

	public VillageType getVillageType() {
		return _villageType;
	}

	public void setVillageType(VillageType villageType) {
		_villageType = villageType;
	}

	/*********************
	 *      METHODS      *
	 ********************/

	public static int calculateCost(VillageType villageType, VillageType newLevel) {
		/* TODO */
		return 0;
	}

	public bool payWages() {
		uint wages = getWages();

		if (wages <= getGoldStock()) {
			removeGold(wages);
			return true;
		}

		killVillagers();
		resetGold();
		return false;
	}

	public UnitComponent hireVillager(UnitType unitType) {
        /* TODO */
		return null;
	}

	public void addGold(uint amount) {
		_goldStock += amount;
	}

	public void addWood(uint amount) {
		_woodStock += amount;
	}

	public void associate(TileComponent tile) {
		/* TODO */
	}

	public void associate(UnitComponent unit) {
		/* TODO */
	}

	void killVillagers() {
		foreach (UnitComponent unit in _supportingUnits) {
			unit.die();
		}
		_supportingUnits = null;
	}

	public void mergeWith(VillageComponent village) {
		/* TODO */
	}

	public void produceMeadows() {
		foreach (UnitComponent unit in _supportingUnits) {
			ActionType currentAction = unit.getCurrentAction();

			if (currentAction == ActionType.CULTIVATING_MEADOW) {
				unit.cultivate();
			}
		}
	}

	public void produceRoads() {
		foreach (UnitComponent unit in _supportingUnits) {
			ActionType currentAction = unit.getCurrentAction();

			if (currentAction == ActionType.BUILDING_ROAD) {
				unit.buildRoad();
			}
		}
	}

	public void removeGold(uint amount) {
		_goldStock -= amount;
	}

	public void removeWood(uint amount) {
		_woodStock -= amount;
	}

	public void replaceTombstonesByForest() {
		foreach (TileComponent tile in _controlledRegion) {
			StructureComponent occupyingStructure = tile.getOccupyingStructure();
			StructureType structureType = occupyingStructure.getStructureType();

			if (structureType == StructureType.TOMBSTONE) {
				occupyingStructure.setStructureType(StructureType.NONE);
				tile.setLandType(LandType.FOREST);
			}
		}
	}

	void resetGold() {
		_goldStock = 0;
	}

	public void updateGoldStock() {
		foreach (TileComponent tile in _controlledRegion) {
			uint revenue = tile.getRevenue();
			addGold(revenue);
		}
	}

    public VillageComponent(VillageType vt)
    {
        /* TODO */
    }



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}