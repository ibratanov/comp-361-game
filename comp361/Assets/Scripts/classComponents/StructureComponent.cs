using UnityEngine;
using System.Collections;

public enum StructureType
{
	NONE,
	TOMBSTONE,
	WATCHTOWER
}

public class StructureComponent : MonoBehaviour {

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	StructureType _structureType;
	TileComponent _location;

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
