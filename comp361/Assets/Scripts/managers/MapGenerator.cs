using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {
	
	//public static MapGenerator _instance;
	
	public AssetManager _assets;
	public GameObject _gameTile;
	
	//public GameObject _playerManager;
	
	public Vector3 _origin;
	public int _rows = 17;
	public int _columns = 18;
	public float _forestRatio = 0.2f;
	public float _meadowRatio = 0.1f;
	public Vector3 _tileHeight = new Vector3(0, 0, 1.732f);
	public Vector3 _tileDiagonal = new Vector3(1.5f, 0, 0.866f);
	private TileComponent[,] _landTiles;
	private TileComponent[,] _waterTiles;
	private int _idCounter = 0;
	
	public void GenerateMap(){
        //TODO: Selecting which map you want
        GenerateSquareGridWithHoles(20, 20, _origin, _tileHeight, _tileDiagonal);
        //GenerateParalleloGrid(20, 20, _origin, _tileHeight, _tileDiagonal);
        //GenerateSquareGrid(20, 20, _origin, _tileHeight, _tileDiagonal);
		AddTerrain(_forestRatio, _meadowRatio);
	}
	
	/// <summary>
	/// Non-random grid creation. Creates just basic hex tiles in a rectangular formation.
	/// </summary>
	/// <param name="rows">The height of the grid.</param>
	/// <param name="columns">The width of the grid.</param>
	/// <param name="startLocation">The position to start creating the map from (bottom left corner).</param>
	/// <param name="tileHeight">The height of an individual tile.</param>
	/// <param name="tileDiagonal">The diagonal length of an individual tile.</param>
	private void GenerateSquareGrid( int rows, int columns, Vector3 startLocation, Vector3 tileHeight, Vector3 tileDiagonal ) 
	{
		if(Network.isServer){
			networkView.RPC("InitializeMapArray", RPCMode.Others, columns, rows);
		}
		InitializeMapArray(columns, rows);

		int gridId = 0;
		Vector3 currentPosition = startLocation;
		int nPlayers = this.GetComponent<GameComponent>()._playerManager.GetPlayers().Count;
		for (int i = 0; i < columns; ++i)
		{
			bool phase = false;
			currentPosition = startLocation + tileHeight * i;
			for (int j = 0; j < rows; ++j)
			{
				int playerIndex = Random.Range(0, nPlayers+1);
				if(Network.isServer){
					networkView.RPC("InstantiateTile", RPCMode.Others, currentPosition, i, j, playerIndex);
				}
				InstantiateTile(currentPosition, i, j, playerIndex);
				
				//Update position for next tile
				Vector3 diag = tileDiagonal;
				if(phase){
					diag.z *= -1; //We're drawing horizontally so it zigzags
				}
				phase = !phase;
				currentPosition += diag;
			}
		}

        // call generateNeighbors on each tile and remove random tiles
        for (int i = 0; i < columns; ++i)
		{
			for (int j = 0; j < rows; ++j)
			{
				if(Network.isServer){
					networkView.RPC("generateNeighbours", RPCMode.Others, i, j);
				}
				generateNeighbours(i, j);
			}
		}
	}

    private void GenerateSquareGridWithHoles(int rows, int columns, Vector3 startLocation, Vector3 tileHeight, Vector3 tileDiagonal)
    {
        if (Network.isServer)
        {
            networkView.RPC("InitializeMapArray", RPCMode.Others, columns, rows);
        }
        InitializeMapArray(columns, rows);

        int gridId = 0;
        Vector3 currentPosition = startLocation;
        int nPlayers = this.GetComponent<GameComponent>()._playerManager.GetPlayers().Count;
        for (int i = 0; i < columns; ++i)
        {
            bool phase = false;
            currentPosition = startLocation + tileHeight * i;
            for (int j = 0; j < rows; ++j)
            {
                int playerIndex = Random.Range(0, nPlayers + 1);
                if (Network.isServer)
                {
                    networkView.RPC("InstantiateTile", RPCMode.Others, currentPosition, i, j, playerIndex);
                }

                InstantiateTile(currentPosition, i, j, playerIndex);

                //Update position for next tile
                Vector3 diag = tileDiagonal;
                if (phase)
                {
                    diag.z *= -1; //We're drawing horizontally so it zigzags
                }
                phase = !phase;
                currentPosition += diag;
            }
        }

        // call generateNeighbors on each tile and remove random tiles
        for (int i = 0; i < columns; ++i)
        {
            for (int j = 0; j < rows; ++j)
            {
                int randomRemove = (i + j + System.DateTime.Now.Millisecond) % 7;
                if (randomRemove == 0)
                {
                    TileComponent tile = _landTiles[i, j].gameObject.GetComponent<TileComponent>();
                    tile.setPlayerIndex(-1);        
                    tile.setLandType(LandType.SEA);
                    Destroy(tile.getTerrainGameObject());
                }

                if (Network.isServer)
                {
                    networkView.RPC("generateNeighbours", RPCMode.Others, i, j);
                }
                generateNeighbours(i, j);
            }
        }
    }

    private void GenerateParalleloGrid(int rows, int columns, Vector3 startLocation, Vector3 tileHeight, Vector3 tileDiagonal)
    {
        if (Network.isServer)
        {
            networkView.RPC("InitializeMapArray", RPCMode.Others, columns, rows);
        }
        InitializeMapArray(columns, rows);

        int gridId = 0;
        Vector3 currentPosition = startLocation;
        int nPlayers = this.GetComponent<GameComponent>()._playerManager.GetPlayers().Count;
        for (int i = 0; i < columns; ++i)
        {
            bool phase = false;
            currentPosition = startLocation + tileHeight * i;
            int tileGenOffset = 200;
            for (int j = 0; j < rows; ++j)
            {
                int playerIndex = Random.Range(0, nPlayers + 1);
                if (Network.isServer)
                {
                    networkView.RPC("InstantiateTile", RPCMode.Others, currentPosition, i, j, playerIndex);
                }

                InstantiateTile(currentPosition, i, j, playerIndex);

                //Update position for next tile
                Vector3 diag = tileDiagonal;
                if (phase && tileGenOffset % 2 == 0) //Changes the shape
                {
                    diag.z *= -1; //We're drawing horizontally so it zigzags
                }
                phase = !phase;
                currentPosition += diag;
                tileGenOffset--;
            }
        }

        // call generateNeighbors on each tile and remove random tiles
        for (int i = 0; i < columns; ++i)
        {
            for (int j = 0; j < rows; ++j)
            {
                int randomRemove = (i + j + System.DateTime.Now.Millisecond) % 10;
                if (randomRemove == 0)
                {
                    TileComponent tile = _landTiles[i, j].gameObject.GetComponent<TileComponent>();
                    tile.setPlayerIndex(-1);
                    tile.setLandType(LandType.SEA);
                    Destroy(tile.getTerrainGameObject());
                }

                if (Network.isServer)
                {
                    networkView.RPC("generateNeighbours", RPCMode.Others, i, j);
                }
                generateNeighbours(i, j);
            }
        }
    }
	

	[RPC]
	private void InitializeMapArray(int columns, int rows){
		_landTiles = new TileComponent[columns, rows];
	}

	[RPC]
    private void InstantiateTile(Vector3 position, int i, int j, int playerIndex)
    {
            GameObject gameTile = (GameObject)Instantiate(_gameTile, position, Quaternion.identity);
            //gameTile.transform.parent = this.transform;
            _landTiles[i, j] = gameTile.GetComponent<TileComponent>();
            _landTiles[i, j].setID(_idCounter++);
            gameTile.transform.parent = this.transform; //Keep things organized with a parental hierarchy
            //		_landTiles[i,j].setTileGameObject(gameTile);
            _landTiles[i, j].setLandType(LandType.GRASS);
            int nPlayers = this.GetComponent<GameComponent>()._playerManager.GetPlayers().Count;
            _landTiles[i, j].setPlayerIndex(playerIndex);
    }

	[RPC]
	private void generateNeighbours(int i, int j)
	{
		List<TileComponent> n = new List<TileComponent>();
		
		if (i + 1 < _columns)
		{
			n.Add(_landTiles[i + 1, j]);
		}
		if (i - 1 >= 0)
		{
			n.Add(_landTiles[i - 1, j]);
		}
		
		if (j - 1 >= 0)
		{
			n.Add(_landTiles[i, j - 1]);
		}
		
		if (j + 1 < _rows)
		{
			n.Add(_landTiles[i, j + 1]);
		}
		
		if (j % 2 != 0)
		{
			if (i + 1 < _columns && j + 1 < _rows )
			{
				n.Add(_landTiles[i + 1, j + 1]);
			}
			
			if (i + 1 < _columns && j - 1 >= 0)
			{
				n.Add(_landTiles[i + 1, j - 1]);
			}
		}
		else
		{
			if (i - 1 >= 0 && j - 1 >= 0)
			{
				n.Add(_landTiles[i - 1, j - 1]);
			}
			
			if (i - 1 >= 0 && j + 1 < _rows)
			{
				n.Add(_landTiles[i - 1, j + 1]);
			}
		}
        
        //Check to make sure no SEA tiles are added to neighbors
        List<TileComponent> nCopy = new List<TileComponent>(n);
        foreach (TileComponent t in nCopy)
        {
            if (t.getLandType() == LandType.SEA) 
            {
                n.Remove(t);
            }
        }
		_landTiles[i,j].setNeighbours(n);
	}

	/// <summary>
	/// Converts some of the tiles within the grid to forest and meadow tiles.
	/// </summary>
	/// <param name="forestRatio">A value between 0 and 1.</param>
	/// <param name="meadowRatio">A value between 0 and 1.</param>
	private void AddTerrain( float forestRatio, float meadowRatio ) {
		//Ensure that the requested amount doesn't exceed the current amount of tiles
		float adjustedRatio_forest = Mathf.Min(Mathf.Max(0.0f, forestRatio), 1.0f);
		float adjustedRatio_meadow = Mathf.Min(Mathf.Max(0.0f, forestRatio), 1.0f);
		//If forest and meadow ratios together exceed 1, reduce the number of meadows
		while (adjustedRatio_forest + adjustedRatio_meadow > 1.0f) {
			adjustedRatio_meadow -= 0.1f;
		}
		int tileCount = _landTiles.GetLength(0) * _landTiles.GetLength(1);
		int forestCount = (int)(tileCount * adjustedRatio_forest);
		int meadowCount = (int)(tileCount * adjustedRatio_meadow);
		
		//Change into forest tiles
		for (int i = 0; i < forestCount; ++i )
		{
			//Choose a random tile
			int indexX = Random.Range(0, _landTiles.GetLength(0)-1);
			int indexY = Random.Range(0, _landTiles.GetLength(1)-1);
			if(Network.isServer){
				networkView.RPC("SetToTerrain", RPCMode.Others, indexX, indexY, (int)LandType.FOREST);
			}
			_landTiles[indexX, indexY].setLandType(LandType.FOREST);
			//Change its properties
			//tile.setLandType(LandType.FOREST);
			//Update the visual component of this tile
			//tile.setGameObject(LandType.FOREST);
		}
		
		//Change into meadow tiles
		for (int i = 0; i < meadowCount; ++i)
		{
			//Choose a random tile
			int indexX = Random.Range(0, _landTiles.GetLength(0)-1);
			int indexY = Random.Range(0, _landTiles.GetLength(1)-1);
			if(Network.isServer){
				networkView.RPC("SetToTerrain", RPCMode.Others, indexX, indexY, (int)LandType.MEADOW);
			}
			_landTiles[indexX, indexY].setLandType(LandType.MEADOW);
			//Update the visual component of this tile
			//tile.setGameObject(LandType.MEADOW);
		}
	}

	[RPC]
	private void SetToTerrain(int i, int j, int landTypeIndex){
		_landTiles[i, j].setLandType((LandType)landTypeIndex);
	}

	[RPC]
	private void SetToMeadow(int i, int j){
		_landTiles[i, j].setLandType(LandType.MEADOW);
	}

	public TileComponent[,] getLandTiles()
	{
		return _landTiles;
	}
}