using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InfluenceMapController : MonoBehaviour
{
    [SerializeField]
    LoadMapFromFile LMFF;

    public static GameObject[,] IMQuadArray;
    public static GameObject[,] TMQuadArray;

    private bool IMEnabled = false;
    private bool TMEnabled = false;
    [SerializeField]
    Text IMModeText;
    [SerializeField]
    Text MapUpdateTimer;
    float timeTillIMUpdate = 0.0f;
    float timeTillTMUpdate = 0.0f;

    const float MapMaxInfluenceValue = 4.0f; // the maximum level of danger in a tile
    const float MapMinInfluenceValue = -1.0f; // the maximum level of danger in a tile
    const float MaxDistance = 10.0f; // tha maximum distance danger will radiate on the influence map

    // this modifier changes the attenuation of the influence (LOW IS FURTHER)

    const float defenceDistanceModifier = 0.3f; // used to increase the rance of influence radiation
    const float fighterDistanceModifier = 0.5f; // used to increase the rance of influence radiation


    // the maximum values of influence a source can radiate
    const float cannonMaxInfluence = 0.6f;
    const float towerMaxInfluence = 1.0f;
    const float barracksMaxInfluence = 1.0f;
    const float airShipMaxInfluence = 1.0f;
    const float defenderMaxInfluence = 1.0f;
    const float friendlyMaxInfluence = -1.0f;

    // The maximum ranges a sources influence can spread
    const int cannonMaxRange = 6;
    const int towerMaxRange = 8;
    const int barracksMaxRange = 10;
    const int airShipMaxRange = 8;
    const int defenderMaxRange = 6;
    const int friendlyMaxRange = 6;

    // the size of the 2D array used to hold the pre baked influence. (these are increased by 1 to allow a "center point" to make superimposing over the full influence map easier
    const int cannonBakeSize = cannonMaxRange + 1;
    const int towerBakeSize = towerMaxRange + 1;
    const int barracksBakeSize = barracksMaxRange + 1;
    const int airShipBakeSize = airShipMaxRange + 1;
    const int defenderBakeSize = defenderMaxRange + 1;
    const int friendlyBakeSize = friendlyMaxRange + 1;

    private float[,] cannonBake;
    private float[,] towerBake;
    private float[,] defenderBake;
    private float[,] barracksBake;
    private float[,] airShipBake;
    private float[,] friendlyBake;

    const float influenceMapUpdateFrequency = 1.0f;

    public GameObject[] mobileUnits;
    public GameObject[] defenderUnits;
    public GameObject[] friendlyUnits;


    // int[,] map = LoadMapFromFile.map;
    public float[,] threatMap;// = new float[LoadMapFromFile.mapSize, LoadMapFromFile.mapSize]; // the map containing just enemies, used for pathing
    public float[,] influenceMap;// = new float[LoadMapFromFile.mapSize, LoadMapFromFile.mapSize]; // thhe map containing enemies and friendlies, used for decision making
    float transparancy = 0.5f;
    Color colour;
    //struct MapNode
    //{
    //    public int x;
    //    public int y;
    //    MapNode(int x, int y)
    //    {
    //        this.x = x;
    //        this.y = y;
    //    }
    //};
    struct dangerSource
    {
        public int x;
        public int y;
        public int dangerType;
        public dangerSource(int x, int y, int dangerType)
        {
            this.x = x;
            this.y = y;
            this.dangerType = dangerType;
        }
    };
    List<dangerSource> dangerSources = new List<dangerSource>();

    void Start()
    {
        LMFF = GameObject.Find("LoadMapFromFile").GetComponent<LoadMapFromFile>();

        IMQuadArray = new GameObject[LMFF.mapWidth, LMFF.mapHeight]; //= GameObject.CreatePrimitive(PrimitiveType.Sphere);
        TMQuadArray = new GameObject[LMFF.mapWidth, LMFF.mapHeight]; //= GameObject.CreatePrimitive(PrimitiveType.Sphere);

        threatMap = new float[LMFF.mapWidth, LMFF.mapHeight]; // the map containing just enemies, used for pathing
        influenceMap = new float[LMFF.mapWidth, LMFF.mapHeight]; // the map containing enemies and friendlies, used for decision making

        cannonBake = new float[cannonBakeSize, cannonBakeSize]; // stores the prebakes influence for the cannon defence
        towerBake = new float[towerBakeSize, towerBakeSize];
        defenderBake = new float[defenderBakeSize, defenderBakeSize];
        barracksBake = new float[barracksBakeSize, barracksBakeSize];
        airShipBake = new float[airShipBakeSize, airShipBakeSize];
        friendlyBake = new float[friendlyBakeSize, friendlyBakeSize];

        // loop to initiaize danger sources and place them on a list        

        initializeDangerSources();

        //// loop for processing the influence map and radiating 'danger' ///
        radiateInfluence();

        string output = " ";

        for (int x = 0; x < LMFF.mapWidth; x++)
        {
            for (int y = 0; y < LMFF.mapHeight; y++)
            {
                output += threatMap[x, y].ToString();
                output += ", ";
            }
            output = " ";
        }


        // load the maps visual
        loadThreatMapVisual();

        createInfluenceMap();

        loadInfluenceMapVisual();

        const float threatMapDelay = 1.0f;
        const float influenceMapDelay = 1.15f;
        InvokeRepeating("updateThreatMap", threatMapDelay, influenceMapUpdateFrequency); // calls the update influence map funtions at set intervals instead of every frame, starting after 1 second
        InvokeRepeating("updateInfluenceMap", influenceMapDelay, influenceMapUpdateFrequency); // calls the update influence map funtions at set intervals instead of every frame, starting after 1 second
    }
    // Update is called once per frame
    void Update()
    {
        ///////////////// Key controlls to display influence map ///////////////////////


        // update the timers showin g the user when the maps will update next
        timeTillIMUpdate -= Time.deltaTime;
        timeTillTMUpdate -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            IMEnabled = !IMEnabled;
            TMEnabled = false;
        }
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            TMEnabled = !TMEnabled;
            IMEnabled = false;
        }

        if (IMEnabled)
        {
            // enable influence map mode text
            IMModeText.text = "Influence Map Activated";
            MapUpdateTimer.text = "Updates in: " + System.Math.Round(timeTillIMUpdate, 1) + " seconds";
        }
        else if (TMEnabled)
        {
            //enable influence map mode text
            IMModeText.text = "Threat Map Activated";
            MapUpdateTimer.text = "Updates in: " + System.Math.Round(timeTillTMUpdate, 1) + " seconds";
        }
        else
        {
            IMModeText.text = "";
            MapUpdateTimer.text = "";
        }

        if (IMEnabled)
        {
            for (int x = 0; x < LMFF.mapWidth; x++)
            {
                for (int y = 0; y < LMFF.mapHeight; y++)
                {
                    IMQuadArray[x, y].SetActive(true);
                }
            }
        }
        else
        {
            //make map visuals invisible
            for (int x = 0; x < LMFF.mapWidth; x++)
            {
                for (int y = 0; y < LMFF.mapHeight; y++)
                {
                    IMQuadArray[x, y].SetActive(false);
                }
            }
        }

        if (TMEnabled)
        {
            for (int x = 0; x < LMFF.mapWidth; x++)
            {
                for (int y = 0; y < LMFF.mapHeight; y++)
                {
                    TMQuadArray[x, y].SetActive(true);
                }
            }
        }
        else
        {
            //make map visuals invisible
            for (int x = 0; x < LMFF.mapWidth; x++)
            {
                for (int y = 0; y < LMFF.mapHeight; y++)
                {
                    TMQuadArray[x, y].SetActive(false);
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////      




    }

    void loadThreatMapVisual()
    {
        for (int x = 0; x < LMFF.mapWidth; x++)
        {
            for (int y = 0; y < LMFF.mapHeight; y++)
            {
                // for each individual tile, check the value (0.0 - 1.0) and instantiate a tile with a colour based on this. 0 being blue, 1 being red.
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);//Instantiate(IMQuad, new Vector3((float)x, 0, y), Quaternion.identity) as GameObject;
                go.transform.position = new Vector3(x, 0.1f, y);
                go.transform.Rotate(90.0f, 0.0f, 0.0f);
                go.GetComponent<MeshCollider>().enabled = false;

                //Get the Renderer component from the new quad
                var quadRenderer = go.GetComponent<Renderer>();
                setRenderModeTransparent(quadRenderer.material);

                colour.a = transparancy;
                colour.r = threatMap[x, y];
                colour.b = 1.0f - threatMap[x, y];
                colour.g = 0.0f;

                //Call SetColor using the shader property name "_Color" and setting the color to red
                quadRenderer.receiveShadows = false;
                quadRenderer.material.SetColor("_Color", colour);

                go.GetComponent<MeshRenderer>().material.SetColor("_color", colour);
                TMQuadArray[x, y] = go;

            }
        }
    }

    void loadInfluenceMapVisual()
    {
        for (int x = 0; x < LMFF.mapWidth; x++)
        {
            for (int y = 0; y < LMFF.mapHeight; y++)
            {
                // for each individual tile, check the value (0.0 - 1.0) and instantiate a tile with a colour based on this. 0 being blue, 1 being red.
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);//Instantiate(IMQuad, new Vector3((float)x, 0, y), Quaternion.identity) as GameObject;
                go.transform.position = new Vector3(x, 0.1f, y);
                go.transform.Rotate(90.0f, 0.0f, 0.0f);
                go.GetComponent<MeshCollider>().enabled = false;

                //Get the Renderer component from the new quad
                var quadRenderer = go.GetComponent<Renderer>();
                setRenderModeTransparent(quadRenderer.material);

                colour.a = transparancy;
                colour.r = influenceMap[x, y];
                colour.b = Mathf.Abs(influenceMap[x, y]);
                colour.g = 0.0f;

                //Call SetColor using the shader property name "_Color" and setting the color to red
                quadRenderer.receiveShadows = false;
                quadRenderer.material.SetColor("_Color", colour);

                go.GetComponent<MeshRenderer>().material.SetColor("_color", colour);
                IMQuadArray[x, y] = go;

            }
        }
    }

    void setRenderModeTransparent(Material m) // https://answers.unity.com/questions/1004666/change-material-rendering-mode-in-runtime.html?_ga=2.33325886.1666490949.1570806515-970671615.1569277966
    {
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.DisableKeyword("_ALPHABLEND_ON");
        m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
    }

    void initializeDangerSources()
    {

        // First create a list of all danger sources. 
        mobileUnits = GameObject.FindGameObjectsWithTag("Fighter - Air");

        for (int x = 0; x < LMFF.mapWidth; x++)
        {
            for (int y = 0; y < LMFF.mapHeight; y++)
            {
                if (LMFF.map[x, y] == LoadMapFromFile.sCannon)
                {
                    threatMap[x, y] = cannonMaxInfluence;
                    dangerSources.Add(new dangerSource(x, y, LoadMapFromFile.sCannon));

                }
                else if (LMFF.map[x, y] == LoadMapFromFile.sTower)
                {
                    threatMap[x, y] = towerMaxInfluence;
                    dangerSources.Add(new dangerSource(x, y, LoadMapFromFile.sTower));
                }
                else if (LMFF.map[x, y] == LoadMapFromFile.sBarracks)
                {
                    threatMap[x, y] = barracksMaxInfluence;
                    dangerSources.Add(new dangerSource(x, y, LoadMapFromFile.sBarracks));
                }
            }
        }

        foreach (GameObject mobileUnit in mobileUnits)
        {
            int x = (int)mobileUnit.transform.position.x;
            int y = (int)mobileUnit.transform.position.z; // changes the Z position to y to convert from 3d space to the 2d grid

            threatMap[x, y] = airShipMaxInfluence;
            dangerSources.Add(new dangerSource(x, y, LoadMapFromFile.sAirShip));
        }


        // Next create pre bakes of radiation

        // Cannon pre bake        
        for (int x = 0; x < cannonBakeSize; x++)
        {
            for (int y = 0; y < cannonBakeSize; y++)
            {

                float xDistance = Mathf.Abs(cannonMaxRange / 2 - x);
                float yDistance = Mathf.Abs(cannonMaxRange / 2 - y);
                float distance = ((xDistance * xDistance) + (yDistance * yDistance)) * defenceDistanceModifier;

                float influenceValue = cannonMaxInfluence - cannonMaxInfluence * (distance / cannonMaxRange); // http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter30_Modular_Tactical_Influence_Maps.pdf

                if (influenceValue > 0)
                {
                    if (cannonBake[x, y] > MapMaxInfluenceValue)
                    {
                        influenceValue = MapMaxInfluenceValue;
                    }
                    cannonBake[x, y] += influenceValue;
                }
            }
        }
        // Tower pre bake
        for (int x = 0; x < towerBakeSize; x++)
        {
            for (int y = 0; y < towerBakeSize; y++)
            {

                float xDistance = Mathf.Abs(towerMaxRange / 2 - x);
                float yDistance = Mathf.Abs(towerMaxRange / 2 - y);
                float distance = ((xDistance * xDistance) + (yDistance * yDistance)) * defenceDistanceModifier;


                float influenceValue = towerMaxInfluence - towerMaxInfluence * (distance / towerMaxRange); // http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter30_Modular_Tactical_Influence_Maps.pdf

                if (influenceValue > 0)
                {
                    if (towerBake[x, y] > MapMaxInfluenceValue)
                    {
                        influenceValue = MapMaxInfluenceValue;
                    }
                    towerBake[x, y] += influenceValue; // use += to allow dangerous zones to overlap
                }
            }
        }

        // Defender pre bake
        for (int x = 0; x < defenderBakeSize; x++)
        {
            for (int y = 0; y < defenderBakeSize; y++)
            {

                float xDistance = Mathf.Abs(defenderMaxRange / 2 - x);
                float yDistance = Mathf.Abs(defenderMaxRange / 2 - y);
                float distance = ((xDistance * xDistance) + (yDistance * yDistance)) * fighterDistanceModifier;


                float influenceValue = defenderMaxInfluence - defenderMaxInfluence * (distance / defenderMaxRange); // http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter30_Modular_Tactical_Influence_Maps.pdf

                if (influenceValue > 0)
                {
                    if (defenderBake[x, y] > MapMaxInfluenceValue)
                    {
                        influenceValue = MapMaxInfluenceValue;
                    }
                    defenderBake[x, y] += influenceValue; // use += to allow dangerous zones to overlap
                }
            }
        }
        // Barracks pre bake
        for (int x = 0; x < barracksBakeSize; x++)
        {
            for (int y = 0; y < barracksBakeSize; y++)
            {

                float xDistance = Mathf.Abs(barracksMaxRange / 2 - x);
                float yDistance = Mathf.Abs(barracksMaxRange / 2 - y);
                float distance = ((xDistance * xDistance) + (yDistance * yDistance)) * defenceDistanceModifier;


                float influenceValue = barracksMaxInfluence - barracksMaxInfluence * (distance / barracksMaxRange); // http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter30_Modular_Tactical_Influence_Maps.pdf

                if (influenceValue > 0)
                {
                    if (barracksBake[x, y] > MapMaxInfluenceValue)
                    {
                        influenceValue = MapMaxInfluenceValue;
                    }
                    barracksBake[x, y] += influenceValue; // use += to allow dangerous zones to overlap
                }
            }
        }
        // Airship pre bake
        for (int x = 0; x < airShipBakeSize; x++)
        {
            for (int y = 0; y < airShipBakeSize; y++)
            {

                float xDistance = Mathf.Abs(airShipMaxRange / 2 - x);
                float yDistance = Mathf.Abs(airShipMaxRange / 2 - y);
                float distance = ((xDistance * xDistance) + (yDistance * yDistance)) * defenceDistanceModifier;


                float influenceValue = airShipMaxInfluence - airShipMaxInfluence * (distance / airShipMaxRange); // http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter30_Modular_Tactical_Influence_Maps.pdf

                if (influenceValue > 0)
                {
                    if (airShipBake[x, y] > MapMaxInfluenceValue)
                    {
                        influenceValue = MapMaxInfluenceValue;
                    }
                    airShipBake[x, y] += influenceValue; // use += to allow dangerous zones to overlap
                }
            }
        }
        // Friendly pre bake
        for (int x = 0; x < friendlyBakeSize; x++)
        {
            for (int y = 0; y < friendlyBakeSize; y++)
            {

                float xDistance = Mathf.Abs(friendlyMaxRange / 2 - x);
                float yDistance = Mathf.Abs(friendlyMaxRange / 2 - y);
                float distance = ((xDistance * xDistance) + (yDistance * yDistance)) * fighterDistanceModifier;


                float influenceValue = friendlyMaxInfluence - friendlyMaxInfluence * (distance / friendlyMaxRange); // http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter30_Modular_Tactical_Influence_Maps.pdf

                if (influenceValue < 0)
                {
                    if (friendlyBake[x, y] > MapMaxInfluenceValue)
                    {
                        influenceValue = MapMaxInfluenceValue;
                    }
                    friendlyBake[x, y] += influenceValue; // use += to allow dangerous zones to overlap
                }
            }
        }
    }



    //  Next radiate influence value using radiation equation : float influenceValue = sourceInfluenceValue - sourceInfluenceValue * (distance / sourceMaxDistance); // http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter30_Modular_Tactical_Influence_Maps.pdf
    void radiateInfluence()
    {
        int xPos;
        int yPos;

        for (int i = 0; i < dangerSources.Count; i++) // for every source of danger, 
        {
            switch (dangerSources[i].dangerType)
            {
                case LoadMapFromFile.sCannon:

                    xPos = (int)dangerSources[i].x - (cannonMaxRange / 2);
                    yPos = (int)dangerSources[i].y - (cannonMaxRange / 2);

                    for (int x = 0; x < cannonBakeSize; x++)
                    {
                        for (int y = 0; y < cannonBakeSize; y++)
                        {
                            if (xPos + x >= 0 && xPos + x < LMFF.mapWidth && yPos + y >= 0 && yPos + y < LMFF.mapHeight)
                            {
                                threatMap[xPos + x, yPos + y] += cannonBake[x, y];
                            }
                        }
                    }
                    break;
                case LoadMapFromFile.sTower:

                    xPos = (int)dangerSources[i].x - (towerMaxRange / 2);
                    yPos = (int)dangerSources[i].y - (towerMaxRange / 2);

                    for (int x = 0; x < towerBakeSize; x++)
                    {
                        for (int y = 0; y < towerBakeSize; y++)
                        {
                            if (xPos + x >= 0 && xPos + x < LMFF.mapWidth && yPos + y >= 0 && yPos + y < LMFF.mapHeight)
                            {
                                threatMap[xPos + x, yPos + y] += towerBake[x, y];
                            }
                        }
                    }
                    break;

                case LoadMapFromFile.sBarracks:

                    xPos = (int)dangerSources[i].x - (barracksMaxRange / 2);
                    yPos = (int)dangerSources[i].y - (barracksMaxRange / 2);

                    for (int x = 0; x < barracksBakeSize; x++)
                    {
                        for (int y = 0; y < barracksBakeSize; y++)
                        {
                            if (xPos + x >= 0 && xPos + x < LMFF.mapWidth && yPos + y >= 0 && yPos + y < LMFF.mapHeight)
                            {
                                threatMap[xPos + x, yPos + y] += barracksBake[x, y];
                            }
                        }
                    }
                    break;
            }

        }

        foreach (GameObject mobileUnit in mobileUnits)
        {
            xPos = (int)mobileUnit.transform.position.x - (airShipMaxRange / 2);
            yPos = (int)mobileUnit.transform.position.z - (airShipMaxRange / 2); // changes the Z position to y to convert from 3d space to the 2d grid

            for (int x = 0; x < airShipBakeSize; x++)
            {
                for (int y = 0; y < airShipBakeSize; y++)
                {
                    if (xPos + x >= 0 && xPos + x < LMFF.mapWidth && yPos + y >= 0 && yPos + y < LMFF.mapHeight)
                    {
                        threatMap[xPos + x, yPos + y] += airShipBake[x, y];
                    }
                }
            }
        }

        foreach (GameObject defenderUnit in defenderUnits)
        {
            xPos = (int)defenderUnit.transform.position.x - (defenderMaxRange / 2);
            yPos = (int)defenderUnit.transform.position.z - (defenderMaxRange / 2); // changes the Z position to y to convert from 3d space to the 2d grid

            for (int x = 0; x < defenderBakeSize; x++)
            {
                for (int y = 0; y < defenderBakeSize; y++)
                {
                    if (xPos + x >= 0 && xPos + x < LMFF.mapWidth && yPos + y >= 0 && yPos + y < LMFF.mapHeight)
                    {
                        threatMap[xPos + x, yPos + y] += defenderBake[x, y];
                    }
                }
            }
        }
    }

    void zeroThreatMap()
    {
        mobileUnits = GameObject.FindGameObjectsWithTag("Fighter - Air");
        defenderUnits = GameObject.FindGameObjectsWithTag("Defender");

        foreach (GameObject defenderUnit in defenderUnits)
        {
            int x = (int)defenderUnit.transform.position.x;
            int y = (int)defenderUnit.transform.position.z; // changes the Z position to y to convert from 3d space to the 2d grid

            threatMap[x, y] = defenderMaxInfluence;
        }

        for (int x = 0; x < LMFF.mapWidth; x++)
        {
            for (int y = 0; y < LMFF.mapHeight; y++)
            {
                if (LMFF.map[x, y] == LoadMapFromFile.sCannon)
                {
                    threatMap[x, y] = cannonMaxInfluence;

                }
                else if (LMFF.map[x, y] == LoadMapFromFile.sTower)
                {
                    threatMap[x, y] = towerMaxInfluence;
                }
                else if (LMFF.map[x, y] == LoadMapFromFile.sBarracks)
                {
                    threatMap[x, y] = barracksMaxInfluence;
                }
                else if (LMFF.map[x, y] == LoadMapFromFile.sDefender)
                {
                    threatMap[x, y] = defenderMaxInfluence;
                }
                else
                {
                    threatMap[x, y] = 0;
                }
            }
        }

        foreach (GameObject mobileUnit in mobileUnits)
        {
            int x = (int)mobileUnit.transform.position.x;
            int y = (int)mobileUnit.transform.position.z; // changes the Z position to y to convert from 3d space to the 2d grid

            if (y > LMFF.mapHeight - 1)
            {
                y = LMFF.mapHeight - 1;
            }
            else if (0.0f > y)
            {
                y = 0;
            }


            if (x > LMFF.mapWidth - 1)
            {
                x = LMFF.mapWidth - 1;
            }
            else if (0.0f > x)
            {
                x = 0;
            }

            threatMap[x, y] = airShipMaxInfluence;
        }
    }

    //void createInfluenceMap()
    //{
    //    friendlyUnits = GameObject.FindGameObjectsWithTag("Fighter - Ground");


    //    influenceMap = threatMap.Clone() as float[,]; // clones the threat map, all all of the influence relating to enemies has already been calculated

    //    foreach (GameObject friendlyUnit in friendlyUnits)
    //    {
    //        int x = (int)friendlyUnit.transform.position.x;
    //        int y = (int)friendlyUnit.transform.position.z; // changes the Z position to y to convert from 3d space to the 2d grid

    //        influenceMap[x, y] = friendlyMaxInfluence;
    //    }

    //    foreach (GameObject friendlyUnit in friendlyUnits)
    //    {
    //        for (int x = 0; x < LMFF.mapWidth; x++)
    //        {
    //            for (int y = 0; y < LMFF.mapHeight; y++)
    //            {

    //                float xDistance = Mathf.Abs((int)friendlyUnit.transform.position.x - x);
    //                float yDistance = Mathf.Abs((int)friendlyUnit.transform.position.z - y);
    //                float distance = ((xDistance * xDistance) + (yDistance * yDistance)) * DistanceModifier;


    //                float influenceValue = sourceInfluenceValue - sourceInfluenceValue * (distance / sourceMaxDistance); // http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter30_Modular_Tactical_Influence_Maps.pdf

    //                if (influenceValue > 0)
    //                {
    //                    if (influenceMap[x, y] > MapMaxInfluenceValue)
    //                    {
    //                        influenceValue = MapMaxInfluenceValue;
    //                    }
    //                    influenceMap[x, y] += -influenceValue; // use += to allow dangerous zones to overlap    , this value is inverted as friendly influence is on the scale 0 to -1                 
    //                }
    //            }
    //        }
    //    }

    //}

    void createInfluenceMap()
    {
        friendlyUnits = GameObject.FindGameObjectsWithTag("Fighter - Ground");

        influenceMap = threatMap.Clone() as float[,]; // clones the threat map, all all of the influence relating to enemies has already been calculated

        foreach (GameObject friendlyUnit in friendlyUnits)
        {
            int xPos = (int)friendlyUnit.transform.position.x - (friendlyMaxRange / 2);
            int yPos = (int)friendlyUnit.transform.position.z - (friendlyMaxRange / 2); // changes the Z position to y to convert from 3d space to the 2d grid

            influenceMap[(int)friendlyUnit.transform.position.x, (int)friendlyUnit.transform.position.z] = friendlyMaxInfluence;

            for (int x = 0; x < friendlyBakeSize; x++)
            {
                for (int y = 0; y < friendlyBakeSize; y++)
                {
                    if (xPos + x >= 0 && xPos + x < LMFF.mapWidth && yPos + y >= 0 && yPos + y < LMFF.mapHeight)
                    {
                        influenceMap[xPos + x, yPos + y] += friendlyBake[x, y];
                    }

                }
            }
        }

    }

    void updateThreatMapVisual()
    {
        for (int x = 0; x < LMFF.mapWidth; x++)
        {
            for (int y = 0; y < LMFF.mapHeight; y++)
            {
                var quadRenderer = TMQuadArray[x, y].GetComponent<Renderer>();

                colour.a = transparancy;
                colour.r = threatMap[x, y];
                colour.b = 1.0f - threatMap[x, y];
                colour.g = 0.0f;

                //Call SetColor using the shader property name "_Color" and setting the color to red
                quadRenderer.receiveShadows = false;
                quadRenderer.material.SetColor("_Color", colour);
            }
        }
    }

    void updateInfluenceMapVisual()
    {
        for (int x = 0; x < LMFF.mapWidth; x++)
        {
            for (int y = 0; y < LMFF.mapHeight; y++)
            {
                var quadRenderer = IMQuadArray[x, y].GetComponent<Renderer>();

                colour.a = transparancy;
                colour.r = influenceMap[x, y];
                colour.b = 0 - influenceMap[x, y];
                colour.g = 0.0f;

                //Call SetColor using the shader property name "_Color" and setting the colour to red
                quadRenderer.receiveShadows = false;
                quadRenderer.material.SetColor("_Color", colour);
                //IMQuadArray[y, x].GetComponent<MeshRenderer>().material.SetColor("_color", colour);
            }
        }
    }

    void updateThreatMap()
    {
        zeroThreatMap();
        //// loop for processing the influence map and radiating 'danger' ///
        radiateInfluence();
        // load the maps visual
        updateThreatMapVisual();

        timeTillTMUpdate = 1.0f;
    }

    void updateInfluenceMap()
    {
        // update the friendly influence map
        createInfluenceMap();
        // load the maps visual
        updateInfluenceMapVisual();

        timeTillIMUpdate = 1.0f;
    }

    public void removeDangerSource(int xPos, int yPos)
    {
        LMFF.map[xPos, yPos] = 0;

        for (int i = 0; i < dangerSources.Count; i++)
        {
            if (dangerSources[i].x == xPos && dangerSources[i].y == yPos)
                dangerSources.RemoveAt(i);
        }
    }
}




