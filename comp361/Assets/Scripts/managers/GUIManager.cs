﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

public class GUIManager : MonoBehaviour {
	private bool _canClickGame = true;

	public GameObject[] _menus;
	public GameObject[] _inGamePanels;
	public GameObject[] _actionPanels; // panels that are in the bottom-right corner
    public GameObject[] _inGameButtons; // buttons that are updated based on in-game data

	//Loading a profile
	public GameObject _availableProfilesPanel;
	public GameObject _loadProfileButtonPrefab;
	private List<GameObject> _loadProfileButtons = new List<GameObject>();
	private PlayerManager _playerManager;
	private string _profileDirectory = "./profiles/";
	private string _mapDirectory = "./maps/";
	
	public GameObject[] _cameras; 
	public Vector3 _initZoomCameraPos;
	public Vector3 _initZoomOutCamPos;

	public float _fadeSpeed = 0.8f;
    public float _errorFadeSpeed = 1.5f;

	string _currentMap;

	public ColorBlock _standardButtonColors;

	// Use this for initialization
	void Start () {
		_playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
		for(int i = 0; i < _menus.Length; ++i){
			if( _menus[i].name.Contains("Menu_Login_Main") 
			   || _menus[i].name.Contains("Menu_Background")
			   || _menus[i].name.Contains("Menu_AvailableProfiles")){
				_menus[i].SetActive(true);
			}
			else{
				_menus[i].SetActive(false);
			}
		}

		UpdateAvailableProfiles();
		
		foreach (GameObject g in _cameras)
		{
			if (g.name.Contains ("zoom"))
			{
				_initZoomCameraPos = g.transform.position;
			}
		}
	}

	public void UpdateAvailableProfiles(){
		foreach(GameObject button in _loadProfileButtons){
			Destroy (button);
		}
		_loadProfileButtons.Clear();
		if(!Directory.Exists(_profileDirectory))//if it doesn't, create it
		{    
			Directory.CreateDirectory(_profileDirectory);
		}
		DirectoryInfo dInfo = new DirectoryInfo(_profileDirectory);
		FileInfo[] fInfo = dInfo.GetFiles();
		float buttonOffset = 50.0f;
		int skipped = 0;
		for(int i = 0; i < fInfo.Length; ++i){
			FileInfo file = fInfo[i];
			string profileName = file.Name.Split(new string[]{"Info.dat"}, System.StringSplitOptions.None)[0];
			bool alreadyLoaded = false;
			foreach(PlayerComponent player in _playerManager.GetPlayers()){
				if(player.getUserName().Equals(profileName)){
					alreadyLoaded = true;
				}
			}
			if(alreadyLoaded){
				skipped++;
				continue;
			}
			GameObject loadProfileButton = (GameObject)Instantiate(_loadProfileButtonPrefab, _loadProfileButtonPrefab.GetComponent<RectTransform>().position, Quaternion.identity);
			loadProfileButton.transform.SetParent( _availableProfilesPanel.transform );
			loadProfileButton.GetComponent<RectTransform>().anchoredPosition3D = _loadProfileButtonPrefab.GetComponent<RectTransform>().anchoredPosition3D - (Vector3.up * buttonOffset * (i-skipped));
			Text t = loadProfileButton.transform.GetChild(0).GetComponent<Text>();
			t.text = profileName;
			Debug.Log(t.text);
			Button b = loadProfileButton.GetComponent<Button>();
			string username = t.text;
			b.onClick.AddListener(() => { 
				for(int j = 0; j < _menus.Length; ++j){
					if( _menus[j].name.Equals("Menu_Title") 
					   || _menus[j].name.Contains("Menu_Background")
					   || _menus[j].name.Contains("Menu_LoadedProfiles")){
						_menus[j].SetActive(true);
					}
					else{
						_menus[j].SetActive(false);
					}
				}
				PlayerComponent player = new PlayerComponent("blank", 0, 0);
				player.Load(username);
				_playerManager.AddPlayer(player);
				_playerManager.UpdateProfileDisplay(false);
			});
			_loadProfileButtons.Add ( loadProfileButton );
		}
		
		foreach (GameObject g in _cameras)
		{
			if (g.name.Contains ("zoom"))
			{
				_initZoomCameraPos = g.transform.position;
			}
		}
	}

