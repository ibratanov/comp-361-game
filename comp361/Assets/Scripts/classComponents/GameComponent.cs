using UnityEngine;
using System.Collections;

public class GameComponent : MonoBehaviour {
	private Color[] _playerColours = {Color.red, Color.green, Color.blue, Color.yellow};

	/*********************
	 *     ATTRIBUTES    *
	 ********************/
	public PlayerManager _playerManager;

	PlayerComponent _currentPlayer;
	PlayerComponent[] _participants;
	PlayerComponent[] _remainingPlayers;
	TileComponent[,] _mapTiles;

	/*********************
	 *  GETTERS/SETTERS  *
	 ********************/

	void setCurrentPlayer(PlayerComponent currentPlayer) {
		_currentPlayer = currentPlayer;
	}

	public PlayerComponent[] getRemainingPlayers() {
		return _remainingPlayers;
	}

	void setRemainingPlayers(PlayerComponent[] remainingPlayers) {
		_remainingPlayers = remainingPlayers;
	}

	/*********************
	 *      METHODS      *
	 ********************/

	public void endGame() {
		/* TODO */
	}

	public void newGame(PlayerComponent[] participants) {
		var firstPlayer = participants[0];
		setRemainingPlayers(participants);
		setCurrentPlayer(firstPlayer);
		
		MapGenerator m = this.GetComponent<MapGenerator>();
		m.GenerateMap();
		
		_mapTiles = m.getLandTiles();
		
		foreach (var tile in _mapTiles)
		{
			int randIndex = Random.Range(0, participants.Length*7);
			tile.setInitialPlayerIndex(randIndex%participants.Length); //TODO: Verify there is no off-by-one error
		}
		
		foreach (var tile in _mapTiles)
		{
			int playerIndex = tile.getInitialPlayerIndex();
			//tile.getGameObject().GetComponent<Renderer>().materials[2].color = Color.white;
			
			if (playerIndex < participants.Length)
			{
				var region = tile.breadthFS();
				if (region.Length < 3)
				{
					foreach (var rTile in region)
					{
						rTile.setInitialPlayerIndex(participants.Length);
					}
				}
				else
				{
					bool regionContainsVillage = false;
					foreach (var rTile in region)
					{
						var regOccupant = rTile.getOccupantType();
						if(regOccupant == OccupantType.VILLAGE){
							regionContainsVillage = true;
						}
						//rTile.getGameObject().GetComponent<Renderer>().materials[2].color = _playerColours[playerIndex];
					}
					if (!regionContainsVillage)
					{
						/*TODO: Add this back in once the functions are properly finished
					   	var player = participants[playerIndex];
	                   	var newHovel = new VillageComponent(VillageType.HOVEL);
	                   	player.add(newHovel);
						TileComponent regionTile = region[0];
						newHovel.associate(regionTile);
						newHovel.addGold(7);
	                   	UnitComponent newPeasant = newHovel.hireVillager(UnitType.PEASANT);
	                   	TileComponent villagerTile = region[1];
	                   	newPeasant.associate(villagerTile);
*/
					}
				}
			}
		}
	}

	public void removePlayer(PlayerComponent player) {
		/* TODO */
	}



	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// Easy function to be called by menu button.
	/// </summary>
	public void BeginGame(){
		PlayerComponent[] players = _playerManager.GetPlayers();
		newGame(players);
	}

	public void TestMapGeneration(){
		_playerManager.AddPlayer(new PlayerComponent("Rita", "rita"));
		_playerManager.AddPlayer(new PlayerComponent("Rita2", "rita2"));
		newGame(_playerManager.GetPlayers());
	}
}
