using UnityEngine;
using System.Collections.Generic;

public enum ActionType
{
    READY_FOR_ORDERS,
    GATHERING_WOOD,
    CLEARING_TOMBSTONE,
    CULTIVATING_MEADOW,
    BUILDING_ROAD,
    ATTACKING,
    EXPANDING_REGION
}

public enum UnitType
{
    PEASANT,
    INFANTRY,
    SOLDIER,
    KNIGHT
}

public class UnitComponent : MonoBehaviour
{
    public GameObject _villagerGameObject;

    readonly static uint COST_PER_UNIT_UPGRADE = 10;
    public readonly static Dictionary<UnitType, uint> UPKEEP = new Dictionary<UnitType, uint>()
    {
        {UnitType.PEASANT, 2},
        {UnitType.INFANTRY, 6},
        {UnitType.SOLDIER, 18},
        {UnitType.KNIGHT, 54}
    };

    public readonly static Dictionary<UnitType, uint> INITIAL_COST = new Dictionary<UnitType, uint>()
    {
        {UnitType.PEASANT, 10},
        {UnitType.INFANTRY, 20},
        {UnitType.SOLDIER, 30},
        {UnitType.KNIGHT, 40}
    };

    private AssetManager _assets;

    /*********************
     *     ATTRIBUTES    *
     ********************/

    static UnitType[] _upgradeCosts;

    uint _roundsCultivating;
    uint _upkeep;
    ActionType _currentAction;
    TileComponent _location;
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
        return _location;
    }

    public bool setLocation(TileComponent location)
    {
        _location = location;
        _villagerGameObject.transform.position = location.getTileGameObject().transform.position;

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

            switch (occType)
            { //TODO: Check if switch properly checks enum equality
                case OccupantType.NONE:
                    if (lType == LandType.SEA || (_unitType == UnitType.KNIGHT && lType == LandType.FOREST))
                    {
                        return false;
                    }
                    else if (destVillage == null)
                    {
                        _village.associate(dest);
                        associate(dest);
                        setCurrentAction(ActionType.EXPANDING_REGION);
                        return true;
                    }
                    else if (destVillage != null && destVillage != _village)
                    {
                        if (_unitType == UnitType.PEASANT)
                        {
                            return false;
                        }
                        takeOverTile(dest);
                        setCurrentAction(ActionType.EXPANDING_REGION);
                        return true;
                    }
                    else if (destVillage != null && destVillage == _village)
                    {
                        associate(dest);
                        return true;
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
                    return true;
                    break;
                case OccupantType.VILLAGE:
                    if (destVillage == _village || _unitType <= UnitType.INFANTRY)
                    { //TODO: check comparisons for enums
                        return false;
                    }
                    else if (_unitType >= UnitType.SOLDIER)
                    {
                        takeOverTile(dest);
                        setCurrentAction(ActionType.ATTACKING);
                        return true;
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
                            return true;
                        }
                    }
                    else if (sType == StructureType.TOMBSTONE)
                    {
                        //destStruct.destroy() //TODO: implement a destroy method which destroys a tombstone when land has been overtaken
                        if (destVillage == _village)
                        {
                            _village.associate(dest);
                            associate(dest);
                        }
                        else
                        {
                            takeOverTile(dest);
                        }
                        setCurrentAction(ActionType.CLEARING_TOMBSTONE);
                        return true;
                    }
                    break;
            }
        }

        return false;
    }

    public UnitType getUnitType()
    {
        return _unitType;
    }

    public void setUnitType(UnitType unitType)
    {
        _unitType = unitType;
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
                        { //TODO: check condition, should be AND?
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
        if (newLevel >= _unitType)
        {
            uint cost = calculateCost(_unitType, newLevel);

            if (_village.getGoldStock() >= cost)
            {
                _unitType = newLevel;
                _village.removeGold(cost);
                return true;
            }
        }

        return false;
    }

    public void associate(TileComponent tile)
    {
        if (_location != null)
        {
            _location.setOccupantType(OccupantType.NONE);
            _location.setOccupyingUnit(null);
        }

        tile.setOccupantType(OccupantType.UNIT);
        tile.setOccupyingUnit(this);
        setLocation(tile);
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

    public void InstantiateUnit(UnitType unitType)
    {
        _assets = GameObject.FindObjectOfType<AssetManager>();
        _roundsCultivating = 0;
        _upkeep = UPKEEP[unitType];
        _currentAction = ActionType.READY_FOR_ORDERS;
        _location = null;
        _unitType = unitType;
        _village = null;
        _villagerGameObject = (GameObject)Instantiate(_assets.getUnitGameObject(unitType), new Vector3(0, 0, 0), Quaternion.identity);
    }

    public UnitComponent(UnitType unitType)
    {
        _assets = GameObject.FindObjectOfType<AssetManager>();
        _roundsCultivating = 0;
        _upkeep = UPKEEP[unitType];
        _currentAction = ActionType.READY_FOR_ORDERS;
        _location = null;
        _unitType = unitType;
        _village = null;
        _villagerGameObject = (GameObject)Instantiate(_assets.getUnitGameObject(unitType), new Vector3(0, 0, 0), Quaternion.identity);
    }

    public void cultivate()
    {
        if (_roundsCultivating >= 2)
        {
            _location.setLandType(LandType.MEADOW);
            _currentAction = ActionType.READY_FOR_ORDERS;
            _roundsCultivating = 0;
        }
        else
        {
            ++_roundsCultivating;
        }
    }

    public void die()
    {
        StructureComponent tombstone = new StructureComponent(StructureType.TOMBSTONE, _location);
        _location.setOccupyingStructure(tombstone);
    }

    public void moveUnit(TileComponent destination)
    {
        if (_currentAction == ActionType.READY_FOR_ORDERS)
        {
            List<TileComponent> neighbours = _location.getNeighbours();

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
                bool isRelocated = setDestination(destination); //setLocation(destination);

                if (isRelocated)
                {
                    LandType landType = destination.getLandType();
                    bool isPaved = destination.hasRoad();

                    if (landType == LandType.MEADOW && _unitType >= UnitType.SOLDIER && !isPaved)
                    {
                        destination.setLandType(LandType.GRASS);
                    }

                    destination.connectRegions();
                }
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
        associate(destination);

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
                        occupyingUnit.die(); //TODO: check if this is the right die()method

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
        return _villagerGameObject;
    }

    public void setGameObject(AssetManager assets, UnitType unitType)
    {
        _villagerGameObject = assets.getUnitGameObject(unitType);
    }

    /*********************
     *   UNITY METHODS   *
     ********************/

    // Use this for initialization
    void Start()
    {
        _roundsCultivating = 0;
        _currentAction = ActionType.READY_FOR_ORDERS;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {

    }

    public void Select()
    {

    }
}