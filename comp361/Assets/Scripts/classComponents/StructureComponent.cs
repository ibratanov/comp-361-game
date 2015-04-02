using UnityEngine;
using System.Collections;
using Assets.Scripts.classComponents;

public enum StructureType
{
	NONE,
	TOMBSTONE,
	WATCHTOWER
}

public class StructureComponent : GenericComponent
{

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	StructureType _structureType;
	TileComponent _location;

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
