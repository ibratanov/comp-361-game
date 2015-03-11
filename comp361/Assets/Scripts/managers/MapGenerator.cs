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

	public void GenerateMap(){
        _landTiles = new TileComponent[_columns, _rows];
		GenerateSquareGrid( _rows, _columns, _origin, _tileHeight, _tileDiagonal);
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
		int nPlayers = this.GetComponent<GameComponent>()._playerManager.GetPlayers().Count;
		int gridId = 0;
		Vector3 currentPosition = startLocation;
		for (int i = 0; i < columns; ++i)
		{
			bool phase = false;
			currentPosition = startLocation + tileHeight * i;
			for (int j = 0; j < rows; ++j)
			{
                GameObject gameTile;
				//Create a new tile and add it to the landTiles array
                if (Network.isClient || Network.isServer) {
                    gameTile = (GameObject)Network.Instantiate(_gameTile, currentPosition, Quaternion.identity, 0);
                }
                else
                {
                    gameTile = (GameObject)Instantiate(_gameTile, currentPosition, Quaternion.identity);
                }
				_landTiles[i,j] = gameTile.GetComponent<TileComponent>();
				//_landTiles[i,j].setGameObject(LandType.GRASS);
				_landTiles[i,j].setInitialPlayerIndex(Random.Range(0, nPlayers));
				gameTile.transform.parent = this.transform; //Keep things organized with a parental hierarchy

				//Update position for next tile
				Vector3 diag = tileDiagonal;
				if(phase){
					diag.z *= -1; //We're drawing horizontally so it zigzags
				}
				phase = !phase;
				currentPosition += diag;
                _landTiles[i, j].setTileGameObject(gameTile);
			}
		}
        // call getNeighbors on each tile
        for (int i = 0; i < columns; ++i)
        {
            for (int j = 0; j < rows; ++j)
            {
                generateNeighbours(i, j);
            }
        }
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
			TileComponent tile = (TileComponent)_landTiles[indexX, indexY];
			//Change its properties
			tile.setLandType(LandType.FOREST);
			//Update the visual component of this tile
			//tile.setGameObject(LandType.FOREST);
		}
		
		//Change into meadow tiles
		for (int i = 0; i < meadowCount; ++i)
		{
			//Choose a random tile
			int indexX = Random.Range(0, _landTiles.GetLength(0)-1);
			int indexY = Random.Range(0, _landTiles.GetLength(1)-1);
			TileComponent tile = (TileComponent)_landTiles[indexX, indexY];
			//Change its properties
			tile.setLandType(LandType.MEADOW);
			//Update the visual component of this tile
			//tile.setGameObject(LandType.MEADOW);
		}
	}

	public void generateNeighbours(int i, int j)
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
		_landTiles[i,j].setNeighbours(n);
    }

	public TileComponent[,] getLandTiles()
	{
		return _landTiles;
	}
}
