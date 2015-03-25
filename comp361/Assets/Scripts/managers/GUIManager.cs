﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {
	public GameObject[] _menus;
	public GameObject[] _inGamePanels;
	public float _fadeSpeed;

	// Use this for initialization
	void Start () {
		for(int i = 0; i < _menus.Length; ++i){
			if( (_menus[i].name.Contains("Menu_Title") && !_menus[i].name.Contains("Online")) || _menus[i].name.Contains("Menu_Background")){
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

    public void setWoodStock(int stock)
    {
        for (int i = 0; i < _inGamePanels.Length; ++i)
        {
            if (_inGamePanels[i].name.Contains("Panel_Resources"))
            {
                _inGamePanels[i].SetActive(true);
                for (int j = 0; j < _inGamePanels[i].transform.childCount; j++)
                {
                    if (_inGamePanels[i].transform.GetChild(j).name.Contains("Text_WoodCount"))
                    {
                        _inGamePanels[i].transform.GetChild(j).GetComponent<Text>().text = stock.ToString();
                    }
                }
            }
        }
    }

	public void DisplayInGameMenu(){
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCDisplayInGameMenu", RPCMode.All);
		}
		else{
			RPCDisplayInGameMenu();
		}
	}

	[RPC]
	private void RPCDisplayInGameMenu(){
		for(int i = 0; i < _menus.Length; ++i){
			if(_menus[i].name.Contains("Menu_InGame")){
				_menus[i].SetActive(true);
			}
			else{
				_menus[i].SetActive(false);
			}
		}
	}

	public void DisplayVillageActions(){
		for(int i = 0; i < _inGamePanels.Length; ++i){
			if(_inGamePanels[i].name.Contains("Panel_Village_Actions")){
				_inGamePanels[i].SetActive(true);
			}
			else if (!(_inGamePanels[i].name.Contains("CurrentPlayer")))
			{
				_inGamePanels[i].SetActive(false);
			}
		}
	}

    public void HideVillageActions()
    {
        for (int i = 0; i < _inGamePanels.Length; ++i)
        {
            if (_inGamePanels[i].name.Contains("Panel_Village_Actions") || _inGamePanels[i].name.Contains("Panel_HireUnit_Village") ||
                _inGamePanels[i].name.Contains("Panel_Upgrade_Village"))
            {
                _inGamePanels[i].SetActive(false);
            }
        }
    }

	public void DisplayUnitActions(){
		for(int i = 0; i < _inGamePanels.Length; ++i){
            if (_inGamePanels[i].name.Contains("Panel_Unit_Actions"))
            {
				_inGamePanels[i].SetActive(true);
			}
			else if (!(_inGamePanels[i].name.Contains("CurrentPlayer")))
			{
				_inGamePanels[i].SetActive(false);
			}
		}
	}

    public void HideUnitActions()
    {
        for (int i = 0; i < _inGamePanels.Length; ++i)
        {
            if (_inGamePanels[i].name.Contains("Panel_Unit_Actions"))
            {
                _inGamePanels[i].SetActive(false);
            }
        }
    }

	public void DisplayTurnPanel(PlayerComponent currentPlayer, Color currentColor)
	{
		foreach (GameObject g in _inGamePanels)
		{
			if (g.name.Contains ("PlayerTurn"))
			{
				FadeInPanel (g);
				return;
			}
		}
	}

	public void FadeInPanel(GameObject panel)
	{

	}

	public void FadeOutPanel(GameObject panel)
	{

	}

	public void UpdateGamePanels(PlayerComponent currentPlayer, Color currentColor)
	{
		foreach (GameObject g in _inGamePanels)
		{
			if (g.name.Contains ("Panel_CurrentPlayer"))
			{
				Text[] playerTexts = g.GetComponentsInChildren<Text>() as Text[];
				foreach (Text t in playerTexts)
				{
					if (t.name.Contains ("CurrentPlayer"))
					{
						t.text = currentPlayer.getUserName();
						t.color = currentColor;
					}
				}
				break;
			}
		}
	}
}
