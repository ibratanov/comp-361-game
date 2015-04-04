using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

	List<PlayerComponent> _players = new List<PlayerComponent>();
	private List<GameObject> _profileButtons = new List<GameObject>();

	//Temporary variables for use in New Profile menu
	public Text _inputUsername;
	public Text _inputPassword;

	//UI
	public GUIManager _guiManager;
	public GameObject _displayInfoButton;
	public GameObject _removeProfileButton;
	public GameObject _loadedProfilesMenu;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

    public List<PlayerComponent> GetPlayers()
    {
        return _players;
    }

	public void AddPlayer(PlayerComponent playerProfile){
		_players.Add(playerProfile);
	}

	public void AddPlayer(string username, int wins, int losses){
		bool alreadyExists = false;
		for(int i = 0; i < _players.Count; ++i){
			if(_players[i].getUserName().Equals(username)){
				alreadyExists = true;
			}
		}
		if(!alreadyExists){
			PlayerComponent player = new PlayerComponent(username, wins, losses);
			AddPlayer(player);
		}
	}

	public void RemovePlayer(PlayerComponent profile){
		_players.Remove(profile);
	}

	public void RemovePlayer(string username){
		for(int i = 1; i < _players.Count; ++i){
			if(_players[i].getUserName().Equals(username)){
				RemovePlayer(_players[i]);
			}
		}
	}

	public PlayerComponent GetPlayer(int index){
		return (PlayerComponent)_players[index];
	}

	#region New Profile Menu commands

	/// <summary>
	/// Sets the current username in New Profile menu.
	/// </summary>
	public void SetUsername(Text name){
		_inputUsername = name;
	}

	/// <summary>
	/// Sets the current password in New Profile menu.
	/// </summary>
	public void SetPassword(Text password){
		_inputPassword = password;
	}

	/// <summary>
	/// Generates a new PlayerComponent and adds it to the PlayerManager.
	/// </summary>
	public void SaveNewPlayer(){
		if(_inputUsername && _inputUsername.text.Length > 0){
			PlayerComponent newPlayer = new PlayerComponent(_inputUsername.text, 0, 0);
			_players.Add (newPlayer);
			Debug.Log ("New player added:" + newPlayer.getUserName());
			//Clear the temporary information
			_inputUsername = null;
			_inputPassword = null;
		}
		else{
			//TODO: Create error message for user
		}
	}

	#endregion

	public void UpdateProfileDisplay(bool removing){
		foreach(GameObject button in _profileButtons){
			Destroy (button);
		}
		_profileButtons.Clear();
		int buttonOffset = -40;
		if(!removing){
			for(int i = 0; i < _players.Count; ++i){
				GameObject profileButton = (GameObject)Instantiate(_displayInfoButton, _displayInfoButton.GetComponent<RectTransform>().position, Quaternion.identity);
				profileButton.transform.SetParent( _loadedProfilesMenu.transform );
				profileButton.GetComponent<RectTransform>().anchoredPosition3D = _displayInfoButton.GetComponent<RectTransform>().anchoredPosition3D + (Vector3.up * buttonOffset * i);
				Button b = profileButton.GetComponent<Button>();
				PlayerComponent profile = _players[i];
				b.onClick.AddListener(() => {
					_guiManager.DisplayProfileInfo(profile);
				});
				Text t = profileButton.transform.GetChild(0).GetComponent<Text>();
				t.text = _players[i].getUserName();
				_profileButtons.Add ( profileButton );
			}
		}
		else{
			for(int i = 0; i < _players.Count; ++i){
				GameObject profileButton = (GameObject)Instantiate(_removeProfileButton, _removeProfileButton.GetComponent<RectTransform>().position, Quaternion.identity);
				profileButton.transform.SetParent( _loadedProfilesMenu.transform );
				profileButton.GetComponent<RectTransform>().anchoredPosition3D = _removeProfileButton.GetComponent<RectTransform>().anchoredPosition3D + (Vector3.up * buttonOffset * i);
				Button b = profileButton.GetComponent<Button>();
				PlayerComponent profile = _players[i];
				b.onClick.AddListener(() => {
					this.RemovePlayer(profile);
					this.UpdateProfileDisplay(true);
				});
				Text t = profileButton.transform.GetChild(0).GetComponent<Text>();
				t.text = _players[i].getUserName();
				_profileButtons.Add ( profileButton );
			}
		}
	}
}
