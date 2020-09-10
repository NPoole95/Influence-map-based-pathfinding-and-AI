using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;


public class LoadMapFromFile : MonoBehaviour
{
    public GameObject NavMeshBaker;

    //public Terrain terrain;

    public int mapHeight;
    public int mapWidth;

    public Transform Ground;
    public Transform WallEnd;
    public Transform Wall;
    public Transform Cannon;
    public Transform Tower;
    public Transform Resource;
    public Transform AirShip;
    public Transform Barracks;
    public Transform OffenceBarracks;

    // multiple different scenery prefabs
    public Transform Bush_01_a;
    public Transform Bush_01_b;
    public Transform Bush_01_c;
    public Transform Bush_02_a;
    public Transform Bush_02_b;
    public Transform Bush_02_c;
    public Transform Rock_01_a;
    public Transform Rock_01_b;
    public Transform Tree_01_a;
    public Transform Tree_01_b;
    public Transform Tree_02_a;
    public Transform Tree_02_b;
    public Terrain terrain;

    public const int sGround = 0;
    public const int sWallEnd = 1;
    public const int sWall = 2;
    public const int sWallRotated = 3;
    public const int sCannon = 4;
    public const int sTower = 5;
    public const int sResource = 6;
    public const int sAirShip = 7;
    public const int sScenery = 8;
    public const int sBarracks = 9;

    public const int sDefender = 10; // only used in influence map, cannot be added in file as is is more that one digit


    public const float wallOffset = 0.5f;
   // public const int mapSize = 30;
    public const int airshipHeight = 5;

    public int[,] map;

    public const string mapName = @"Assets/Resources/Maps/Map1.txt";

    public void Awake()
    {
        map = readFile(mapName);

        // random generator for adding in scenery
        System.Random rand = new System.Random();
        int r;



        // load in the grass floor 

        Transform ground = Instantiate(Ground, new Vector3(mapWidth / 2, -0.1f, mapHeight / 2), Quaternion.identity);
        ground.transform.localScale = new Vector3 (mapWidth / 10, 1.0f, mapHeight / 10);

        terrain.transform.position = new Vector3(-10.0f, 0.0f, -10.0f);//((mapWidth / 2), -0.1f, (mapHeight / 2));
        terrain.terrainData.size = new Vector3(mapWidth + 20, 50.0f, mapHeight + 20);       

        // instantiates all objects in the scene based on the text file that was read in
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                switch (map[x, y])
                {
                    case sGround:
                        // do nothing as this is empty space
                        break;
                    case sWallEnd:
                            Instantiate(WallEnd, new Vector3(x, 0, y), Quaternion.identity);
                        break;
                    case sWall:
                            Instantiate(Wall, new Vector3(x, 0, y), Quaternion.identity);
                            Instantiate(Wall, new Vector3(x + wallOffset, 0, y), Quaternion.identity);
                            Instantiate(Wall, new Vector3(x - wallOffset, 0, y), Quaternion.identity);
                        break;
                    case sWallRotated:
                            Instantiate(Wall, new Vector3(x, 0, y), Quaternion.AngleAxis(90.0f, Vector3.up));
                            Instantiate(Wall, new Vector3(x, 0, y + wallOffset), Quaternion.AngleAxis(90.0f, Vector3.up));
                            Instantiate(Wall, new Vector3(x, 0, y - wallOffset), Quaternion.AngleAxis(90.0f, Vector3.up));
                        break;
                    case sCannon:
                        Instantiate(Cannon, new Vector3(x, 0, y), Quaternion.identity);
                        break;
                    case sTower:
                       Instantiate(Tower, new Vector3(x, 0, y), Quaternion.identity);
                        break;
                    case sResource:
                        Instantiate(Resource, new Vector3(x, 0, y), Quaternion.AngleAxis(-90.0f, Vector3.right));
                        break;
                    case sAirShip:
                        Instantiate(AirShip, new Vector3(x, airshipHeight, y), Quaternion.AngleAxis(-90.0f, Vector3.right));
                        break;
                    case sBarracks:
                        Instantiate(Barracks, new Vector3(x, 0.0f, y), Quaternion.AngleAxis(180.0f, Vector3.up));
                        break;
                    case sScenery:
                        r = rand.Next(0, 11);
                        if (r == 0)
                        {
                            Instantiate(Bush_01_a, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 1)
                        {
                            Instantiate(Bush_01_b, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 2)
                        {
                            Instantiate(Bush_01_c, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 3)
                        {
                            Instantiate(Bush_02_a, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 4)
                        {
                            Instantiate(Bush_02_b, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 5)
                        {
                            Instantiate(Bush_02_c, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 6)
                        {
                            Instantiate(Rock_01_a, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 7)
                        {
                            Instantiate(Rock_01_b, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 8)
                        {
                            Instantiate(Tree_01_a, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 9)
                        {
                            Instantiate(Tree_01_b, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 10)
                        {
                            Instantiate(Tree_02_a, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        else if (r == 11)
                        {
                            Instantiate(Tree_02_b, new Vector3(x, 0, y), Quaternion.identity);
                        }
                        break;

                }
            }
        }

       //for (int x = -1; x < mapWidth + 1; x++)
       //{
       //    Instantiate(Tree_01_a, new Vector3(x, 0, -1), Quaternion.identity);
       //    Instantiate(Tree_01_a, new Vector3(x, 0, mapWidth + 1), Quaternion.identity);       
       //}
       //
       //for (int x = -1; x < mapHeight + 1; x++)
       //{           
       //    Instantiate(Tree_01_a, new Vector3(-1, 0, x), Quaternion.identity);
       //    Instantiate(Tree_01_a, new Vector3(mapHeight + 1, 0, x), Quaternion.identity);
       //}

        NavMeshBaker = GameObject.FindWithTag("NavMeshBaker");
    }

    public int[,] readFile(string mapFile)
    {
        var file = new StreamReader(mapFile);
        string line;

        line = file.ReadLine();
        for (int i = 0; i < line.Length; i++)
        {
            mapWidth = int.Parse(line);
        }

        line = file.ReadLine();
        for (int i = 0; i < line.Length; i++)
        {
           mapHeight = int.Parse(line);
        }

        map = new int[mapWidth, mapHeight];
        int lineCount = mapHeight - 1;

        while ((line = file.ReadLine()) != null)
        {
            if (lineCount >= 0)
            {
                for (int i = 0; i < mapWidth && i < line.Length; i++)
                {
                    map[i, lineCount] = line[i] - '0';
                }
            }
            lineCount--;
        }
        file.Close();

        return map;
    }
}





