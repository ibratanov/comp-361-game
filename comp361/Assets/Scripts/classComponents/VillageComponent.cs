using UnityEngine;
using System.Collections.Generic;

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
	List<TileComponent> _controlledRegion = new List<TileComponent>();
	List<UnitComponent> _supportingUnits;
	VillageType _villageType;

	/*
	 * TODO: Decide - may be unnecessary
	 */
	VillageType[] _upgradeCosts;

	/*********************
	 *    CONSTRUCTOR    *
	 ********************/

	public VillageComponent(uint gold, uint wood, PlayerComponent player, List<TileComponent> region, List<UnitComponent> units, VillageType villageType) {
		_goldStock = gold;
		_woodStock = wood;
		_player = player;
		_controlledRegion = region;
		_supportingUnits = units;
		_villageType = villageType;
	}

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

	public List<TileComponent> getControlledRegion() {
		return _controlledRegion;
	}

	public List<UnitComponent> getSupportingUnits() {
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


    public void addToControlledRegion(TileComponent tile)
    {
        _controlledRegion.Add(tile);
    }

	public static uint calculateCost(VillageType villageType, VillageType newLevel) {
		VillageType current = villageType;
		uint cost = 0;

		switch (villageType)
		{
			case VillageType.HOVEL:
				if (newLevel.Equals(VillageType.TOWN)) {
					cost = 8;
				} else if (newLevel.Equals(VillageType.FORT)) {
					cost = 16;
				}
				break;
			case VillageType.TOWN:
				if (newLevel.Equals(VillageType.FORT)) {
					cost = 8;
				}
				break;
		}

		return cost;
	}

	public bool upgradeVillage(VillageType newLevel) {
		if (newLevel > _villageType) {
			uint cost = calculateCost(_villageType, newLevel);

			if (_woodStock >= cost) {
				setVillageType(newLevel);
				removeWood(cost);
				return true;
			}
		}
		return false;
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
        if (_goldStock < UnitComponent.INITIAL_COST[unitType])
        {
            // TODO: insufficient resource error
            return null;
        }
        _goldStock = _goldStock - UnitComponent.INITIAL_COST[unitType];
        UnitComponent u = new UnitComponent(unitType);
        u.setVillage(this);
        bool hasSpace = false;
        for (int i = 0; i < _controlledRegion.Count; i++)
        {
            if (_controlledRegion[i].getOccupantType() == OccupantType.NONE)
            {
                hasSpace = true;
                u.associate(_controlledRegion[i]);
                break;
            }
        }
       
        if (hasSpace == false)
        {
            // TODO: no more space error
            return null;
        }
        _supportingUnits.Add(u);
        return u;
	}

	public void addGold(uint amount) {
		_goldStock += amount;
	}

	public void addWood(uint amount) {
		_woodStock += amount;
	}

	public void associate(List<TileComponent> tiles) {
		foreach (TileComponent tile in tiles) {
			associate(tile);
		}
	}

	public void associate(TileComponent tile) {
		VillageComponent formerVillage = tile.getVillage();

		if (formerVillage) {
			List<TileComponent> formerRegion = formerVillage.getControlledRegion();
			formerRegion.Remove(tile);
		}

		_controlledRegion.Add(tile);
		tile.setVillage(this);
	}

	public void associate(List<UnitComponent> units) {
		foreach (UnitComponent unit in units) {
			associate(unit);
		}
	}

	public void associate(UnitComponent unit) {
		VillageComponent formerVillage = unit.getVillage();

		if (formerVillage) {
			List<UnitComponent> units = formerVillage.getSupportingUnits();
			units.Remove(unit);
		}

		if (_supportingUnits == null) {
			_supportingUnits = new List<UnitComponent>();
		}

		_supportingUnits.Add(unit);

		unit.setVillage(this);
	}

	void killVillagers() {
		foreach (UnitComponent unit in _supportingUnits) {
			unit.die();
		}
		_supportingUnits = null;
	}

	public void mergeWith(VillageComponent village) {
		addWood(village.getWoodStock());
		addGold(village.getGoldStock());
		associate(village.getControlledRegion());
		associate(village.getSupportingUnits());
		village.getPlayer().remove(village);
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

    public VillageComponent(VillageType villageType, PlayerComponent currentPlayer)
    {
        _goldStock = 0;
        _woodStock = 0;
        _player = currentPlayer;
        _controlledRegion = new List<TileComponent>();
        _supportingUnits = new List<UnitComponent>();
        _villageType = villageType;
    }



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}