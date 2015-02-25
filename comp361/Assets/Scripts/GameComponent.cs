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

	PlayerComponent[] getRemainingPlayers() {
		return _remainingPlayers;
	}

	void setRemainingPlayers(PlayerComponent[] remainingPlayers) {
		_remainingPlayers = remainingPlayers;
	}

	/*********************
	 *      METHODS      *
	 ********************/

	void endGame() {
		/* TODO */
	}

	void newGame(PlayerComponent[] participants) {
		/* TODO */
	}

	void removePlayer(PlayerComponent player) {
		/* TODO */
	}



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
