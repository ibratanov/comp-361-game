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
        _healthLeft = getTotalHealthByType(villageType);
	}
	
	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/
	
    public int getTotalHealthByType(VillageType type)
    {
        switch (type)
        {
            case VillageType.HOVEL:
                return 1;
            case VillageType.TOWN:
                return 2;
            case VillageType.FORT:
                return 5;
            case VillageType.CASTLE:
                return 10;
        }
        return -1;
    }

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
        _healthLeft = getTotalHealthByType(_villageType);
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
		_healthLeft = _healthLeft - 1;
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
            ThrowError("Error: The village is already at the maximium level.");
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
			ThrowError("Insufficient wood.");
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
		_menus.setGoldStock((int)_goldStock);
		bool hasSpace = false;
		for (int i = 0; i < _controlledRegion.Count; i++)
		{
			if (_controlledRegion[i].getOccupantType() == OccupantType.NONE)
			{
				hasSpace = true;
				CreateUnit(i, unitType);
				/*
				_supportingUnits.Add(unit);

                if (unitType != UnitType.PEASANT && unitType != UnitType.SOLDIER)
                {
                    unit.TrampleMeadow(unit.getLocation());
                }
                if (unitType != UnitType.KNIGHT && unitType != UnitType.CANNON && _controlledRegion[i].getLandType() == LandType.FOREST)
                {
                    unit.GatherWood(unit.getLocation());
                }
                */
				break;
			}
		}
		
		if (hasSpace == false)
		{
            ThrowError("Insufficient space to spawn villager.");
            return;
		}
	}
	
	/// <summary>
	/// Creates a Unit of a given type.
	/// </summary>
	/// <returns>The unit.</returns>
	/// <param name="tc">The tileComponent which will own this unit.</param>
	/// <param name="uType">The type of unit to create</param>
	public void CreateUnit(int regionIndex, UnitType uType){
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCCreateUnit", RPCMode.Others, regionIndex, (int)uType);
		}
		RPCCreateUnit(regionIndex, (int)uType);
	}

	[RPC]
	public void RPCCreateUnit(int regionIndex, int uTypeIndex){
		UnitType uType = (UnitType)uTypeIndex;
		TileComponent tc = _controlledRegion[regionIndex];
		UnitComponent uc = tc.gameObject.AddComponent<UnitComponent>();
		uc.Initialize(uType);
		tc.setOccupantType(OccupantType.UNIT);
		tc.setOccupyingUnit(uc);

		_supportingUnits.Add(uc);
		
		if (uType != UnitType.PEASANT && uType != UnitType.SOLDIER)
		{
			uc.TrampleMeadow(uc.getLocation());
		}
		if (uType != UnitType.KNIGHT && uType != UnitType.CANNON && _controlledRegion[regionIndex].getLandType() == LandType.FOREST)
		{
			uc.GatherWood(uc.getLocation());
		}

		tc.Highlight();
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
		_supportingUnits = new List<UnitComponent>();
	}
	
	public void mergeWith(VillageComponent village) {
		addWood(village.getWoodStock());
		addGold(village.getGoldStock());
		associate(village.getControlledRegion());
		associate(village.getSupportingUnits());
		village.getPlayer().remove(village);
	}
	
	public void produceMeadows() {
            foreach (UnitComponent unit in _supportingUnits)
            {
                ActionType currentAction = unit.getCurrentAction();

                if (currentAction == ActionType.CULTIVATING_MEADOW)
                {
                    unit.cultivate();
                }
            }

	}
	
	public void produceRoads() {

            foreach (UnitComponent unit in _supportingUnits)
            {
                ActionType currentAction = unit.getCurrentAction();

                if (currentAction == ActionType.BUILDING_ROAD)
                {
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
	
	public void Initialize(VillageType vType, PlayerComponent player)
	{
		_goldStock = 0;
		_woodStock = 0;
		_player = player;
		_player.addVillage(this);
		_villageType = vType;
		_controlledRegion = new List<TileComponent>();
		_supportingUnits = new List<UnitComponent>();
		_occupyingTile = null;
		_menus = GameObject.FindObjectOfType<GUIManager>();
		setVillageGameObject(vType);
        _healthLeft = getTotalHealthByType(vType);
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

    public void DestroyVillage()
    {
        // call die() on all units
        foreach (var unit in _supportingUnits)
        {
            unit.die();
        }

        // clear supporting units list
        _supportingUnits = new List<UnitComponent>();
        
        // set all tiles to neutral
        foreach (var tile in _controlledRegion)
        {
            tile.setOccupantType(OccupantType.NONE);
            tile.setOccupyingUnit(null);
            tile.setOccupyingStructure(null);
            tile.setPlayerIndex(0);
            tile.RemoveRoad();
        }
        // destroy village object
        GameObject.Destroy(_villageGameObject);
    }
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}