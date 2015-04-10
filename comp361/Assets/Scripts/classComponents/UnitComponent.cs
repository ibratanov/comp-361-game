using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.classComponents; //For copying components

public enum ActionType
{
    READY_FOR_ORDERS,
    GATHERING_WOOD,
    CLEARING_TOMBSTONE,
    CULTIVATING_MEADOW,
    BUILDING_ROAD,
    ATTACKING,
    EXPANDING_REGION,
    ALREADY_MOVED
}

public enum UnitType
{
    PEASANT,
    INFANTRY,
    SOLDIER,
    KNIGHT,
    CANNON
}

public class UnitComponent : GenericComponent
{
    public GameObject _unitGameObject;
	private bool _isMoving;

    readonly static uint COST_PER_UNIT_UPGRADE = 10;
    public readonly static Dictionary<UnitType, uint> UPKEEP = new Dictionary<UnitType, uint>()
    {
        {UnitType.PEASANT, 2},
        {UnitType.INFANTRY, 6},
        {UnitType.SOLDIER, 18},
        {UnitType.KNIGHT, 54},
        {UnitType.CANNON, 5}
    };

    public readonly static Dictionary<UnitType, uint> INITIAL_COST = new Dictionary<UnitType, uint>()
    {
        {UnitType.PEASANT, 10},
        {UnitType.INFANTRY, 20},
        {UnitType.SOLDIER, 30},
        {UnitType.KNIGHT, 40},
        {UnitType.CANNON, 35}
    };

    /*********************
     *     ATTRIBUTES    *
     ********************/

    static UnitType[] _upgradeCosts;

    uint _roundsCultivating;
    uint _upkeep;
    ActionType _currentAction;
    UnitType _unitType;
    VillageComponent _village;

	readonly public static Color[] PLAYER_COLOURS = 
	{
		Color.white,
		Color.red,
		Color.green,
		Color.blue,
		Color.cyan
	};
	readonly public static Color[] PLAYER_COLOURS_DARK = 
	{
		Color.white,
		Color.red,
		Color.green,
		Color.blue,
		Color.cyan
	};

    /*********************
     *  GETTERS/SETTERS  *
     ********************/
    public uint getUpkeep()
    {
        return _upkeep;
    }

    public uint getRoundsCultivating()
    {
        return _roundsCultivating;
    }

	public void setRoundsCultivating(uint rounds)
	{
		_roundsCultivating = rounds;
	}


    public ActionType getCurrentAction()
    {
        return _currentAction;
    }

    public void setCurrentAction(ActionType currentAction)
    {
        _currentAction = currentAction;
    }

    public TileComponent getLocation()
    {
        return this.GetComponent<TileComponent>();
    }

	//Updates the Unit Component on the destination tile and removes it from the current tile
    public bool setLocation(TileComponent destination)
    {
		if(this.gameObject.transform.position != destination.gameObject.transform.position){
			if(destination.GetComponent<UnitComponent>() == null){
				destination.gameObject.AddComponent<UnitComponent>();
			}
			UnitComponent destComponent = destination.GetComponent<UnitComponent>();

			//Make sure the old Unit is removed and the new copy is added to the list of supporting units
			List<UnitComponent> units = _village.getSupportingUnits();
			units.Remove(this);
			units.Add(destComponent);

			//Copy all the info into the destination tile
			destComponent.setCurrentAction( this.getCurrentAction() );
			destComponent.setUnitType( this.getUnitType(), false );
			destComponent.setVillage( this.getVillage() );
			destComponent.setGameObject( this.getGameObject() );

			destination.setOccupantType(OccupantType.UNIT);
			//destination.setOccupyingUnit(this);

			//Remove the association from the previous tile
			Destroy(this.gameObject.GetComponent<UnitComponent>());
			this.gameObject.GetComponent<TileComponent>().setOccupantType(OccupantType.NONE);

			//Update the visible object's location
			//_unitGameObject.transform.position = destination.gameObject.transform.position; //Use this only if you want them to snap to the new position
			_isMoving = true;

			//Keep the UnitComponent selected for further actions
			GameComponent gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameComponent>();
            gameManager.setLastSelectedUnit(destComponent.GetComponent<UnitComponent>());

			destination.UpdateDraw();
		}

        return true;
    }

