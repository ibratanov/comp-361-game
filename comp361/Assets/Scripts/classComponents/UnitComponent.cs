using UnityEngine;
using System.Collections.Generic;

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

	readonly static int COST_PER_UNIT_UPGRADE = 10;

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

	public bool setLocation(TileComponent location) {
		_location = location;
		return true;
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
			if (u1 > u2) {
				throw new System.Exception("The first parameter cannot be smaller than the second.");
			}

			int cost = 0;

			for (UnitType u = u1; u != u2; ++u) {
				cost += COST_PER_UNIT_UPGRADE;
			}

			return cost;
	}

	public bool isContested(TileComponent destination) {
		List<TileComponent> neighbours = _location.getNeighbours();
		neighbours.Add(destination);

		foreach (TileComponent tile in neighbours) {
			UnitComponent enemyUnit = tile.getOccupyingUnit();

			if (enemyUnit) {
				VillageComponent enemyVillage = enemyUnit.getVillage();

				if (enemyVillage != _village) {
					UnitType enemyUnitType = enemyUnit.getUnitType();

					if (_unitType <= enemyUnitType) {
						return true;
					}
				}
			} else {
				StructureComponent enemyStructure = tile.getOccupyingStructure();

				if (enemyStructure) {
					StructureType enemyStructureType = enemyStructure.getStructureType();

					if (enemyStructureType == StructureType.WATCHTOWER) {
						VillageComponent enemyVillage = tile.getVillage();

						if (_unitType <= UnitType.INFANTRY || _village != enemyVillage) {
							return true;
						}
					}
				}
			}
		}

		return false;
	}

	public bool upgradeUnit(UnitType newLevel) {
		if (newLevel >= _unitType) {
			int cost = calculateCost(_unitType, newLevel);

			if (_village.getGoldStock() >= cost) {
				_unitType = newLevel;
				_village.removeGold(cost);
				return true;
			}
		}

		return false;
	}

	public void associate(TileComponent tile) {
		_location.setOccupantType(OccupantType.NONE);
		_location.setOccupyingUnit(null);

		tile.setOccupantType(OccupantType.UNIT);
		tile.setOccupyingUnit(this);

		setLocation(tile);
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

	public UnitComponent(UnitType unitType) {
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
		if (_currentAction == ActionType.READY_FOR_ORDERS) {
			List<TileComponent> neighbours = destination.getNeighbours();

			bool isReachable = false;

			foreach (TileComponent tile in neighbours) {
				if (tile == _location) {
					isReachable = true;
					break;
				}
			}

			if (isReachable) {
				bool isRelocated = setLocation(destination);

				if (isRelocated) {
					LandType landType = destination.getLandType();
					bool isPaved = destination.hasRoad();

					if (landType == LandType.MEADOW && _unitType >= UnitType.SOLDIER && !isPaved) {
						destination.setLandType(LandType.GRASS);
					}

					destination.connectRegions();
				}
			}
		}
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
