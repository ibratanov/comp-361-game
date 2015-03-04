﻿using UnityEngine;
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
	public GameObject _villagerGameObject;

	readonly static uint COST_PER_UNIT_UPGRADE = 10;

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	static UnitType[] _upgradeCosts;

	int _roundsCultivating;
	uint _upkeep;
	ActionType _currentAction;
	TileComponent _location;
	UnitType _unitType;
	VillageComponent _village;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/
	public uint getUpkeep() {
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

	public static uint calculateCost(UnitType u1, UnitType u2) {
			if (u1 > u2) {
				throw new System.Exception("The first parameter cannot be smaller than the second.");
			}

			uint cost = 0;

			for (UnitType u = u1; u != u2; ++u) {
				cost += COST_PER_UNIT_UPGRADE;
			}

			return cost;
	}

	public bool isContested(TileComponent destination) {
		/* TODO */
		return true;
	}

	public bool upgradeUnit(UnitType newLevel) {
		if (newLevel >= _unitType) {
			uint cost = calculateCost(_unitType, newLevel);

			if (_village.getGoldStock() >= cost) {
				_unitType = newLevel;
				_village.removeGold(cost);
				return true;
			}
		}

		return false;
	}

	public void associate(TileComponent tile) {
		/* TODO */
	}

	public void buildRoad() {
        var ut = getUnitType();
        if (ut == UnitType.PEASANT)
        {
            var t = getLocation();
            var b = t.hasRoad();
            if (b == false)
            {
                if (t.getLandType() == LandType.MEADOW || t.getLandType() == LandType.GRASS)
                {
                    t.createRoad();
                }
            }
        }
	}

	public void Create(UnitType unitType) {
		/* TODO */
	}

	public void cultivate() {
		if (_roundsCultivating >= 2) {
			_location.setLandType(LandType.MEADOW);
			_currentAction = ActionType.READY_FOR_ORDERS;
			_roundsCultivating = 0;
		} else {
			++_roundsCultivating;
		}
	}

	public void die() {
		StructureComponent tombstone = new StructureComponent(StructureType.TOMBSTONE, _location);
		_location.setOccupyingStructure(tombstone);
	}

	public void moveUnit(TileComponent destination) {
		/* TODO */
	}

	public void takeOverTile(TileComponent destination) {
		/* TODO */
	}

	public GameObject getGameObject() {
		return _villagerGameObject;
	}
	
	public void setGameObject(AssetManager assets, UnitType unitType){
		_villagerGameObject = assets.getUnitGameObject(unitType);
	}

	/*********************
	 *   UNITY METHODS   *
	 ********************/

	// Use this for initialization
	void Start() {
		_roundsCultivating = 0;
		_currentAction = ActionType.READY_FOR_ORDERS;
	}

	// Update is called once per frame
	void Update() {

	}
}
