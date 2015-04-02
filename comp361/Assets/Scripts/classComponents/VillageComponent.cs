using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.classComponents;

public enum VillageType {
	HOVEL,
	TOWN,
	FORT,
    CASTLE
}

public class VillageComponent : GenericComponent
{

	private GameObject _villageGameObject;

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	uint _goldStock;
	uint _woodStock;
	PlayerComponent _player;
	List<TileComponent> _controlledRegion;
	List<UnitComponent> _supportingUnits;
	VillageType _villageType;
    TileComponent _occupyingTile;
    private GUIManager _menus;
    private int _healthLeft;

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

    public void setOccupyingTile(TileComponent tile)
    {
        _occupyingTile = tile;
    }

    public TileComponent getOccupyingTile()
    {
        return _occupyingTile;
    }

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


	public void setVillageType(VillageType villageType) {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCsetVillageType", RPCMode.Others, (int)villageType);
		}
		else{
			RPCsetVillageType((int)villageType);
		}
	}
	
	[RPC]
	private void RPCsetVillageType(int villageTypeIndex) {
		_villageType = (VillageType)villageTypeIndex;
		if(_villageGameObject){
			GameObject oldObject = _villageGameObject;
			Destroy(oldObject);
		}
		AssetManager assetManager = GameObject.FindGameObjectWithTag("AssetManager").GetComponent<AssetManager>();
		_villageGameObject = assetManager.createVillageGameObject((VillageType)villageTypeIndex, this.gameObject.transform.position);
		_villageGameObject.transform.parent = this.transform;
	}

	public VillageType getVillageType() {
		return _villageType;
	}

    public int GetHealthLeft() 
    {
        return _healthLeft;
    }

	/*********************
	 *      METHODS      *
	 ********************/
    public void DecrementHealth()
    {
        _healthLeft -= _healthLeft;
    }

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
                else if (newLevel.Equals(VillageType.CASTLE))
                {
                    cost = 28;
                }
				break;
			case VillageType.TOWN:
				if (newLevel.Equals(VillageType.FORT)) {
					cost = 8;
				}
                else if (newLevel.Equals(VillageType.CASTLE))
                {
                    cost = 20;
                }
				break;
            case VillageType.FORT:
                if (newLevel.Equals(VillageType.CASTLE))
                {
                    cost = 12;
                }
                break;
		}

		return cost;
	}

	public bool upgradeVillage() {

        VillageType newLevel = (VillageType)_villageType + 1;
        if ((int)newLevel > (int) VillageType.CASTLE)
        {
            ThrowError("Error: Village already at max level.");
            return false;
        }
        uint cost = calculateCost(_villageType, newLevel);
        if (_woodStock >= cost)
        {
            setVillageType(newLevel);
            removeWood(cost);
            return true;
        }
        else
        {
            ThrowError("Insufficient gold.");
            return false;
        }	
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

	public void hireVillager(UnitType unitType) {
		if (_goldStock < UnitComponent.INITIAL_COST[unitType])
		{
            ThrowError("Insufficient gold.");
            return;
		}
        if (unitType == UnitType.CANNON)
        {
            if (_woodStock < 12)
            {
                ThrowError("Insufficient wood.");
                return;
            }
        }
		_goldStock = _goldStock - UnitComponent.INITIAL_COST[unitType];
		bool hasSpace = false;
		for (int i = 0; i < _controlledRegion.Count; i++)
		{
			if (_controlledRegion[i].getOccupantType() == OccupantType.NONE)
			{
				//if (unitType == UnitType.KNIGHT && _controlledRegion[i].getLandType() != LandType.MEADOW)
				//{
				hasSpace = true;
				UnitComponent unit = CreateUnit(_controlledRegion[i], unitType);
				_supportingUnits.Add(unit);
				break;
				//}
			}
		}
		
		if (hasSpace == false)
		{
            ThrowError("Not enough space to hire new villager.");
            return;
		}
	}

	/// <summary>
	/// Creates a Unit of a given type.
	/// </summary>
	/// <returns>The unit.</returns>
	/// <param name="tc">The tileComponent which will own this unit.</param>
	/// <param name="uType">The type of unit to create</param>
	public UnitComponent CreateUnit(TileComponent tc, UnitType uType){
		UnitComponent uc = tc.gameObject.AddComponent<UnitComponent>();
		uc.Initialize(uType);
		return uc;
	}

	public void addGold(uint amount) {
		_goldStock += amount;
		_menus.setGoldStock((int)_goldStock);
	}

	public void addWood(uint amount) {
		_woodStock += amount;
        _menus.setWoodStock((int)_woodStock);
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
		tile.UpdateVillageReference();
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
        _menus.setWoodStock((int)_woodStock);
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

	public void Initialize(VillageType vType, PlayerComponent currentPlayer)
	{
		_goldStock = 0;
		_woodStock = 0;
		_player = currentPlayer;
		_villageType = vType;
		_controlledRegion = new List<TileComponent>();
		_supportingUnits = new List<UnitComponent>();
		_occupyingTile = null;
		_menus = GameObject.FindObjectOfType<GUIManager>();
		setVillageGameObject(vType);
        switch (vType)
        {
            case VillageType.CASTLE:
                _healthLeft = 10;
                break;
            case VillageType.FORT:
                _healthLeft = 5;
                break;
            case VillageType.HOVEL:
                _healthLeft = 1;
                break;
            case VillageType.TOWN:
                _healthLeft = 2;
                break;
        }
	}

	public void setVillageGameObject(VillageType vType)
	{
		if(_villageGameObject){
			GameObject oldVillage = _villageGameObject;
			Destroy(oldVillage);
		}
		AssetManager assetManager = GameObject.FindGameObjectWithTag("AssetManager").GetComponent<AssetManager>();
		GameObject village = assetManager.createVillageGameObject(vType, this.gameObject.transform.position);//(GameObject)Instantiate( assetManager.getVillageGameObject(vType), this.gameObject.transform.position, Quaternion.identity);
		_villageGameObject = village;
		village.transform.parent = this.gameObject.transform;
	}

	public GameObject getVillageGameObject()
	{
		return _villageGameObject;
	}
	
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}