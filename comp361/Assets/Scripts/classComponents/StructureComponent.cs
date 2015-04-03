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

	public StructureComponent (StructureType structureType, TileComponent location) {
		_structureType = structureType;
		_location = location;
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
    private void RPCCreateStructure(int st)
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