    public bool setDestination(TileComponent dest)
    {
        /**
         * Implementation of setLocation Seq. Diagr.
         * setLocation above is actually a setter.
         */
        bool shouldMerge = false;

            OccupantType occType = dest.getOccupantType();
            LandType lType = dest.getLandType();
            VillageComponent destVillage = dest.getVillage();

			bool successfullyMoved = false;
            switch (occType)
            { 
                case OccupantType.NONE:
                    if (lType == LandType.SEA || (_unitType == UnitType.KNIGHT && lType == LandType.FOREST))
                    {
						return false;
                    }
                    else if (destVillage == null)
                    {
                        //_village.associate(dest);
						dest.setVillage(_village); //temp - test out
						setCurrentAction(ActionType.EXPANDING_REGION);
						setLocation(dest);
						dest.setPlayerIndex(this.GetComponent<TileComponent>().getPlayerIndex());
						successfullyMoved = true;
                        shouldMerge = true;
					}
                    // destination is an empty tile in different player's village
					else if (destVillage != null && destVillage != _village)
                    {
                        if (_unitType == UnitType.PEASANT || _unitType == UnitType.CANNON)
                        {
							return false;
                        } 
                        //takeOverTile(dest);
                        takeOverTile(dest);
                        setCurrentAction(ActionType.EXPANDING_REGION);
						successfullyMoved = true;
                        shouldMerge = false;
					}
                    // destination is an empty tile in the same village as the unit currently belongs to
					else if (destVillage != null && destVillage == _village)
                    {
						setLocation(dest);
						successfullyMoved = true;
                        shouldMerge = true;
					}
					break;
                case OccupantType.UNIT:
                    if (destVillage == _village)
                    {
                        return false;
                    }
                    takeOverTile(dest);
                    setCurrentAction(ActionType.EXPANDING_REGION);
					successfullyMoved = true;
                    shouldMerge = false;
                    //UnitComponent destUnit = dest.getOccupyingUnit();
                    //destUnit.die();
                    //takeOverTile(dest);
                    //setCurrentAction(ActionType.ATTACKING);
                    //successfullyMoved = true;
					break;
				case OccupantType.VILLAGE:
                    // destination is the village tile of another player's village
                    if (destVillage == _village || _unitType <= UnitType.INFANTRY || _unitType == UnitType.CANNON)
                    { //TODO: check comparisons for enums
                        return false;
                    }
                    else if (_unitType >= UnitType.SOLDIER)
                    {
                        takeOverTile(dest);
                        setCurrentAction(ActionType.ATTACKING);
						successfullyMoved = true;
                        shouldMerge = false;
					}
					break;
                case OccupantType.STRUCTURE:
                    StructureComponent destStruct = dest.getOccupyingStructure();
                    StructureType sType = destStruct.getStructureType();

                    if (destVillage == _village && sType != StructureType.TOMBSTONE)
                    {
                        return false;
                    }
                    else if (destVillage != _village && sType == StructureType.WATCHTOWER)
                    {
                        if (_unitType <= UnitType.INFANTRY)
                        {
                            return false;
                        }
                        else if (_unitType >= UnitType.SOLDIER)
                        {
                            //destStruct.destroy() //TODO: implement a destroy method which destroys an enemy's structure upon invasion
                            takeOverTile(dest);
                            setCurrentAction(ActionType.ATTACKING);
							successfullyMoved = true;
                            shouldMerge = false;
						}
					}
                    else if (sType == StructureType.TOMBSTONE)
                    {
                        //destStruct.destroy() //TODO: implement a destroy method which destroys a tombstone when land has been overtaken
						setCurrentAction(ActionType.CLEARING_TOMBSTONE);
						if (destVillage == _village)
                        {
                            _village.associate(dest);
							setLocation(dest);
                        }
                        else
                        {
                            takeOverTile(dest);
                        }
						successfullyMoved = true;
                        shouldMerge = true;
					}
					break;
            }

			if(successfullyMoved){
                bool isPaved = dest.hasRoad();

				switch (lType)
				{
				case LandType.MEADOW:
					if (!isPaved && (_unitType != UnitType.INFANTRY && _unitType != UnitType.PEASANT))
					{
						//setLocation(dest);
						TrampleMeadow(dest);                        
						//return true;
					}
					break;
				case LandType.FOREST:
					if (_unitType != UnitType.KNIGHT || _unitType != UnitType.CANNON)
					{
						GatherWood(dest);
                        setLocation(dest);
						//return true;
					}
					break;
				}
			}
			return shouldMerge;
	}


