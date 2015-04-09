using UnityEngine;
using System.Collections;
using Assets.Scripts.classComponents;

public enum StructureType
{
	TOMBSTONE,
	WATCHTOWER,
    NONE
}

public class StructureComponent : GenericComponent
{

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	StructureType _structureType;
	TileComponent _location;
    GameObject _structureGameObject;

	/*********************
	 *    CONSTRUCTOR    *
	 ********************/

	public StructureComponent (StructureType structureType, TileComponent location)  {
		_structureType = structureType;
		_location = location;
        _structureGameObject = new GameObject();
	}

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

    public void CreateStructure(StructureType st)
    {
        if (Network.isServer || Network.isClient)
        {
            networkView.RPC("RPCCreateStructure", RPCMode.Others, (int)st);
        }
        RPCCreateStructure((int)st);
    }


    [RPC]
    public void RPCCreateStructure(int st)
    {
        _structureType = (StructureType)st;
        if (_structureGameObject)
        {
            GameObject oldObject = _structureGameObject;
            Destroy(oldObject);
        }
        AssetManager assetManager = GameObject.FindGameObjectWithTag("AssetManager").GetComponent<AssetManager>();
        _structureGameObject = assetManager.createStructureGameObject((StructureType)st, _location.gameObject.transform.position);
        _structureGameObject.transform.parent = _location.gameObject.transform;
    }


	public StructureType getStructureType() {
		return _structureType;
	}

	public void setStructureType(StructureType structureType) {
		_structureType = structureType;
	}

	public TileComponent getLocation()
	{
		return _location;
	}

	public void setLocation(TileComponent location)
	{
		_location = location;
	}

    public void DestroyStructureGameObject()
    {
        Destroy(_structureGameObject);
    }

    public void die()
    {
        TileComponent tc = this.GetComponent<TileComponent>();
        tc.setOccupyingStructure(null);
        tc.setOccupantType(OccupantType.NONE);
        GameObject.Destroy(_structureGameObject);
    }

    public void Attack(TileComponent tc)
    {
        if (tc.getOccupantType() != OccupantType.UNIT)
        {
            ThrowError("There is no unit to attack on this tile.");
        }
        else
        {
            var uc = tc.getOccupyingUnit();
            if (_location.getNeighbours().Contains(uc.getLocation()))
            {
                if (uc.getUnitType() == UnitType.PEASANT)
                {
                    uc.die();
                }
                else
                {
                    ThrowError("You cannot attack this unit with a watchtower.");
                }
            }
            else
            {
                ThrowError("This unit is too far away to attack.");
            }
        }
        


    }

	/*********************
	 *      METHODS      *
	 ********************/

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}
}
