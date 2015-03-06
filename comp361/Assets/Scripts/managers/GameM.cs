using UnityEngine;
using System.Collections;

public class GameM : MonoBehaviour {

    public PlayerManager playerManager;

	// Use this for initialization
	void Start () {
        GameComponent g = this.GetComponent<GameComponent>();
        PlayerComponent[] p = new PlayerComponent[2] { new PlayerComponent("Rita", "rita"), new PlayerComponent("Rita2", "rita2") };
        g.newGame(p);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