	public void SetText(Text uiText, string newText){
		uiText.text = newText;
	}

	#region TitleMenu Profile Display
	public void DisplayProfileInfo(PlayerComponent profile){
		for(int i = 0; i < _inGamePanels.Length; ++i){
			if(_inGamePanels[i].name.Contains("Display_Profile")){
				GameObject infoPanel = _inGamePanels[i].transform.GetChild(1).gameObject;
				infoPanel.transform.GetChild(0).GetComponent<Text>().text = profile.getUserName();
				infoPanel.transform.GetChild(2).GetComponent<Text>().text = profile.getWins() + "/" + (profile.getWins() + profile.getLosses());
				_inGamePanels[i].SetActive(true);
			}
		}
	}

	public void HideProfileInfo(){
		for(int i = 0; i < _inGamePanels.Length; ++i){
			if(_inGamePanels[i].name.Contains("Display_Profile")){
				_inGamePanels[i].SetActive(false);
			}
		}
	}

	public void HideLoadedProfilePanel(){
		for(int i = 0; i < _menus.Length; ++i){
			if(_menus[i].name.Contains("Menu_LoadedProfiles")){
				_menus[i].SetActive(false);
			}
		}
	}

	public void DisplayLoadedProfilePanel(){
		for(int i = 0; i < _menus.Length; ++i){
			if(_menus[i].name.Contains("Menu_LoadedProfiles")){
				_menus[i].SetActive(true);
			}
		}
	}

	#endregion TitleMenu Profile Display
	
	// Update is called once per frame
	void Update () {
		if(_currentMap != null && _currentMap.Contains("Parallel")){
			_cameras[0].transform.position = new Vector3(1.5f,50.0f,26.25f);
		}
	}

	public bool getCanClickGame() {
		return _canClickGame;
	}

	public void setCanClickGame(bool canClickGame) {
		_canClickGame = canClickGame;
	}

	#region ResourcesPanel

