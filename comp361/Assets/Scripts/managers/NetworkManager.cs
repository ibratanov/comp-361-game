//From the tutorial: http://www.paladinstudios.com/2013/07/10/how-to-create-an-online-multiplayer-game-with-unity/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {

	//Server creation
	private const string _projectName = "Comp361_RITAGame"; //Should be unique to this project
	private string _roomName = "testRoom"; //Can be any name (possible suggestion: player name)
	private const int _maxPlayers = 6;
	private const int _portNumber = 25602;

	//Server joining
	private HostData[] _hostList;
	private bool _refreshHostList = false;
	public GameObject _joinAvailablePanel;		//The panel which becomes the parent of the buttons.
	public GameObject _joinButtonPrefab;		//The prefab of the button to create instances of.
	public GameObject _currentMenu;				//The menu that this button lives on.
	public GameObject _connectedMenu;			//The menu that the player is redirected to after a successful connection.
	private List<GameObject> _sessionButtons = new List<GameObject>();
	public PlayerManager _playerManager;

	public void SetRoomNameToFirstPlayer(){
		_roomName = _playerManager.GetPlayer(0).getUserName();
	}
// --- INTERFACE CREATION --- //
	#region CREATE INTERFACE

	void Start(){

	}

	void Update(){
		if(_refreshHostList){
			RefreshHostList();
			if(_hostList != null){
				int buttonOffset = -40;
				for(int i = 0; i < _hostList.Length; ++i){
					GameObject sessionButton = (GameObject)Instantiate(_joinButtonPrefab, _joinButtonPrefab.GetComponent<RectTransform>().position, Quaternion.identity);
					sessionButton.transform.SetParent( _joinAvailablePanel.transform );
					sessionButton.GetComponent<RectTransform>().anchoredPosition3D = _joinButtonPrefab.GetComponent<RectTransform>().anchoredPosition3D + (Vector3.up * buttonOffset * i);
					Button b = sessionButton.GetComponent<Button>();
					int hostIndex = i;
					b.onClick.AddListener(() => {
						JoinServer(_hostList[hostIndex]); 
						_currentMenu.SetActive(false);
						_connectedMenu.SetActive(true);
						_refreshHostList = false;
					});
					Text t = sessionButton.transform.GetChild(0).GetComponent<Text>();
					t.text = _hostList[i].gameName;
					_sessionButtons.Add ( sessionButton );
					_refreshHostList = false;
				}
			}
		}
	}

	public void StartRefreshingHostList(){
		_refreshHostList = true;
	}

	public void StopRefreshingHostList(){
		_refreshHostList = false;
	}

	#endregion CREATE INTERFACE

	#region CREATE SERVER

	//Create a server and register it to the Master Server
	public void StartServer(){
		if(!Network.isClient && !Network.isServer){
			Network.InitializeServer(_maxPlayers, _portNumber, !Network.HavePublicAddress());
			MasterServer.RegisterHost(_projectName, _roomName, "This is a comment");
		}
	}

	//Confirmation that the server has indeed been created.
	void OnServerInitialized(){
		Debug.Log("Server Initialized");
	}

	#endregion CREATE SERVER

	#region JOIN SERVER

	//Get the host information that matches the gameTypeName.
	public void RefreshHostList(){
		MasterServer.RequestHostList(_projectName);
		Debug.Log(MasterServer.PollHostList().Length);
		foreach(GameObject button in _sessionButtons){
			Destroy(button);
		}
		_sessionButtons.Clear();
	}

	//Event triggered on Master Server
	void OnMasterServerEvent(MasterServerEvent msEvent){
		//Confirm the server was registered
		if(msEvent == MasterServerEvent.RegistrationSucceeded){
			Debug.Log("RegistrationSucceeded");
		}

		Debug.Log("msEvent: " + msEvent.ToString());
		//Obtain data required to join the host server
		if(msEvent == MasterServerEvent.HostListReceived){
			_hostList = MasterServer.PollHostList();
			Debug.Log("HostListReceived");
		}
	}

	//Join the server belonging to the given hostData
	public void JoinServer(HostData hostData){
		Network.Connect(hostData);
		Debug.Log("trying to join " + hostData.gameName);
	}

	//Confirmation that the server has indeed been created.
	void OnConnectedToServer(){
		Debug.Log("Server Joined");
		PlayerManager playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
		PlayerComponent player = playerManager.GetPlayer(0);
		networkView.RPC("UpdatePlayersJoined", RPCMode.Others, player.getUserName(), player.getWins(), player.getLosses());
	}

	//Send PlayerProfile information across the network
	[RPC]
	public void UpdatePlayersJoined(string username, int wins, int losses){
		_playerManager.AddPlayer(username, wins, losses);
		if(Network.isServer){
			List<PlayerComponent> players = _playerManager.GetPlayers();
			for(int i = 0; i < players.Count; ++i){
				networkView.RPC("UpdatePlayersJoined", RPCMode.Others, players[i].getUserName(), players[i].getWins(), players[i].getLosses());
			}
		}
		_playerManager.UpdateProfileDisplay(false);
	}

	#endregion JOIN SERVER

	public void Disconnect(){
		if(Network.isServer){
			Network.Disconnect();
			MasterServer.UnregisterHost();
			Debug.Log("Disconnected Server");
		}
		else if(Network.isClient){
			Network.Disconnect();
			Debug.Log("Disconnected from Server");
			networkView.RPC ("UpdatePlayersDisconnected", RPCMode.Others, _playerManager.GetPlayer(0).getUserName());
		}
	}

	//Send PlayerProfile information across the network
	[RPC]
	public void UpdatePlayersDisconnected(string username){
		_playerManager.RemovePlayer(username);
		_playerManager.UpdateProfileDisplay(false);
	}
}