	public void setUnitType(UnitType unitType, bool updateAppearance) {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCsetUnitType", RPCMode.Others, (int)unitType, updateAppearance);
		}
		RPCsetUnitType((int)unitType, updateAppearance);
	}
	
	[RPC]
	private void RPCsetUnitType(int unitTypeIndex, bool updateAppearance) {
		_unitType = (UnitType)unitTypeIndex;
		if(updateAppearance){
			if(_unitGameObject){
				GameObject oldObject = _unitGameObject;
				Destroy(oldObject);
			}
			AssetManager assetManager = GameObject.FindGameObjectWithTag("AssetManager").GetComponent<AssetManager>();
			_unitGameObject = assetManager.createUnitGameObject((UnitType)unitTypeIndex, this.gameObject.transform.position);
			_unitGameObject.transform.parent = this.transform;
		}
	}

	public void createUnit(UnitType unitType) {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCcreateUnit", RPCMode.Others, (int)unitType);
		}
		RPCcreateUnit((int)unitType);
	}
	
	[RPC]
	private void RPCcreateUnit(int unitTypeIndex) {
		_unitType = (UnitType)unitTypeIndex;
		if(_unitGameObject){
			GameObject oldObject = _unitGameObject;
			Destroy(oldObject);
		}
		AssetManager assetManager = GameObject.FindGameObjectWithTag("AssetManager").GetComponent<AssetManager>();
		_unitGameObject = assetManager.createUnitGameObject((UnitType)unitTypeIndex, this.gameObject.transform.position);
		_unitGameObject.transform.parent = this.transform;
	}

    public UnitType getUnitType()
    {
        return _unitType;
    }

    public VillageComponent getVillage()
    {
        return _village;
    }

    public void setVillage(VillageComponent village)
    {
        _village = village;
    }


    /*********************
     *      METHODS      *
     ********************/

    public static uint calculateCost(UnitType u1, UnitType u2)
    {
        if (u1 > u2)
        {
            throw new System.Exception("The first parameter cannot be smaller than the second.");
        }

        uint cost = 0;

        for (UnitType u = u1; u != u2; ++u)
        {
            cost += COST_PER_UNIT_UPGRADE;
        }

        return cost;
    }
    public void CombineUnit(UnitComponent uc)
    {
        if (this.Equals(uc))
        {
            ThrowError("You cannot combine a unit with itself.");
            return;
        }
        if (this.GetComponent<TileComponent>().getPlayerIndex() != uc.GetComponent<TileComponent>().getPlayerIndex())
        {
            ThrowError("You can only combine two of your own units.");
            return;
        }
        // peasant + peasant = infantry
        // peasant + infantry = soldier
        // peasant + soldier = knight
        // infantry + infantry = knight
        if (_unitType == UnitType.PEASANT)
        {
            if (uc.getUnitType() == UnitType.PEASANT)
            {
                // infantry
                uc.destroy();
                setUnitType(UnitType.INFANTRY, true);
                return;
            }
            if (uc.getUnitType() == UnitType.INFANTRY)
            {
                // soldier
                uc.destroy();
                setUnitType(UnitType.SOLDIER, true);
                return;
            }
            if (uc.getUnitType() == UnitType.SOLDIER)
            {
                // knight
                uc.destroy();
                setUnitType(UnitType.KNIGHT, true);
                return;
            }
        }
        if (_unitType == UnitType.INFANTRY)
        {
            if (uc.getUnitType() == UnitType.INFANTRY)
            {
                // knight
                uc.destroy();
                setUnitType(UnitType.KNIGHT, true);
                return;
            }
        }


        if (_unitType == UnitType.PEASANT)
        {
            if (uc.getUnitType() == UnitType.PEASANT)
            {
                // infantry
                uc.destroy();
                setUnitType(UnitType.INFANTRY, true);
                return;
            }
        }
        if (_unitType == UnitType.INFANTRY)
        {
            if (uc.getUnitType() == UnitType.PEASANT)
            {
                // soldier
                uc.destroy();
                setUnitType(UnitType.SOLDIER, true);
                return;
            }
            if (uc.getUnitType() == UnitType.INFANTRY)
            {
                // knight
                uc.destroy();
                setUnitType(UnitType.KNIGHT, true);
                return;
            }
        }
        if (_unitType == UnitType.SOLDIER)
        {
            if (uc.getUnitType() == UnitType.PEASANT)
            {
                // knight
                uc.destroy();
                setUnitType(UnitType.KNIGHT, true);
                return;
            }
        }
        ThrowError("The two units cannot be merged.");
    }

    public bool upgradeUnit(UnitType newLevel)
    {
        if (newLevel == UnitType.CANNON) return false;
        if (this._unitType == UnitType.CANNON) 
        {
            ThrowError("Cannons cannot be upgraded.");
            return false;
        }
        if (newLevel >= _unitType)
        {
            uint cost = calculateCost(_unitType, newLevel);

            if (_village.getGoldStock() >= cost)
            {
				setUnitType(newLevel, true);
                _village.removeGold(cost);
                return true;
            }
            else
            {
                ThrowError("Insufficient gold.");
                return false;
            }
        }
        else
        {
            ThrowError("Invalid upgrade type.");
        }

        return false;
    }
	
    public void buildRoad()
    {
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
                    _currentAction = ActionType.BUILDING_ROAD;
                }
            }
        }
        else
        {
            ThrowError("Only a peasant can build a road.");
        }
    }


	public void Initialize(UnitType uType)
	{
		_roundsCultivating = 0;
		_upkeep = UPKEEP[uType];
		_currentAction = ActionType.READY_FOR_ORDERS;
		TileComponent tComponent = this.gameObject.GetComponent<TileComponent>();
		_village = tComponent.getVillage();
		createUnit(uType);
		SetColour(tComponent.getPlayerIndex());
		tComponent.setOccupantType(OccupantType.UNIT);
	}

    public void cultivate()
    {
        if (_roundsCultivating == 0)
        {
            this.GetComponent<TileComponent>().setLandType(LandType.MEADOW);
            _currentAction = ActionType.READY_FOR_ORDERS;
            _roundsCultivating = 0;
        }
        else
        {
            _roundsCultivating--;
        }
    }

    public void TrampleMeadow(TileComponent tile)
    {
        tile.setLandType(LandType.GRASS);
    }

    public void GatherWood(TileComponent tile)
    {
        _currentAction = ActionType.GATHERING_WOOD;
        _village.addWood(1);
        tile.setLandType(LandType.GRASS);
    }

    public void die()
    {
        _village.RemoveSupportingUnit(this);
        TileComponent tc = this.GetComponent<TileComponent>();
        tc.setOccupyingUnit(null);
        tc.setOccupantType(OccupantType.STRUCTURE);
		StructureComponent tombstone = new StructureComponent(StructureType.TOMBSTONE, tc);
        tombstone.CreateStructure(StructureType.TOMBSTONE);
		tc.setOccupyingStructure(tombstone);
        GameObject.Destroy(this.getGameObject());
    }

    public void destroy()
    {
        TileComponent tc = this.GetComponent<TileComponent>();
        tc.setOccupyingUnit(null);
        tc.setOccupantType(OccupantType.NONE);
        GameObject.Destroy(this.getGameObject());
    }

    public void cultivateMeadow()
    {
        if (this.GetComponent<TileComponent>().getLandType() == LandType.MEADOW)
        {
            ThrowError("There is already a meadow on this tile.");
            return;
        }
        _currentAction = ActionType.CULTIVATING_MEADOW;
        _roundsCultivating = 2;
    }

    public void moveUnit(TileComponent destination)
    {
        if (_currentAction == ActionType.READY_FOR_ORDERS)
        {   
            List<TileComponent> neighbours = this.GetComponent<TileComponent>().getNeighbours();
            int id = this.GetComponent<TileComponent>().getID();

            bool isReachable = false;

            foreach (TileComponent tile in neighbours)
            {
                if (tile.getID() == destination.getID())
                {
                    isReachable = true;
                    break;
                }
            }

            if (isReachable)
            {
                bool shouldMerge = setDestination(destination);
				if (shouldMerge) destination.connectRegions();
                if (shouldMerge && _unitType == UnitType.CANNON) _currentAction = ActionType.ALREADY_MOVED;
            }
            else
            {
                ThrowError("The unit cannot reach the destination tile.");
            }
        }
        else
        {
            ThrowError("The unit cannot move any more in this turn.");
        }
    }

    public void fireOnVillage(TileComponent target)
    {
        if (_unitType == UnitType.CANNON)
        {
            if (_village.getWoodStock() < 1)
            {
                ThrowError("Insufficient wood.");
                return;
            }
            // add all tiles within 2 tile radius of current tile
            HashSet<TileComponent> fireableArea = this.GetComponent<TileComponent>().getTwoHexRadius();
            
            if (fireableArea.Contains(target))
            {
                if (target.getOccupantType() == OccupantType.UNIT)
                {
                    // destroy unit 
                    target.getOccupyingUnit().die();
                }
                if (target.getOccupantType() == OccupantType.VILLAGE)
                {
                    var village = target.getVillage();
                    village.DecrementHealth();
                    if (village.GetHealthLeft() == 0)
                    {
                        village.DestroyVillage();
                    }
                }
                _village.removeWood(1);
            }
            else
            {
                ThrowError("Unable to reach target tile.");
                return;
            }
        }
    }


    public void takeOverTile(TileComponent dest)
    {
        // get current village of tile
        VillageComponent previousVillage = dest.getVillage();
        int previousPlayer = dest.getPlayerIndex();
        VillageType previousVillageType = previousVillage.getVillageType();
        float woodStock = previousVillage.getWoodStock();
        float goldStock = previousVillage.getGoldStock();

        bool tileInvaded = false;
        bool destroyVillage = false;

        // get what's occupying the tile
        switch (dest.getOccupantType())
        {
            case OccupantType.NONE:
                // if not protected by a stronger unit in a 1 hex radius, give tile to this unit's village
                tileInvaded = true;
                foreach (var t in this.GetComponent<TileComponent>().getTwoHexRadius())
                {
                    if (t.getOccupyingUnit() != null && t.getPlayerIndex() == previousVillage.getControlledRegion()[0].getPlayerIndex())
                    {
                        if (t.getOccupyingUnit().getUnitType() != UnitType.CANNON && t.getOccupyingUnit().getUnitType() >= _unitType)
                        {
                            tileInvaded = false;
                            ThrowError("An enemy unit is guarding this tile.");
                        }
                    }
                    if (t.getOccupyingStructure() != null && t.getPlayerIndex() == previousVillage.getControlledRegion()[0].getPlayerIndex())
                    {
                        if (t.getOccupyingStructure().getStructureType() == StructureType.WATCHTOWER && _unitType <= UnitType.INFANTRY)
                        {
                            tileInvaded = false;
                            ThrowError("An enemy watch tower is guarding this tile.");
                        }
                    }
                }
                break;
            case OccupantType.STRUCTURE:
                // if unit is a soldier or knight, kill structure, give tile to this unit's village
                if (_unitType == UnitType.SOLDIER || _unitType == UnitType.KNIGHT)
                {
                    tileInvaded = true;
                    dest.getOccupyingStructure().die();
                }
                else
                {
                    ThrowError("Your unit is not stronger than the watch tower.");
                }
                break;
            case OccupantType.UNIT:
                // if unit's level < current unit's level, kill unit, give tile to this unit's village
                if (dest.getOccupyingUnit().getUnitType() < _unitType)
                {
                    tileInvaded = true;
                    dest.getOccupyingUnit().die();
                }
                else
                {
                    ThrowError("Your unit is not stronger than the opposing unit.");
                }
                break;
            case OccupantType.VILLAGE:
                // if unit is a soldier or knight, turn village tile to meadow of this unit's color
                if (_unitType == UnitType.SOLDIER || _unitType == UnitType.KNIGHT)
                {
                    tileInvaded = true;
                    dest.TurnVillageToMeadow(this.GetComponent<TileComponent>().getPlayerIndex());
                    foreach (var t in previousVillage.getControlledRegion())
                    {
                        t.setVillage(null);
                    }
                }
                break;
        }

        if (tileInvaded)
        {
            // give tile to this unit's village
			TileComponent currentTile = this.GetComponent<TileComponent>();
            _village.addToControlledRegion(dest);
            dest.setPlayerIndex(currentTile.getPlayerIndex());
			dest.setOccupyingUnit(this);
			dest.setOccupantType(OccupantType.UNIT);
			currentTile.setOccupyingUnit(null);
			previousVillage.RemoveTile(dest);
            if (previousVillage.getControlledRegion().Count < 3)
            {
                destroyVillage = true;
            }
			setCurrentAction(ActionType.EXPANDING_REGION);
            setLocation(dest);
        }

        // if less than 3 tiles left, destroy village
        if (destroyVillage)
        {
            // give resources to this village
            _village.addGold(previousVillage.getGoldStock());
            _village.addWood(previousVillage.getWoodStock());
            previousVillage.DestroyVillage();
        }
        else
        {
            // if >=3 tiles left, randomly regenerate new village on remaining tiles
            List<TileComponent> totalArea = new List<TileComponent>();
            foreach (var t in previousVillage.getControlledRegion())
            {
                totalArea.Add(t);
            }
            // find all components of land belonging to opposing player, regenerate village randomly on one
			int temp = 0;
			TileComponent tempTile = previousVillage.getControlledRegion()[temp];
			while (tempTile.getID() == dest.getID() && temp < previousVillage.getControlledRegion().Count) {
				tempTile = previousVillage.getControlledRegion()[++temp];
			}
            List<TileComponent> firstArea = tempTile.breadthFS();


            foreach (var t in firstArea)
            {
                if (totalArea.Contains(t))
                {
                    totalArea.Remove(t);
                }
            }

            // if controlled region isn't empty, there's a second area
            List<TileComponent> secondArea = new List<TileComponent>();
            if (totalArea.Count > 1 || totalArea.Count == 1 && totalArea[0].getID() != dest.getID())
            {
                secondArea = totalArea[0].breadthFS();
                foreach (var t in secondArea)
                {
                    if (totalArea.Contains(t))
                    {
                        totalArea.Remove(t);
                    }
                }
            }
            List<TileComponent> thirdArea = new List<TileComponent>();

			if (totalArea.Count > 1 || totalArea.Count == 1 && totalArea[0].getID() != dest.getID())
            {
                thirdArea = totalArea;
            }

            float[] gold = AssignStock(firstArea, secondArea, thirdArea, goldStock);
            float[] wood = AssignStock(firstArea, secondArea, thirdArea, woodStock);

            if (firstArea.Count < 3)
            {
                if (firstArea[0].getVillage() != null)
                {
                    VillageComponent firstVillage = firstArea[0].getVillage();
                    GameObject.Destroy(firstVillage.getVillageGameObject());
                    foreach (var tt in totalArea)
                    {
                        tt.setVillage(null);
                    }
                }
                foreach (var t in firstArea)
                {
                    t.setPlayerIndex(0);
                    t.setVillage(null);
                }
            }
            else
            {
                foreach (var t in firstArea)
                {
                    if (t.getVillage() == null)
                    {
                        VillageComponent newHovel = GameObject.FindObjectOfType<GameComponent>().CreateVillage(t, previousVillageType, GameObject.FindObjectOfType<GameComponent>().getRemainingPlayers()[previousPlayer - 1]);
                        t.setOccupantType(OccupantType.VILLAGE);

                        newHovel.associate(t);
                        newHovel.addGold((uint)gold[0]);
                        newHovel.addWood((uint)wood[0]);
                        foreach (var tile in firstArea)
                        {
                            tile.setVillage(newHovel);
                            newHovel.addToControlledRegion(tile);
                        }
                        break;
                    }
                }
            }
            remainingVillage(secondArea, previousPlayer, previousVillageType, gold[1], wood[1]);
            remainingVillage(thirdArea, previousPlayer, previousVillageType, gold[2], wood[2]);

        }

		foreach (TileComponent tile in previousVillage.getControlledRegion()) {
			tile.UpdateDraw();
		}
    }

    private float[] AssignStock(List<TileComponent> firstArea, List<TileComponent> secondArea, List<TileComponent> thirdArea, float stock)
    {
        float[] assignment = new float[3];

        for (int i = 0; i < 3; i++)
        {
            assignment[i] = 0;

        }

        if (firstArea.Count > 2 && secondArea.Count > 2 && thirdArea.Count > 2)
        {
            assignment[0] = (int)stock / 3;
            assignment[1] = (int)stock / 3;
            assignment[2] = stock - assignment[0] - assignment[1];
        }
        else if (firstArea.Count > 2 && secondArea.Count > 2)
        {
            assignment[0] = (int)stock / 2;
            assignment[1] = stock - assignment[0];
        }

        else if (thirdArea.Count > 2 && secondArea.Count > 2)
        {
            assignment[2] = (int)stock / 2;
            assignment[1] = stock - assignment[2];
        }

        else if (thirdArea.Count > 2 && firstArea.Count > 2)
        {
            assignment[2] = (int)stock / 2;
            assignment[0] = stock - assignment[2];
        }
        else if (firstArea.Count > 2)
        {
            assignment[0] = stock;
        }
        else if (secondArea.Count > 2)
        {
            assignment[1] = stock;
        }
        else if (thirdArea.Count > 2)
        {
            assignment[2] = stock;
        }
        return assignment;
    }

    private void remainingVillage(List<TileComponent> area, int playerIndex, VillageType villageType, float gold, float wood)
    {
        if (area.Count > 0)
        {
            foreach (var t in area)
            {
                t.setVillage(null);
            }
            if (area.Count < 3)
            {
                if (area[0].getVillage() != null)
                {
                    VillageComponent firstVillage = area[0].getVillage();
                    GameObject.Destroy(firstVillage);
                }
                foreach (var t in area)
                {
                    t.setPlayerIndex(0);
                    t.setVillage(null);
                }
            }
            else
            {
                foreach (var t in area)
                {
                    if (t.getVillage() == null)
                    {
                        var players = GameObject.FindObjectOfType<GameComponent>().getRemainingPlayers();
                        VillageComponent newHovel = GameObject.FindObjectOfType<GameComponent>().CreateVillage(t, villageType, GameObject.FindObjectOfType<GameComponent>().getRemainingPlayers()[playerIndex - 1]);
                        t.setOccupantType(OccupantType.VILLAGE);
                        //t.UpdateVillageReference();

                        newHovel.associate(t);
                        newHovel.addGold((uint)gold);
                        newHovel.addWood((uint)wood);
                        foreach (var tile in area)
                        {
                            tile.setVillage(newHovel);
                            newHovel.addToControlledRegion(tile);
                        }
                        break;
                    }
                }
            }
        }
    }
    public GameObject getGameObject()
    {
        return _unitGameObject;
    }

    public void setGameObject(GameObject gameObject)
    {
		_unitGameObject = gameObject;
		_unitGameObject.transform.parent = this.gameObject.transform;
		_isMoving = true;
    }

	public void SetColour(int index)
	{
		Renderer[] renderers = _unitGameObject.GetComponentsInChildren<Renderer>() as Renderer[];
		for (int i = 0; i < renderers.Length; i++)
		{
			foreach (Material m in renderers[i].materials){
				if(m.name.Contains("mat_dark") || 
				   m.name.Contains("mat_clothDark") ){
					m.SetColor("_Color", PLAYER_COLOURS_DARK[index]);
				}
				else if(m.name.Contains("mat_light") ||
				        m.name.Contains("mat_clothLight") ){
					m.SetColor("_Color", PLAYER_COLOURS[index]);
				}
			}
		}
	}

    /*********************
     *   UNITY METHODS   *
     ********************/

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
		if(_isMoving){
			float moveRate = 10.0f;
			if( Vector3.Magnitude(this.gameObject.transform.position - _unitGameObject.transform.position) > 0.01f){
				_unitGameObject.transform.position = Vector3.Lerp(_unitGameObject.transform.position, this.gameObject.transform.position, moveRate * Time.deltaTime);
			}
			else{
				_unitGameObject.transform.position = this.gameObject.transform.position;
				_isMoving = false;
			}
		}
    }

    void Awake()
    {

    }
}