//From the tutorial: http://www.paladinstudios.com/2013/07/10/how-to-create-an-online-multiplayer-game-with-unity/

using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	//Server creation
	private const string projectName = "Comp361_RITAGame"; //Should be unique to this project
	private const string roomName = "testRoom"; //Can be any name (possible suggestion: player name)
	private const int maxPlayers = 4;
	private const int portNumber = 25602;

	//Server joining
	private HostData[] hostList;

// --- INTERFACE CREATION --- //
	#region CREATE INTERFACE

	//Create a GUI display
	void OnGUI()
	{
		//If this game instance has not connect to a server nor created one yet
		if(!Network.isClient && !Network.isServer)
		{
			if(GUI.Button(new Rect(100,100,250,100), "Start Server")){
				StartServer();
			}
			if(GUI.Button(new Rect(100,250,250,100), "Refresh Hosts")){
				RefreshHostList();
			}
			if(hostList != null){
				for(int i = 0; i < hostList.Length; ++i){
					if(GUI.Button(new Rect(400,100+(110*i),300,100), hostList[i].gameName)){
						JoinServer(hostList[i]);
					}
				}
			}
		}
	}

	#endregion CREATE INTERFACE

	#region CREATE SERVER

	//Create a server and register it to the Master Server
	private void StartServer(){
		Network.InitializeServer(maxPlayers, portNumber, !Network.HavePublicAddress());
		MasterServer.RegisterHost(projectName, roomName, "This is a comment");
//		MasterServer.ipAddress = "127.0.0.1"; //Testing locally - TODO: Remove this in final implementation
	}

	//Confirmation that the server has indeed been created.
	void OnServerInitialized(){
		Debug.Log("Server Initialized");
	}

	#endregion CREATE SERVER

	#region JOIN SERVER

	//Get the host information that matches the gameTypeName.
	private void RefreshHostList(){
		MasterServer.RequestHostList(projectName);
		Debug.Log(MasterServer.PollHostList().Length);
	}

	//Event triggered on Master Server
	void OnMasterServerEvent(MasterServerEvent msEvent){
		//Confirm the server was registered
		if(msEvent == MasterServerEvent.RegistrationSucceeded){
			Debug.Log("msEvent equalled RegistrationSucceeded");
		}

		Debug.Log("msEvent: " + msEvent.ToString());
		//Obtain data required to join the host server
		if(msEvent == MasterServerEvent.HostListReceived){
			hostList = MasterServer.PollHostList();
			Debug.Log("msEvent equalled HostListReceived");
		}
	}

	//Join the server belonging to the given hostData
	private void JoinServer(HostData hostData){
		Network.Connect(hostData);
		Debug.Log("trying to join " + hostData.gameName);
	}

	//Confirmation that the server has indeed been created.
	void OnConnectedToServer(){
		Debug.Log("Server Joined");
	}

	#endregion JOIN SERVER

}
