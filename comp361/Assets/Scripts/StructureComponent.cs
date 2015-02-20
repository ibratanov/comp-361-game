using UnityEngine;
using System.Collections;

enum StructureType
{
	NONE,
	TOMBSTONE,
	WATCHTOWER
}

public class StructureComponent : MonoBehaviour {

	StructureType _structureType;
	TileComponent _location;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	StructureType getStructureType() {
		return _structureType;
	}

	void setStructureType(StructureType structureType) {
		_structureType = structureType;
	}

	TileComponent getLocation()
	{
		return _location;
	}

	void setLocation(TileComponent location)
	{
		_location = location;
	}
}
