using UnityEngine;
using System.Collections;

public class AssetManager : MonoBehaviour {
	public GameObject[] _terrains;
	public GameObject[] _villagerUnits;
	public GameObject[] _villages;
	public GameObject[] _structures;
    public Material[] _villageMaterials;

	public GameObject createTerrainGameObject(LandType lType, Vector3 position){
		return (GameObject)Instantiate(_terrains[(int)lType], position, Quaternion.identity);
	}

	public GameObject createUnitGameObject(UnitType uType, Vector3 position){
		return (GameObject)Instantiate(_villagerUnits[(int)uType], position, Quaternion.identity);
	}

	public GameObject createVillageGameObject(VillageType vType, Vector3 position){
		return (GameObject)Instantiate(_villages[(int)vType], position, Quaternion.identity);
	}

    public GameObject createStructureGameObject(StructureType sType, Vector3 position)
    {
        return (GameObject)Instantiate(_structures[(int)sType], position, Quaternion.identity);
    }

    public Material getVillageMaterial(VillageType vType)
    {
        return _villageMaterials[(int)vType];
    }
}
