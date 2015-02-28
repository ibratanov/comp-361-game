using UnityEngine;
using System.Collections;

public class AssetManager : MonoBehaviour {
	public GameObject[] _terrains;
	public GameObject[] _villagerUnits;
	public GameObject[] _villages;
	public GameObject[] _structures;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public GameObject getTerrainGameObject(LandType lType){
		return _terrains[(int)lType];
	}

	public GameObject getUnitGameObject(UnitType uType){
		return _villagerUnits[(int)uType];
	}

	public GameObject getVillageGameObject(VillageType vType){
		return _villages[(int)vType];
	}

	public GameObject getStructureGameObject(StructureType sType){
		return _structures[(int)sType];
	}
}
