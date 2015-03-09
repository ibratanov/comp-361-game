using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	public GameObject[] _menus;
	public GameObject[] _inGamePanels;

	// Use this for initialization
	void Start () {
		for(int i = 0; i < _menus.Length; ++i){
			if(_menus[i].name.Contains("Menu_Title") || _menus[i].name.Contains("Menu_Background")){
				_menus[i].SetActive(true);
			}
			else{
				_menus[i].SetActive(false);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DisplayVillageActions(){
		for(int i = 0; i < _inGamePanels.Length; ++i){
			if(_inGamePanels[i].name.Contains("Panel_Village_Actions")){
				_inGamePanels[i].SetActive(true);
			}
			else{
				_inGamePanels[i].SetActive(false);
			}
		}
	}

	public void DisplayUnitActions(){
		for(int i = 0; i < _inGamePanels.Length; ++i){
			if(_inGamePanels[i].name.Contains("Panel_Unit_Actions")){
				_inGamePanels[i].SetActive(true);
			}
			else{
				_inGamePanels[i].SetActive(false);
			}
		}
	}
}
