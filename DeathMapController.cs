using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathMapController : MonoBehaviour
{
    [SerializeField]
    LoadDeathMapFromFile LMFF;
    int layer_mask;

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


    const float friendlyMaxInfluence = -1.0f;
    const int friendlyMaxRange = 6;
    const int friendlyBakeSize = friendlyMaxRange + 1;
    private float[,] friendlyBake;

    // the maximum values of influence a source can radiate

    const float defenderMaxInfluence = 1.0f;

    // The maximum ranges a sources influence can spread

    const int defenderMaxRange = 6;


    // the size of the 2D array used to hold the pre baked influence. (these are increased by 1 to allow a "center point" to make superimposing over the full influence map easier
    const int defenderBakeSize = defenderMaxRange + 1;

    private float[,] defenderBake;


    public int fighterDeath = 10;

    const float influenceMapUpdateFrequency = 1.0f;

    List<MapNode> defenderUnits = new List<MapNode>();

    public GameObject[] friendlyUnits;

    // int[,] map = LoadMapFromFile.map;
    public float[,] threatMap;// = new float[LoadMapFromFile.mapSize, LoadMapFromFile.mapSize]; // the map containing just enemies, used for pathing
    public float[,] influenceMap;// = new float[LoadMapFromFile.mapSize, LoadMapFromFile.mapSize]; // thhe map containing enemies and friendlies, used for decision making
    float transparancy = 0.5f;
    Color colour;
    struct MapNode
    {
        public int x;
        public int y;
        public MapNode(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    };
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
        layer_mask = LayerMask.GetMask("Wall");
        LMFF = GameObject.Find("LoadDeathMapFromFile").GetComponent<LoadDeathMapFromFile>();

        IMQuadArray = new GameObject[LMFF.mapWidth, LMFF.mapHeight]; //= GameObject.CreatePrimitive(PrimitiveType.Sphere);
        TMQuadArray = new GameObject[LMFF.mapWidth, LMFF.mapHeight]; //= GameObject.CreatePrimitive(PrimitiveType.Sphere);

        threatMap = new float[LMFF.mapWidth, LMFF.mapHeight]; // the map containing just enemies, used for pathing
        influenceMap = new float[LMFF.mapWidth, LMFF.mapHeight]; // thhe map containing enemies and friendlies, used for decision making

        defenderBake = new float[defenderBakeSize, defenderBakeSize];
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


        InvokeRepeating("updateThreatMap", 1.0f, influenceMapUpdateFrequency); // calls the update influence map funtions at set intervals instead of every frame, starting after 1 second
        InvokeRepeating("updateInfluenceMap", 1.15f, influenceMapUpdateFrequency); // calls the update influence map funtions at set intervals instead of every frame, starting after 1 second
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

        if(IMEnabled)
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

        foreach (MapNode defenderUnit in defenderUnits)
        {
            xPos = defenderUnit.x - (defenderMaxRange / 2);
            yPos = defenderUnit.y - (defenderMaxRange / 2);

            for (int x = 0; x < defenderBakeSize; x++)
            {
                for (int y = 0; y < defenderBakeSize; y++)
                {
                    if (xPos + x >= 0 && xPos + x < LMFF.mapWidth && yPos + y >= 0 && yPos + y < LMFF.mapHeight)
                    {
                        if (!WallPresent(defenderUnit.x, defenderUnit.y, xPos + x, yPos + y))
                        {
                            threatMap[xPos + x, yPos + y] += defenderBake[x, y];
                        }
                    }
                }
            }
        }
    }

    void zeroThreatMap()
    {
        threatMap = new float[LMFF.mapWidth, LMFF.mapHeight];
        foreach (MapNode defenderUnit in defenderUnits)
        {

            threatMap[defenderUnit.x, defenderUnit.y] = defenderMaxInfluence;
        }
    }

   
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

                //Call SetColor using the shader property name "_Color" and setting the color to red
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

    public void AddDeath(int xPos, int yPos)
    {
        MapNode newDeath = new MapNode();
        newDeath.x = xPos;
        newDeath.y = yPos;

        defenderUnits.Add(newDeath);

        LMFF.map[xPos, yPos] = 1;
    }

    private bool WallPresent(int originX, int originY, int currentX, int currentY)
    {
        RaycastHit hit;
        float rayHeight = 0.8f; // a height that ensures the ray passes through any wall

        Vector3 rayStart = new Vector3(originX, rayHeight, originY);
        Vector3 rayend = new Vector3(currentX, rayHeight, currentY);

        if (Physics.Linecast(rayStart, rayend, out hit, layer_mask))
        {
            if (hit.transform.tag == "Wall")
            {  
                return true;
            }
        }
        return false;
    }
}




