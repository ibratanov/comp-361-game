using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	public GameObject[] _menus;

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
}
