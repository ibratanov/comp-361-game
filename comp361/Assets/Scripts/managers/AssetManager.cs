using UnityEngine;
using System.Collections;

public class AssetManager : MonoBehaviour {
	public GameObject[] _terrains;
	public GameObject[] _villagerUnits;
	public GameObject[] _villages;
	public GameObject[] _structures;

	public GameObject getTerrainGameObject(LandType lType){
		return _terrains[(int)lType];
	}

	public GameObject getTerrainGameObject(int assetIndex){
		return _terrains[assetIndex];
	}

	public GameObject getUnitGameObject(UnitType uType){
		return _villagerUnits[(int)uType];
	}

	public GameObject getUnitGameObject(int assetIndex){
		return _terrains[assetIndex];
	}

	public GameObject getVillageGameObject(VillageType vType){
		return _villages[(int)vType];
	}

	public GameObject getVillageGameObject(int assetIndex){
		return _terrains[assetIndex];
	}

	public GameObject getStructureGameObject(StructureType sType){
		return _structures[(int)sType];
	}

	public GameObject getStructureGameObject(int assetIndex){
		return _terrains[assetIndex];
	}
}
