using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {
	public AssetManager _assets;
	public TileComponent _tileComponent;

	public Vector3 _origin;
	public int _rows = 17;
	public int _columns = 18;
	public float _forestRatio = 0.2f;
	public float _meadowRatio = 0.1f;
	public Vector3 _tileHeight = new Vector3(0, 0, 1.732f);
	public Vector3 _tileDiagonal = new Vector3(1.5f, 0, 0.866f);
	private TileComponent[,] _landTiles;
	private TileComponent[,] _waterTiles;

	// Use this for initialization
	void Start () {
		_landTiles = new TileComponent[_columns,_rows];
		//_waterTiles = new GameObject[_rows,_columns];
		GenerateMap();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void GenerateMap(){
		if(Network.isServer){
			GenerateSquareGrid( _rows, _columns, _origin, _tileHeight, _tileDiagonal);
			AddTerrain(_forestRatio, _meadowRatio);
		}
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
		int gridId = 0;
		Vector3 currentPos = startLocation;
		for (int i = 0; i < columns; ++i)
		{
			bool phase = false;
			currentPos = startLocation + tileHeight * i;
			for (int j = 0; j < rows; ++j)
			{
				//Add a new tile
				TileComponent tile = new TileComponent();
				GameObject tileObject = (GameObject)Instantiate(_assets.getTerrainGameObject(LandType.GRASS), currentPos, Quaternion.identity); //TODO: Network this
				tileObject.transform.parent = this.transform; //Keep things organized
				tile.setGameObject(tileObject);
				_landTiles[i,j] = tile; //For future access

				//Update position for next tile
				Vector3 diag = tileDiagonal;
				if(phase){
					diag.z *= -1; //We're drawing horizontally so it zigzags
				}
				phase = !phase;
				currentPos += diag;
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
			int indexX = Random.Range(0, _landTiles.GetLength(0)-1);
			int indexY = Random.Range(0, _landTiles.GetLength(1)-1);
			TileComponent tile = (TileComponent)_landTiles[indexX, indexY];
			tile.setLandType(LandType.FOREST);
			GameObject tileObject = tile.getGameObject();
			tileObject = (GameObject)Instantiate(_assets.getTerrainGameObject(LandType.FOREST), tileObject.transform.position, Quaternion.identity);
			tileObject.transform.parent = this.transform; //Keep things organized
			tile.setGameObject(tileObject);
		}
		
		//Change into meadow tiles
		for (int i = 0; i < meadowCount; ++i)
		{
			int indexX = Random.Range(0, _landTiles.GetLength(0)-1);
			int indexY = Random.Range(0, _landTiles.GetLength(1)-1);
			TileComponent tile = (TileComponent)_landTiles[indexX, indexY];
			tile.setLandType(LandType.MEADOW);
			GameObject tileObject = tile.getGameObject();
			tileObject = (GameObject)Instantiate(_assets.getTerrainGameObject(LandType.MEADOW), tileObject.transform.position, Quaternion.identity);
			tileObject.transform.parent = this.transform; //Keep things organized
			tile.setGameObject(tileObject);
		}
	}
}