    public void setWoodStock(int stock)
    {
        for (int i = 0; i < _inGamePanels.Length; ++i)
        {
            if (_inGamePanels[i].name.Contains("Panel_Resources"))
            {
                //_inGamePanels[i].SetActive(true);
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

	public void setGoldStock(int stock)
	{
		for (int i = 0; i < _inGamePanels.Length; ++i)
		{
			if (_inGamePanels[i].name.Contains("Panel_Resources"))
			{
				//_inGamePanels[i].SetActive(true);
				for (int j = 0; j < _inGamePanels[i].transform.childCount; j++)
				{
					if (_inGamePanels[i].transform.GetChild(j).name.Contains("Text_GoldCount"))
					{
						_inGamePanels[i].transform.GetChild(j).GetComponent<Text>().text = stock.ToString();
					}
				}
			}
		}
	}

    public void setUpgradeVillageButtonCosts(VillageType village)
    {
        foreach (GameObject button in _inGameButtons)
        {
            if (button.name.Contains("Button_Upgrade_Village"))
            {
                switch (village)
                {
                    case VillageType.HOVEL:
                        button.transform.GetChild(0).GetComponent<Text>().text = "Upgrade (8)";
                        break;
                    case VillageType.TOWN:
                        button.transform.GetChild(0).GetComponent<Text>().text = "Upgrade (8)";
                        break;
                    case VillageType.FORT:
                        button.transform.GetChild(0).GetComponent<Text>().text = "Upgrade \n(12G, 12W)";
                        break;
                    case VillageType.CASTLE:
                        button.transform.GetChild(0).GetComponent<Text>().text = "Upgrade";
                        break;
                }
            }
        }
    }

    public void setUpgradeUnitButtonCosts(UnitType unit) {
        switch(unit) {
            case UnitType.PEASANT:
                updateButtonContents("Infantry (10 G)", "Soldier (20 G)", "Knight (30 G)");
                break;
            case UnitType.INFANTRY:
                updateButtonContents("Infantry", "Soldier (10 G)", "Knight (20 G)");
                break;
            case UnitType.SOLDIER:
                updateButtonContents("Infantry", "Soldier", "Knight (10 G)");
                break;
            case UnitType.KNIGHT:
                updateButtonContents("Infantry", "Soldier", "Knight");
                break;
        }
    }

    public void updateButtonContents(string b1, string b2, string b3) {
        foreach (GameObject button in _inGameButtons)
        {
            if (button.name.Contains("Button_Infantry"))
            {
                button.transform.GetChild(0).GetComponent<Text>().text = b1;
            }
            else if (button.name.Contains("Button_Soldier"))
            {
                button.transform.GetChild(0).GetComponent<Text>().text = b2;
            }
            else if (button.name.Contains("Button_Knight"))
            {
                button.transform.GetChild(0).GetComponent<Text>().text = b3;
            }
        }
    }

	public void hideResourcesPanel()
	{
		foreach (GameObject panel in _inGamePanels)
		{
			if (panel.name.Contains("Resources"))
			{
				panel.SetActive(false);
			}
		}
	}

	public void showResourcesPanel()
	{
		foreach (GameObject panel in _inGamePanels)
		{
			if (panel.name.Contains("Resources"))
			{
				panel.SetActive(true);
			}
		}
	}

	#endregion 

	public void DisplayInGameMenu(){
		if(Network.isServer || Network.isClient){
			networkView.RPC("RPCDisplayInGameMenu", RPCMode.All);
		}
		else{
			RPCDisplayInGameMenu();
		}
	}

    public void DisplayErrorMessage(string message)
    {
        var errorPanel = new GameObject();

        for (int i = 0; i < _inGamePanels.Length; ++i)
        {
            if (_inGamePanels[i].name.Contains("Panel_ErrorMessage"))
            {
                errorPanel = _inGamePanels[i];
            }
        }

        for (int i = 0; i < errorPanel.transform.childCount; i++ )
        {
            if (errorPanel.transform.GetChild(i).name.Contains("ErrorMessage"))
            {
                errorPanel.transform.GetChild(i).GetComponent<Text>().text = message;
            }
        }
        errorPanel.SetActive(true);
        StartCoroutine("delayErrorDisappear");
    }

    public void DisappearErrorMessage()
    {
        var errorPanel = new GameObject();

        for (int i = 0; i < _inGamePanels.Length; ++i)
        {
            if (_inGamePanels[i].name.Contains("Panel_ErrorMessage"))
            {
                errorPanel = _inGamePanels[i];
            }
        }
        errorPanel.SetActive(false);
    }

    IEnumerator delayErrorDisappear()
    {
        yield return new WaitForSeconds(_errorFadeSpeed);
        DisappearErrorMessage();
    }

	[RPC]
	private void RPCDisplayInGameMenu(){
		for(int i = 0; i < _menus.Length; ++i){
			if(_menus[i].name.Contains("Menu_InGame")){
				_menus[i].SetActive(true);
				foreach (GameObject panel in _inGamePanels)
				{
					if (panel.name.Contains("Resources"))
				    {
						panel.SetActive(false);
					}
				}
			}
			else{
				_menus[i].SetActive(false);
			}
		}
	}

	#region ActionPanels

	public void HideOtherActionPanels(string s)
	{
		if (s == string.Empty)
		{
			foreach (GameObject g in _actionPanels)
			{
				g.SetActive(false);
			}
			return;
		}
		foreach (GameObject g in _actionPanels)
		{
			if (!g.name.Contains (s))
			{
				g.SetActive(false);
			}
		}
	}

    public void DisplayStructureActions()
    {
        for (int i = 0; i < _inGamePanels.Length; ++i)
        {
            if (_inGamePanels[i].name.Contains("Panel_Structure_Actions"))
            {
                _inGamePanels[i].SetActive(true);
				HideOtherActionPanels("Structure_Actions");
				break;
            }
        }
    }

    public void HideStructureActions()
    {
        for (int i = 0; i < _inGamePanels.Length; ++i)
        {
            if (_inGamePanels[i].name.Contains("Panel_Structure_Actions"))
            {
                _inGamePanels[i].SetActive(false);
            }
        }
    }
	
	public void DisplayVillageActions(VillageComponent village){
		for(int i = 0; i < _inGamePanels.Length; ++i){
			if(_inGamePanels[i].name.Contains("Panel_Village_Actions")){
				_inGamePanels[i].SetActive(true);
				HideOtherActionPanels("Village_Actions");

				foreach (Button b in _inGamePanels[i].GetComponentsInChildren<Button>() as Button[])
				{
					if (b.name.Contains ("Upgrade"))
					{
						if (village.getVillageType() == VillageType.CASTLE || village.getWoodStock() < 8 ||
						    (village.getVillageType() == VillageType.FORT && village.getWoodStock() < 12))
						{
							DisableButton(b);
						}
					}
					else if (b.name.Contains ("Hire"))
					{
						if (village.getGoldStock() < 10)
						{
							DisableButton(b);
						}
					}
					else if (b.name.Contains ("Watchtower"))
					{
						if (village.getVillageType() == VillageType.HOVEL || village.getWoodStock() < 5)
						{
							DisableButton(b);
						}
					}
				}
			}
//			else if (!(_inGamePanels[i].name.Contains("CurrentPlayer")))
//			{
//				_inGamePanels[i].SetActive(false);
//				foreach (Button b in _inGamePanels[i].GetComponentsInChildren<Button>() as Button[])
//				{
//					EnableButton(b);
//				}
//			}
		}
	}

    public void DisplayUpgradeMenu()
    {
            // TODO: grey out options based on last selected unit type
    }

    public void HideVillageActions()
    {
        for (int i = 0; i < _inGamePanels.Length; ++i)
        {
            if (_inGamePanels[i].name.Contains("Panel_Village_Actions") || _inGamePanels[i].name.Contains("Panel_HireUnit_Village") ||
                _inGamePanels[i].name.Contains("Panel_Upgrade_Village"))
            {
				foreach (Button b in _inGamePanels[i].GetComponentsInChildren<Button>() as Button[])
				{
					EnableButton(b);
				}
                _inGamePanels[i].SetActive(false);
            }
        }
    }

	public void DisplayUnitActions(UnitType unit){
		for(int i = 0; i < _inGamePanels.Length; ++i){
            if (_inGamePanels[i].name.Contains("Panel_Unit_Actions"))
            {
                _inGamePanels[i].SetActive(true);
				switch (unit)
				{
                case UnitType.PEASANT:
                        foreach (Button b in _inGamePanels[i].GetComponentsInChildren<Button>() as Button[])
                        {
                            if (b.name.Contains("OpenFire"))
                            {
                                DisableButton(b);
                            }                            
                        }
                    break;
				case UnitType.INFANTRY:
					foreach (Button b in _inGamePanels[i].GetComponentsInChildren<Button>() as Button[])
					{
                        if (b.name.Contains("Harvest") || b.name.Contains("OpenFire") || b.name.Contains("Build"))
						{

							DisableButton(b);
						}
						
					}
					break;
				case UnitType.SOLDIER:
					foreach (Button b in _inGamePanels[i].GetComponentsInChildren<Button>() as Button[])
					{
                        if (b.name.Contains("Harvest") || b.name.Contains("OpenFire") || b.name.Contains("Build"))
						{
							DisableButton(b);
						}
						
					}
					break;
				case UnitType.KNIGHT:
					foreach (Button b in _inGamePanels[i].GetComponentsInChildren<Button>() as Button[])
					{
						if (b.name.Contains("Harvest") || b.name.Contains ("Upgrade") || b.name.Contains("Build") || b.name.Contains ("OpenFire"))
						{
							DisableButton(b);
						}
					}
                    break;
                case UnitType.CANNON:
                    foreach (Button b in _inGamePanels[i].GetComponentsInChildren<Button>() as Button[])
                    {
                        if (b.name.Contains("Harvest") || b.name.Contains("Upgrade") || b.name.Contains("Build") || b.name.Contains("Merge"))
                        {
                            DisableButton(b);
                        }
                    }
                    break;
				default:
					break;
				}
				HideOtherActionPanels("Unit_Actions");

			}
		}
	}

	#endregion

    public void HideUnitActions()
    {
        for (int i = 0; i < _inGamePanels.Length; ++i)
        {
            if (_inGamePanels[i].name.Contains("Panel_Unit_Actions"))
            {
				foreach (Button b in _inGamePanels[i].GetComponentsInChildren<Button>() as Button[])
				{
					EnableButton(b);
				}
                _inGamePanels[i].SetActive(false);
            }
        }
    }

	void DisableButton(Button b)
	{
		ColorBlock cb = b.colors;
		cb.normalColor = Color.grey;
		cb.pressedColor = Color.grey;
		cb.highlightedColor = Color.grey;
		b.colors = cb;
		b.interactable = false;
	}
	
	void EnableButton(Button b)
	{
		b.interactable = true;
		b.colors = _standardButtonColors;
	}

	#region EndTurn 

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
						break;
					}
				}
				break;
			}
		}
		DisplayTurnPanel(currentPlayer, currentColor);
	}

	/// <summary>
	/// Displays the panel indicating next player to get a turn.
	/// </summary>
	/// <param name="currentPlayer">Current player.</param>
	/// <param name="currentColor">Current color.</param>
	public void DisplayTurnPanel(PlayerComponent currentPlayer, Color currentColor)
	{
		GameObject panel;
		foreach (GameObject g in _inGamePanels)
		{
			if (g.name.Contains ("Panel_PlayerTurn"))
			{
				panel = g;
				g.SetActive(true);
				Text[] playerTexts = g.GetComponentsInChildren<Text>() as Text[];
				foreach (Text t in playerTexts)
				{
					if (t.name.Contains ("CurrentPlayer"))
					{
						t.text = currentPlayer.getUserName();
						t.color = currentColor;
						break;
					}
				}
				StartCoroutine ("DelayStartFade", panel);
				break;
			}
            if (g.name.Contains("Button_End_Turn"))
            {
                if (currentPlayer.Equals(GameObject.FindObjectOfType<GameComponent>().getCurrentPlayer()))
                {
                    g.SetActive(true);
                }
                else
                {
                    g.SetActive(false);
                }
                
            }
		}
	}
	
	IEnumerator DelayStartFade(GameObject panel)
	{
		yield return new WaitForSeconds(_fadeSpeed);
		panel.SetActive(false);
	}

	/// <summary>
	/// Switches to zoomed out camera if current player ends turn while zoomed in.
	/// </summary>
	public void SwitchCameras()
	{
		if (_cameras[0].name.Contains ("main"))
		{
			if (!_cameras[0].activeSelf)
			{
				_cameras[1].transform.position = _initZoomCameraPos;
				_cameras[1].SetActive (false);
				_cameras[0].SetActive (true);
				_currentMap = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameComponent>()._currentMap;
				if(_currentMap.Contains("Parallel")){
					_cameras[0].transform.position = new Vector3(1.5f,50.0f,26.25f);
				}
			}
		}
		else 
		{
			if (!_cameras[1].activeSelf)
			{
				_cameras[0].transform.position = _initZoomCameraPos;
				_cameras[0].SetActive (false);
				_cameras[1].SetActive (true);
			}
		}
	}

	#endregion

	#region EveryRound

	/// <summary>
	/// Displays the "current round" panel.
	/// </summary>
	/// <param name="round">Round.</param>
	public void DisplayRoundPanel (int round)
	{
		GameObject panel;
		foreach (GameObject g in _inGamePanels)
		{
			if (g.name.Contains ("Panel_CurrentRound"))
			{
				panel = g;
				g.SetActive(true);
				Text[] ts = g.GetComponentsInChildren<Text>() as Text[];
				foreach (Text t in ts)
				{
					t.text = "Round " + round; 
				}
				StartCoroutine ("DelayStartFade", panel);
				break;
			}
		}
	}

	#endregion

	public void DisplaySavedOfflineGames() {
		foreach (GameObject g in _menus) {
			if (g.name.Contains("Offline_SavedGames")) {
				Button[] bs = g.GetComponentsInChildren<Button>() as Button[];
				string[] mapNames = new string[] { "Square", "Parallel", "Holy" };
				foreach (string mapName in mapNames) {
					foreach (Button b in bs) {
						if (b.name.Contains(mapName)) {
							if (File.Exists(_mapDirectory + mapName + "Info.dat")) {
								EnableButton(b);
								b.GetComponentInChildren<Text>().text = "Saved " + mapName + " Grid";
							}
							else {
								DisableButton(b);
								b.GetComponentInChildren<Text>().text = "No " + mapName + " Save";
							}
							break;
						}
					}
				}
			}
		}
	}

	public void DisplaySavedOnlineGames() {
		foreach (GameObject g in _menus) {
			if (g.name.Contains("Online_SavedGames")) {
				Button[] bs = g.GetComponentsInChildren<Button>() as Button[];
				string[] mapNames = new string[] { "Square", "Parallel", "Holy" };
				foreach (string mapName in mapNames) {
					foreach (Button b in bs) {
						if (b.name.Contains(mapName)) {
							if (File.Exists(_mapDirectory + mapName + "Info.dat")) {
								EnableButton(b);
								b.GetComponentInChildren<Text>().text = "Saved " + mapName + " Grid";
							}
							else {
								DisableButton(b);
								b.GetComponentInChildren<Text>().text = "No " + mapName + " Save";
							}
							break;
						}
					}
				}
			}
		}
	}

	public void InitializeSettingsPanel(List<PlayerComponent> players)
	{
		foreach (GameObject g in _inGamePanels)
		{
			if (g.name.Contains ("Settings"))
			{
				int i = 0;
				Text[] ts = g.GetComponentsInChildren<Text>() as Text[];
				foreach (Text t in ts)
				{
					if (t.name.Contains("Text_Player"))
					{
						if (players[i] != null)
						{
							t.text = players[i].getUserName();
						}
						else 
						{
							break;
						}
					}
				}				
				break;
			}
		}
	}

	public void UpdateSettingsPanel (int eliminatedIndex)
	{
		foreach (GameObject g in _inGamePanels)
		{
			if (g.name.Contains ("Settings"))
			{
				Text[] ts = g.GetComponentsInChildren<Text>() as Text[];
				foreach (Text t in ts)
				{
					if (t.name.Contains(eliminatedIndex.ToString()))
					{
						t.color = Color.black;
					}
				}				
				break;
			}
		}
	}

    public void displayCurrentSelection(string selection)
    {
        for (int i = 0; i < _menus.Length; ++i)
        {
            if (_menus[i].name.Contains("Menu_InGame"))
            {
                foreach (GameObject panel in _inGamePanels)
                {
                    if (panel.name.Contains("CurrentSelection"))
                    {
                        panel.transform.GetChild(0).GetComponent<Text>().text = selection;
                        panel.SetActive(true);

                    }
                }
            }
        }
    }

	#region win/lose
	public void displayWin(string playerName) {
		displayEndPanel("Win", playerName);
	}

    public void displayLose(string playerName)
    {
		displayEndPanel("Lose", playerName);
	}

	private void displayEndPanel(string endType, string playerName) {
		for (int i = 0; i < _menus.Length; ++i) {
			if (_menus[i].name.Contains("Menu_InGame")) {
				foreach (GameObject panel in _inGamePanels) {
                    if (panel.name.Contains(endType))
                    {
                        panel.transform.GetChild(0).GetComponent<Text>().text = "You " + endType + ", " + playerName;
						panel.SetActive(true);
                        
					}
				}
			}
		}
	}
	#endregion
}
