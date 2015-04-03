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
			//Copy all the info into the destination tile
			destComponent.setCurrentAction( this.getCurrentAction() );
			destComponent.setUnitType( this.getUnitType() );
			destComponent.setVillage( this.getVillage() );
			destComponent.setGameObject( this.getGameObject() );
			destination.setOccupantType(OccupantType.UNIT);

			//Remove the association from the previous tile
			Destroy(this.gameObject.GetComponent<UnitComponent>());
			this.gameObject.GetComponent<TileComponent>().setOccupantType(OccupantType.NONE);

			//Update the visible object's location
			//_unitGameObject.transform.position = destination.gameObject.transform.position; //Use this only if you want them to snap to the new position
			//_isMoving = true;

			//Keep the UnitComponent selected for further actions
			GameComponent gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameComponent>();
            gameManager.setLastSelectedUnit(destComponent.GetComponent<UnitComponent>());
		}

        return true;
    }

    public bool setDestination(TileComponent dest)
    {
        /**
         * Implementation of setLocation Seq. Diagr.
         * setLocation above is actually a setter.
         */
        bool contested = isContested(dest);
        if (!contested)
        {
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
                        _village.associate(dest);
						setCurrentAction(ActionType.EXPANDING_REGION);
						setLocation(dest);
						dest.setPlayerIndex(this.GetComponent<TileComponent>().getPlayerIndex());
						successfullyMoved = true;
					}
                    // destination is an empty tile in different player's village
					else if (destVillage != null && destVillage != _village)
                    {
                        if (_unitType == UnitType.PEASANT || _unitType == UnitType.CANNON)
                        {
							return false;
                        } 
                        takeOverTile(dest);
                        setCurrentAction(ActionType.EXPANDING_REGION);
						successfullyMoved = true;
					}
                    // destination is an empty tile in the same village as the unit currently belongs to
					else if (destVillage != null && destVillage == _village)
                    {
						setLocation(dest);
						successfullyMoved = true;
					}
					break;
                case OccupantType.UNIT:
                    if (destVillage == _village)
                    {
                        return false;
                    }
                    UnitComponent destUnit = dest.getOccupyingUnit();
                    //destUnit.dieInBattle(); //TODO: implement a method which kills the unit on the newly invaded tile.
                    takeOverTile(dest);
                    setCurrentAction(ActionType.ATTACKING);
					successfullyMoved = true;
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
						//setLocation(dest);
						GatherWood(dest);                       
						//return true;
					}
					break;
				}
			}
			return true;
		}

		return false;
	}
	
	public void setUnitType(UnitType unitType) {
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCsetUnitType", RPCMode.Others, (int)unitType);
		}
		RPCsetUnitType((int)unitType);
	}
	
	[RPC]
	private void RPCsetUnitType(int unitTypeIndex) {
		_unitType = (UnitType)unitTypeIndex;
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

    public bool isContested(TileComponent destination)
    {
        List<TileComponent> neighbours = destination.getNeighbours();
        neighbours.Add(destination);

        foreach (TileComponent tile in neighbours)
        {
            UnitComponent enemyUnit = tile.getOccupyingUnit();

            if (enemyUnit)
            {
                VillageComponent enemyVillage = enemyUnit.getVillage();

                if (enemyVillage != _village)
                {
                    UnitType enemyUnitType = enemyUnit.getUnitType();

                    if (_unitType <= enemyUnitType)
                    {
                        return true;
                    }
                }
            }
            else
            {
                StructureComponent enemyStructure = tile.getOccupyingStructure();

                if (enemyStructure)
                {
                    StructureType enemyStructureType = enemyStructure.getStructureType();

                    if (enemyStructureType == StructureType.WATCHTOWER)
                    {
                        VillageComponent enemyVillage = tile.getVillage();

                        if (_unitType <= UnitType.INFANTRY || _village != enemyVillage)
                        { 
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    public bool upgradeUnit(UnitType newLevel)
    {
        if (newLevel == UnitType.CANNON) return false;
        if (this._unitType == UnitType.CANNON) return false;
        if (newLevel >= _unitType)
        {
            uint cost = calculateCost(_unitType, newLevel);

            if (_village.getGoldStock() >= cost)
            {
				setUnitType(newLevel);
                _village.removeGold(cost);
                return true;
            }
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
                }
            }
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
		tComponent.setOccupantType(OccupantType.UNIT);
	}

    public void cultivate()
    {
        if (_roundsCultivating >= 2)
        {
            this.GetComponent<TileComponent>().setLandType(LandType.MEADOW);
            _currentAction = ActionType.READY_FOR_ORDERS;
            _roundsCultivating = 0;
        }
        else
        {
            ++_roundsCultivating;
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
		TileComponent tc = this.GetComponent<TileComponent>();
		StructureComponent tombstone = new StructureComponent(StructureType.TOMBSTONE, tc);
		tc.setOccupyingStructure(tombstone);
        GameObject.Destroy(this.getGameObject());
    }

    public void moveUnit(TileComponent destination)
    {
        if (_currentAction == ActionType.READY_FOR_ORDERS)
        {   
            List<TileComponent> neighbours = this.GetComponent<TileComponent>().getNeighbours();

            bool isReachable = false;

            foreach (TileComponent tile in neighbours)
            {
                if (tile == destination)
                {
                    isReachable = true;
                    break;
                }
            }

            if (isReachable)
            {
                bool isRelocated = setDestination(destination);
                if (isRelocated && _unitType == UnitType.CANNON) _currentAction = ActionType.ALREADY_MOVED;
            }
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
            HashSet<TileComponent> fireableArea = new HashSet<TileComponent>();
            foreach (var neighbour in this.GetComponent<TileComponent>().getNeighbours())
            {
                fireableArea.Add(neighbour);
            }

            HashSet<TileComponent> neighbourNeighbours = new HashSet<TileComponent>();

            foreach (var neighbour in fireableArea)
            {
                foreach (var neighbourNeighbour in neighbour.getNeighbours())
                {
                    neighbourNeighbours.Add(neighbourNeighbour);
                }
            }

            foreach (var nn in neighbourNeighbours)
            {
                fireableArea.Add(nn);
            }
            
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
                        // TODO: destroy village
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

    public void takeOverTile(TileComponent destination)
    {
        VillageComponent enemyVillage = destination.getVillage();
        PlayerComponent enemyPlayer = enemyVillage.getPlayer();
        OccupantType destinationOccupantType = destination.getOccupantType();

        if (destinationOccupantType == OccupantType.VILLAGE)
        {
            _village.addGold(enemyVillage.getGoldStock());
            _village.addWood(enemyVillage.getWoodStock());
            enemyPlayer.remove(enemyVillage);
        }

        _village.associate(destination);
		setLocation(destination);

        List<TileComponent> neighbours = destination.getNeighbours();

        bool isVillageDestroyed = false;

        foreach (TileComponent neighbour in neighbours)
        {
            List<TileComponent> region = neighbour.breadthFS();

            if (region.Count >= 3 && !TileComponent.containsVillage(region))
            {
                VillageComponent newHovel = new VillageComponent(0, 0, enemyPlayer, region, null, VillageType.HOVEL);

                enemyPlayer.add(newHovel);

                foreach (TileComponent tile in region)
                {
                    newHovel.associate(tile);

                    if (tile.getOccupantType() == OccupantType.UNIT)
                    {
                        newHovel.associate(tile.getOccupyingUnit());
                    }
                }
            }
            else
            {
                foreach (TileComponent tile in region)
                {
                    OccupantType tileOccupantType = tile.getOccupantType();

                    if (tileOccupantType == OccupantType.UNIT)
                    {
                        UnitComponent occupyingUnit = tile.getOccupyingUnit();
                        occupyingUnit.die(); 

                        StructureComponent tomb = new StructureComponent(StructureType.TOMBSTONE, tile);
                        tile.setOccupyingStructure(tomb);
                    }
                    else if (tileOccupantType == OccupantType.VILLAGE)
                    {
                        VillageComponent village = tile.getVillage();
                        enemyPlayer.remove(village);
                        // TODO: KILL VILLAGE
                        village = null;

                        tile.setOccupyingStructure(null);

                        isVillageDestroyed = true;
                    }
                }
            }
        }

        if (isVillageDestroyed)
        {
            List<VillageComponent> enemyVillages = enemyPlayer.getVillages();

            GameComponent game = enemyPlayer.getGame();
            List<PlayerComponent> players = game.getRemainingPlayers();

            if (enemyVillages.Count == 0)
            {
                enemyPlayer.incrementLosses();
                game.removePlayer(enemyPlayer);

                players = game.getRemainingPlayers();
            }

            if (players.Count == 1)
            {
                players[0].incrementWins();
                game.endGame();
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