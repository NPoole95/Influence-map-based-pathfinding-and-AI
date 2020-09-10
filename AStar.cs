using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{

    public InfluenceMapController influenceMapController;
    public LoadMapFromFile LMFF;
    public class mapNode
    {
        public int x;
        public int y;

        public float costFromStart;
        public float costToGoal;
        public float totalCost;
        public mapNode parent;

        public mapNode(int xPos, int yPos)
        {
            x = xPos;
            y = yPos;
            costFromStart = 0;
            costToGoal = 0;
            totalCost = 0;
            parent = null;
        }
        public mapNode(mapNode mapNode)
        {
            x = mapNode.x;
            y = mapNode.y;
            costFromStart = mapNode.costFromStart;
            costToGoal = mapNode.costToGoal;
            totalCost = mapNode.totalCost;
            parent = mapNode.parent;
        }
    }

    public struct basicNode
    {
        public int x;
        public int y;

        public basicNode(int xPos, int yPos)
        {
            x = xPos;
            y = yPos;
        }
    }

    List<mapNode> openList;
    List<mapNode> closedList;
    List<basicNode> adjacentTiles;
    public List<mapNode> path;

    mapNode currentNode;
    mapNode startNode;
    mapNode temp;
    mapNode temp2;
    mapNode newNode;
    float NewCost;

    private void Awake()
    {

        //Open: priorityque of searchnode
        openList = new List<mapNode>();
        //Closed: list of searchnode
        closedList = new List<mapNode>();

        adjacentTiles = new List<basicNode>(new[]{new basicNode(0,1), new basicNode(1, 1) , new basicNode(1,0) , new basicNode(1, -1) ,
                                                  new basicNode(0, -1) , new basicNode(-1,-1) , new basicNode(-1, 0) , new basicNode(-1,1) });

        path = new List<mapNode>();
        //public List<Vector3> path = new List<Vector3>();

        influenceMapController = GameObject.Find("InfluenceMapController").GetComponent<InfluenceMapController>();
        LMFF = GameObject.Find("LoadMapFromFile").GetComponent<LoadMapFromFile>();
    }

    public List<mapNode> AStarSearch(int startX, int startY, int goalX, int goalY)
    {

        //clear Open and Closed
        openList.Clear();
        closedList.Clear();
        path.Clear();

        // initialize a start node
        startNode = new mapNode(startX, startY);
        startNode.costToGoal = PathCostEstimate(startX, startY, goalX, goalY);

        //push StartNode on Open
        openList.Add(startNode);

        // Process the list until success or failure
        while (openList.Count != 0)
        {
            //openList.Sort(comparison);
            insertSort(openList);

            // pop Node from Open // Node has lowest TotalCost

            currentNode = new mapNode(openList[0]);
            openList.RemoveAt(0);

            // if at goal, we’re done
            if (currentNode.x == goalX && currentNode.y == goalY)
            {

                //construct a path backward from Node to StartLoc               

                bool foundStart = false;

                temp = currentNode;

                do
                {
                    if (temp.parent == null)
                    {
                        foundStart = true;
                    }
                    else
                    {
                        temp2 = new mapNode(temp.x, temp.y);
                        temp2.parent = temp.parent;
                        temp2.totalCost = temp.totalCost;
                        temp2.costFromStart = temp.costFromStart;
                        temp2.costToGoal = temp.costToGoal;

                        path.Insert(0, temp2);

                        temp.x = temp.parent.x;
                        temp.y = temp.parent.y;
                        temp.totalCost = temp.parent.totalCost;
                        temp.costToGoal = temp.parent.costToGoal;
                        temp.costFromStart = temp.parent.costFromStart;
                        temp.parent = temp.parent.parent;

                        temp2 = new mapNode(0, 0);
                    }
                } while (!foundStart);

                path.Insert(0, startNode);

                return path;

            }
            else
            {
                for (int i = 0; i < adjacentTiles.Count; i++)
                {
                    newNode = new mapNode(currentNode.x + adjacentTiles[i].x, currentNode.y + adjacentTiles[i].y);

                    // check that not checking tile which is out of bounds
                    if (newNode.x >= LMFF.mapWidth || newNode.y >= LMFF.mapHeight || newNode.x < 0 || newNode.y < 0)
                    {
                        continue;
                    }
                    // check that the tile being explored is navigatable
                    if (LMFF.map[newNode.x, newNode.y] == LoadMapFromFile.sWallEnd || LMFF.map[newNode.x, newNode.y] == LoadMapFromFile.sWall || LMFF.map[newNode.x, newNode.y] == LoadMapFromFile.sWallRotated)
                    {
                        if (newNode.x != goalX || newNode.y != goalY)
                        {
                            continue;
                        }

                    }

                    NewCost = currentNode.costFromStart + TraverseCost(/*currentNode, */newNode);

                    // ignore this node if exists and no improvement
                    if (compareCost(openList, newNode.x, newNode.y, NewCost) || compareCost(closedList, newNode.x, newNode.y, NewCost)) // here possibly keep not of if it is in either list, that way the lists do not need to be searched again lower down
                    {
                        continue;
                    }

                    // store the new or improved information
                    newNode.parent = currentNode;

                    newNode.costFromStart = NewCost;

                    newNode.costToGoal = PathCostEstimate(newNode.x, newNode.y, goalX, goalY);

                    newNode.totalCost = newNode.costFromStart + newNode.costToGoal;

                    removeFromList(ref closedList, newNode.x, newNode.y); // if newnode is in closed, remove it

                    // if newnode is in openlist, sort list
                    if (searchList(openList, newNode.x, newNode.y))
                    {
                        //adjust newnodes location on open (insert sort)
                        adjustNode(newNode, ref openList);
                    }
                    else
                    {
                        //pushNewNode onto Open
                        openList.Add(newNode);
                    }
                } // now done with Node
            }
            //push Node onto Closed
            closedList.Add(currentNode);
        }
        return null;       // if no path found and Open is empty
    }

    bool searchList(List<mapNode> list, int x, int y)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].x == x && list[i].y == y)
            {
                return true;
            }
        }
        return false;
    }

    void removeFromList(ref List<mapNode> list, int x, int y)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].x == x && list[i].y == y)
            {
                list.RemoveAt(i);
                break;
            }
        }
    }

    int PathCostEstimate(int startX, int startY, int goalX, int goalY) // manhattan distance?
    {
        return Mathf.Abs(goalX - startX) + Mathf.Abs(goalY - startY);
    }

    float TraverseCost(/*mapNode currentNode, */mapNode newNode) // heuristic value
    {
        try
        {
            return influenceMapController.threatMap[newNode.x, newNode.y] * 20; // this is multiplied as the value of the threat  map is very low, (0-1), the hiugher the multiplying factor, the more the agents will avoid danger
        }
        catch (System.IndexOutOfRangeException)
        {
            Debug.LogError("out of bounds: " + newNode.x + " " + newNode.y);
        }
        return 0;
    }

    static int comparison(mapNode node1, mapNode node2)
    {
        return node1.totalCost.CompareTo(node2.totalCost);
    }


    bool compareCost(List<mapNode> list, int x, int y, float NewCost)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].x == x && list[i].y == y)
            {
                if (list[i].costFromStart <= NewCost)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void adjustNode(mapNode mapNode, ref List<mapNode> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].x == mapNode.x && list[i].y == mapNode.y)
            {
                list[i] = mapNode;
            }
        }
    }

    static void insertSort(List<mapNode> list)
    {
        int n = list.Count;
        int flag;

       mapNode val;

        // Console.WriteLine("Insertion Sort");
        // Console.Write("Initial array is: ");
        // for (i = 0; i < n; i++)
        //  {
        //Console.Write(arr[i] + " ");
        // }
        if (list.Count > 1)
        {
            for (int i = 1; i < n; i++)
            {
                val = list[i];
                flag = 0;
                for (int j = i - 1; j >= 0 && flag != 1;)
                {
                    if (val.totalCost < list[j].totalCost)
                    {
                        list[j + 1] = list[j];
                        j--;
                        list[j + 1] = val;
                    }
                    else flag = 1;
                }
            }
        }
        //Console.Write("\nSorted Array is: ");
        //for (i = 0; i < n; i++)
       // {
       //     Console.Write(arr[i] + " ");
       // }
    }


}
