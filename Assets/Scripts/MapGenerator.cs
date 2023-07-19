using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class MapGenerator : MonoBehaviour
{

    private int mapWidth = 20;
    private int mapHeight = 20;
    private int _randomNumber;
    private int _tileNumber = 1;
    private int _minDistanceBetweenCities = 5;

    private float _horizonMultiplier;
    private float _verticalMultiplier;
    private float _oddHorizonOffset;
    private List<int[]> _citySpot = new List<int[]>();

    private GameObject _createdTile;

    [SerializeField] private GameObject _city;
    [SerializeField] private GameObject _grass;
    [SerializeField] private GameObject _forest;
    [SerializeField] private GameObject _rock;
    [SerializeField] private GameObject _water;
    [SerializeField] private GameObject _desert;

    [SerializeField] private  Tilemap _tileMap;
    [SerializeField] private  Tile _cityTile;
    [SerializeField] private  Tile _grassTile;
    [SerializeField] private  Tile _forestTile;
    [SerializeField] private  Tile _rockTile;
    [SerializeField] private  Tile _waterTile;
    [SerializeField] private  Tile _desertTile;

    [SerializeField] private TextMeshProUGUI _AmountOfCitiesInput;
    
    private int _AmountOfCities = 0;

    private Dictionary<string, Tile> _mapDict;

    private List<GameObject> _tileGameObjects = new List<GameObject>();

    public void Generate()
    {
        // Clear the generated tiles so new ones can be generated
        DestroyTiles();
        // Here we are getting an offset so that the hexes will touch each other.
        _horizonMultiplier = _grass.GetComponent<Renderer>().bounds.size.x;
        _verticalMultiplier = _grass.GetComponent<Renderer>().bounds.size.y * 0.75f;

        _oddHorizonOffset = _grass.GetComponent<Renderer>().bounds.size.x / 2;

        int failureCounter = 0;
        int tries = 0;

        while (_citySpot.Count < _AmountOfCities)
        {
            // Get the random numbers and make them work with the map in which the middle is 0,0
            int randomHeight = Random.Range(1, mapHeight - 1);
            if(randomHeight < mapHeight/2) 
            {
                randomHeight = -randomHeight;
            }
            else
            {   
                randomHeight = randomHeight - (mapHeight/2);
            }
            int randomWidth = Random.Range(1, mapWidth - 1);
            if(randomWidth < mapWidth/2)
            {
                randomWidth = -randomWidth;
            }
            else
            {
                randomWidth = randomWidth - (mapWidth/2);
            }
                
            if (_citySpot.Count == 0)
            {
                _citySpot.Add(new int[] {randomWidth, randomHeight});
                Debug.Log("Added the first city at: " + randomWidth + "," + randomHeight);
            }
            else
            {
                List<bool> markCity = new List<bool>();
                // check the other city spots and see if the distance is big enough to create a new city
                foreach(int[] spot in _citySpot)
                {
                    if((randomWidth > spot[0] + _minDistanceBetweenCities || randomWidth < spot[0] - _minDistanceBetweenCities) ||
                        (randomHeight > spot[1] + _minDistanceBetweenCities || randomHeight < spot[1] - _minDistanceBetweenCities))
                    {
                        markCity.Add(true);
                    }
                    else
                    {
                        markCity.Add(false);
                    }
                }
                if (markCity.Contains(false))
                {
                    failureCounter++;
                } 
                else
                {
                    _citySpot.Add(new int[] {randomWidth, randomHeight});
                    failureCounter = 0;
                    Debug.Log("Added a city");
                }
                if(failureCounter == 10)
                {
                    _citySpot.Clear();
                    tries++;
                }
                // if(tries > 2)
                // {
                //     Debug.Log("Not possible");
                //     break;
                // }
            }
        }

        // Here we are going to generate the world randomly.
        // For each row we generate the map from left to right.
        for(int height = -(mapHeight/2); height < (mapHeight/2); height++)
        {
            for(int width = -(mapWidth/2); width < (mapWidth/2); width++)
            {
                int[] checkData = new int[] {width, height};
                bool buildCity = false;

                foreach(int[] item in _citySpot)
                {
                    if(item[0] == checkData[0] && item[1] == checkData[1])
                    {
                        buildCity = true;
                    }
                }

                if (buildCity)
                {
                    CreateTile(_city, width, height, "cityTile");
                    buildCity = false;
                }
                else
                {
                    // We are going to generate a random number that we can use to get a random tile
                    _randomNumber = Random.Range(0, 11);
                    // To have more grass and water we are going to give them more numbers
                    switch(_randomNumber)
                    {
                        // If the number is 1, 2 or 3 or lower do grass, this gives 30% chance of grass
                        case < 4:
                            CreateTile(_grass, width, height, "grassTile");
                            break;
                        // If the number is 4, 5 or 6 the result will be forest. thus a 30% chance
                        case > 3 and < 7:
                            CreateTile(_forest, width, height, "forestTile");
                            break;
                        // If the number is 7 or 8 the result will be water, thus a 20% chance
                        case > 6 and < 9:
                            CreateTile(_water, width, height, "waterTile");
                            break;
                        // If the number is a 9 it will do rocks thus 10% chance for rocks
                        case > 8 and <10:
                            CreateTile(_rock, width, height, "rockTile");
                            break;
                        // If the number is a 10 it will do desert thus 10% chance for desert
                        case >9:
                            CreateTile(_desert, width, height, "desertTile");
                            break;
                    }
                    // CreateTile(rock, width, height, "rockTile");
                }
            }
        }
    }

    private void CreateTile(GameObject tile, int width, int height, string map)
    {
        if(height%2==0)
        {
            _createdTile = Instantiate(tile, new Vector3(width * _horizonMultiplier, height * _verticalMultiplier, -1f), Quaternion.identity);
        }
        else{
            _createdTile  = Instantiate(tile, new Vector3(width * _horizonMultiplier + _oddHorizonOffset, height * _verticalMultiplier, -1f), Quaternion.identity);
        }
        _tileGameObjects.Add(_createdTile);
        WorkWithTile(_createdTile.name, _tileNumber, map, width, height);
        _tileNumber++;
    }

    private void WorkWithTile(string _createdTileName, int client_TileNumber, string map, int width, int height)
    {
        GameObject client_CreatedTile = GameObject.Find(_createdTileName);
        _mapDict = new Dictionary<string, Tile>  {{"cityTile", _cityTile},
                                                  {"grassTile", _grassTile},
                                                  {"forestTile", _forestTile},
                                                  {"rockTile", _rockTile},
                                                  {"waterTile", _waterTile},
                                                  {"desertTile", _desertTile}};

        // Change the tile name to somesthing we can more easily debug if needed.
        // Add it to a city list so that the world manager knows where they are so they can be used later on
        // Add the tile to an dictionary so we can get to the gameobject more easily later on
        if(client_CreatedTile != null)
        {
            client_CreatedTile.name = client_TileNumber.ToString();
            _tileMap.SetTile(new Vector3Int(width, height, 0), _mapDict[map]);
        }
    }

    public void SetAmountOfCities()
    {
        string newStringValue = _AmountOfCitiesInput.text;
        string newValueTrimmed = newStringValue.Trim();
        // For some reason there is an extra byte at the end of the string. For now I just pick a substring and ignore the last byte.
        if (int.TryParse(newValueTrimmed.Substring(0, newValueTrimmed.Length-1), out int newValue))
        {
            _AmountOfCities = newValue;
        }
        else
        {
            Debug.Log("Amount of cities wasn't a number");
        }
        
    }

    private void DestroyTiles()
    {
        _citySpot.Clear();
        foreach(GameObject tile in _tileGameObjects)
        {
            Destroy(tile);
        }
    }
}
