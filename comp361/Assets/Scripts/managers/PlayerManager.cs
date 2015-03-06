using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {
	List<PlayerComponent> _players = new List<PlayerComponent>();

	//Temporary variables for use in New Profile menu
	public Text _inputUsername;
	public Text _inputPassword;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public PlayerComponent[] GetPlayers()
    {
        return _players.ToArray();
    }

	public void AddPlayer(PlayerComponent playerProfile){
		_players.Add(playerProfile);
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
		if(_inputUsername.text.Length > 0 && _inputPassword.text.Length > 0){
			PlayerComponent newPlayer = new PlayerComponent(_inputUsername.text, _inputPassword.text);
			_players.Add (newPlayer);
			Debug.Log ("New player added:" + newPlayer.getUserName());
			//Clear the temporary information
			_inputUsername = null;
			_inputPassword = null;
		}
	}

	#endregion
}
