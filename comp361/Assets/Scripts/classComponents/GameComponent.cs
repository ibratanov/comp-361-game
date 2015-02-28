using UnityEngine;
using System.Collections;

public class GameComponent : MonoBehaviour {

	/*********************
	 *     ATTRIBUTES    *
	 ********************/

	PlayerComponent _currentPlayer;
	PlayerComponent[] _participants;
	PlayerComponent[] _remainingPlayers;
	TileComponent[] _mapTiles;

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
        Random r = new Random();
        for (int i = 0; i < 300; i++)
        {            
            TileComponent t = new TileComponent(r.Next(0, participants.Length + 1));
            _mapTiles.Add(t);
        }
        for (int i = 0; i < 60; i++)
        {
            TileComponent forestTile = _mapTiles[r.Next(0, 300)];
            forestTile.SetLandType(LandType.FOREST);
        }
        for (int i = 0; i < 60; i++)
        {
            TileComponent meadowTile = _mapTiles[r.Next(0, 300)];
            forestTile.setLandType(LandType.MEADOW);
        }
        foreach (var tile in _mapTiles)
        {
            int playerIndex = getInitialPlayerIndex();

            if (playerIndex < participants.Length)
            {
                var region = mapTiles.breadthFS();
                if (region.Length < 3)
                {
                    foreach (var rTile in region)
                    {
                        rTile.setInitializingValue(participants.Length);
                    }
                }
                else
                {
                    foreach (var rTile in region)
                    {
                       var regOccupant = rTile.getOccupantType();
                       if (regOccupant != OccupantType.VILLAGE)
                       {
                           var player = participants[playerIndex];
                           var newHovel = new VillageComponent(VillageType.HOVEL);
                           player.add(newHovel);
                           newHovel.associate(rTile);
                           newHovel.addGold(7);
                           var newPeasant = newHovel.hireVillager(UnitType.PEASANT);
                           int villagerTile = region[1];
                           newPeasant.associate(rTile);
                       }
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
}
