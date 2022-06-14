using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

///Level Generation Refences
///The following were used and adapted to assist me to produce this code.
/// Romain Beuadon (2018) Random 2D dungeon Generator source code and tutorial  [Source code]. http://www.rombdn.com/blog/2018/01/12/random-dungeon-bsp-unity/
/// Unkown (Unkown) Basic BSP  Dungeon Generation tutorial [Website].  http://www.roguebasin.com/index.php/Basic_BSP_Dungeon_generation

public class BSPTree
{
    public Node root;

    public static int loopEscapeLimit = 100000;
    public static int loopEscapeCounter = 0;

    public int maxRoomSize;
    public int minRoomSize;

    public List<Node> rooms;

    Tilemap floormap;

    Tilemap wallMap;

    Tileset ts;

    Node spawnRoom;
    Node exitRoom;

    public bool done = false;

    int totalNumOfBoons;
    int totalNumOfCurses;
    int maxNumEnemyPerRoom;

    public BSPTree(int levelRows, int levelColumns, int emaxRoomSize, int eminRoomSize, Tilemap floorm, Tilemap wallm, Tileset tsa)
    {
        root = new Node(new Rect(0, 0, (float)levelRows, (float)levelColumns),this);


        floormap = floorm;
        wallMap = wallm;
        ts = tsa;

        rooms = new List<Node>();

        maxRoomSize = emaxRoomSize;
        minRoomSize = eminRoomSize;
        //Now we start the process of splitting all the paritions.
        paritionSplit(root);


    }

    public void paritionSplit(Node n)
    {

        //This function runs recursively and it's what splits up our map randomly until we have hit the limit of how many times we want to recure
        //Or we've run out of space for another Viable split to occur without it overlapping. The node.split function actually handles the splitting
        //But this function is the one that recursively runs down our entire tree.

        if (loopEscapeCounter >= loopEscapeLimit)
        {
            //Abort
            Debug.LogError("LevelGeneration Recursive Loop Barrier Hit! " +
                "This wasn't suppose to happen some funky stuff might go down.");


        }
        else
        {
            //Let's keep splitting the nodes (paritions) down until we get nice sized rooms.


            if(n.LeafTest() == true)
            {

                if(n.rect.width > maxRoomSize || n.rect.height > maxRoomSize 
                    || Random.Range(0.0f,1.0f) > 0.25f)
                {
                    if (n.split(minRoomSize, maxRoomSize))
                    {

                        paritionSplit(n.left);
                        paritionSplit(n.right);
                        loopEscapeCounter++;
                        //We increment after we've calculated both sides this means 
                        //we always process two splits.
                    }
                }

            }

        }



    }

    public void BuildRooms(Node n)
    {
        //This function actually builds the dungeon visually. It places the correct tile in the correct area for it according to the rules of the tileset.
        //And then it recursively runs on every other node.

        if (n == null)
        {
            return;
        }


        if (n.LeafTest())
        {
            for (int i = (int)n.roomArea.xMin; i <= (int)n.roomArea.xMax; i++)
            {
                for (int j = (int)n.roomArea.yMin; j <= (int)n.roomArea.yMax; j++)
                {


                    if (i >= (int)n.roomArea.xMin && j == (int)n.roomArea.yMin)
                    {
                        //This is on the bottom edge.
                        //Debug.Log("Room Creation: Bot Edge");
                        floormap.SetTile(new Vector3Int(i, j, 0), ts.GetFloorTile());
                        if (i == n.roomArea.xMin)
                        {
                            wallMap.SetTile(new Vector3Int(i, j, 0), ts.GetWallCorner("BotLeft"));
                        }
                        else if (i == n.roomArea.xMax)
                        {
                            wallMap.SetTile(new Vector3Int(i, j, 0), ts.GetWallCorner("BotRight"));
                        }
                        else
                        {
                            wallMap.SetTile(new Vector3Int(i, j, 0), ts.GetWall("Bot"));
                        }
                    }
                    else if (i >= (int)n.roomArea.xMin && j == (int)n.roomArea.yMax)
                    {
                        //Debug.Log("Room Creation: Top Edge");
                        //This is on the top edge.
                        floormap.SetTile(new Vector3Int(i, j, 0), ts.GetFullWall());
                        if (i == n.roomArea.xMin)
                        {
                            wallMap.SetTile(new Vector3Int(i, j, 0), ts.GetWallCorner("TopLeft"));
                        }
                        else if (i == n.roomArea.xMax)
                        {
                            wallMap.SetTile(new Vector3Int(i, j, 0), ts.GetWallCorner("TopRight"));
                        }
                        else
                        {
                            wallMap.SetTile(new Vector3Int(i, j, 0), ts.GetWall("Top"));
                        }
                    }
                    else if (i == (int)n.roomArea.xMin && j >= (int)n.roomArea.yMin)
                    {
                        //Debug.Log("Room Creation: Left Edge");
                        //This is on the left edge.
                        floormap.SetTile(new Vector3Int(i, j, 0), ts.GetFloorTile());
                        wallMap.SetTile(new Vector3Int(i, j, 0), ts.GetWall("Left"));
                    }
                    else if (i == (int)n.roomArea.xMax && j >= (int)n.roomArea.yMin)
                    {
                        //Debug.Log("Room Creation: Right Edge");
                        //This is on the Right edge.
                        floormap.SetTile(new Vector3Int(i, j, 0), ts.GetFloorTile());
                        wallMap.SetTile(new Vector3Int(i, j, 0), ts.GetWall("Right"));
                    }
                    else
                    {
                        //Debug.Log("Room Creation: Not in an edge");
                        //We are literally anywhere else inside the rectangle let's place a floor.
                        floormap.SetTile(new Vector3Int(i, j, 0), ts.GetFloorTile());
                    }
                }
            }


            //map.RefreshAllTiles();

        }
        else
        {
            BuildRooms(n.left);
            BuildRooms(n.right);
        }
    }


    public void BuildCorridors(Node n)
    {
        //This function builds the dungeon's corrdiros visiauly running recusrively for every corridor attached to every room. (As each room is connected to atleast one other room.

        if (n == null)
        {
            return;
        }

        //Debug.Log("BuildingCorridors");

        BuildCorridors(n.left);
        BuildCorridors(n.right);

        //if (n.corridor == null)
        //{
        //    return;
        //}

        foreach(Rect c in n.corridor.GetCorridorArea())
        {
            
            for (int i = (int)c.x; i <= c.xMax; i++)
            {
                
                for (int j = (int) c.y; j <= c.yMax; j++)
                {
                    Vector3Int pos = new Vector3Int(i, j, 0);
                    //check the first time the corridor enters the room. if its on the edge do this once.

                    if (floormap.GetTile(pos) == null)
                    {
                        floormap.SetTile(pos, ts.GetFloorTile());
                        wallMap.SetTile(pos, ts.GetCorridorWallTile());
                    }
                    //else if(wallMap.GetTile(pos) != null)
                    //{
                        //There is a floor tile and there is a wall on it.
                        //Remove the wall tile (place a door tile?)
                      //  floormap.SetTile(pos, ts.GetFloorTile());
                       // wallMap.SetTile(pos, null);
                        //need to know which direction the corridor is going.

                    //}
                }
            }
        }


    }

    public void BuildCorridorWalls(Node n)
    {
        //Like all the others, this function recursively builds the dungeon's walls visually according to the rules of the tile set.


        if (n == null)
        {
            return;
        }

        //Debug.Log("Building: Corridor Walls");

        BuildCorridorWalls(n.left);
        BuildCorridorWalls(n.right);


        foreach (Rect c in n.corridor.GetCorridorArea())
        {

            for (int i = (int)c.x; i <= c.xMax; i++)
            {

                for (int j = (int)c.y; j <= c.yMax; j++)
                {
                    Vector3Int pos = new Vector3Int(i, j, 0);
                    //check the first time the corridor enters the room. if its on the edge do this once.

                    if (wallMap.GetTile(pos) == null)
                    {
                        wallMap.SetTile(pos, ts.GetCorridorWallTile());
                    }
                    
                }
            }
        }


    }



    public void BuildDoorways(Node n)
    {
        //Finds the room's doorways and replaces the apporiate tiles to be walkable.
        //Does this by checking outside of the room's bounds by one and attempting to
        //find a chain of tiles.
        //Probably the largest function in here.

        if (n == null)
        {
            return;
        }

        //Debug.Log("Building: Doorways");


        if (n.LeafTest() && n.roomArea != null || n.roomArea.width == 0)
        {
            
            int nXMin = (int)n.roomArea.xMin - 1;
            int nXMax = (int)n.roomArea.xMax + 1;
            int nYMin = (int)n.roomArea.yMin - 1;
            int nYMax = (int)n.roomArea.yMax + 1;
            //Check every tile around the room to see if it has a floor tile..
            //If it does let's create a dooorwar along one of the edges.

            //check top, right, bottom and left.

            int numOftiles = 0;
            bool tileChain = false;

            for (int i = nXMin + 1; i <= nXMax - 1; i++)
            {
                //Check top edge.
                //For each tile in top edge check if there is a corridor (i.e. a floor tile.)
                if (floormap.GetTile(new Vector3Int(i, nYMax, 0)) != null)
                {
                    //If we've found a floor tile. that must mean there is a corridor close to this room.
                    tileChain = true;
                    numOftiles++;
                }
                else if (tileChain == true)
                {
                    //If there is no tile and we still have an existing tile chain. Lets create a door.
                    if (numOftiles != 1 && numOftiles != 0)
                    {
                        int tileNum = Random.Range((int)i - numOftiles, i - 2);
                        //place a door
                        floormap.SetTile(new Vector3Int(tileNum, nYMax - 1, 0), ts.GetFloorTile());
                        wallMap.SetTile(new Vector3Int(tileNum, nYMax - 1, 0), null);
                        floormap.SetTile(new Vector3Int(tileNum + 1, nYMax - 1, 0), ts.GetFloorTile());
                        wallMap.SetTile(new Vector3Int(tileNum + 1, nYMax - 1, 0), null);

                        if(tileNum+1 == nXMax - 1)
                        {
                            //we#re on the corner let's place a wall to act as an edge.
                            wallMap.SetTile(new Vector3Int(tileNum + 1, nYMax - 1, 0), ts.GetWall("Right"));
                        }
                        if (tileNum == nXMin + 1)
                        {
                            //we#re on the corner let's place a wall to act as an edge.
                            wallMap.SetTile(new Vector3Int(tileNum, nYMax - 1, 0), ts.GetWall("Left"));
                        }

                        Rect aDoor = new Rect(tileNum, nYMax - 1, 2, 1);
                        n.AddDoorway(aDoor);

                    }
                    //else
                    //{
                    //    floormap.SetTile(new Vector3Int(i-1, nYMax - 1, 0), ts.GetFloorTile());
                    //    wallMap.SetTile(new Vector3Int(i-1, nYMax - 1, 0), null);
                    //}

                    numOftiles = 0;
                    tileChain = false;
                }
            }

            //Cleanup incase we get a tile that goes out of 'bounds'
            if (tileChain == true)
            {
                int tileNum = Random.Range((int)nXMax - 1 - numOftiles, nXMax - 2);
                //place a door
                floormap.SetTile(new Vector3Int(tileNum, nYMax - 1, 0), ts.GetFloorTile());
                wallMap.SetTile(new Vector3Int(tileNum, nYMax - 1, 0), null);
                floormap.SetTile(new Vector3Int(tileNum + 1, nYMax - 1, 0), ts.GetFloorTile());
                wallMap.SetTile(new Vector3Int(tileNum + 1, nYMax - 1, 0), null);
                numOftiles = 0;
                tileChain = false;
                Rect aDoor = new Rect(tileNum, nYMax - 1, 2, 1);
                n.AddDoorway(aDoor);
                if (tileNum + 1 == nXMax - 1)
                {
                    //we#re on the corner let's place a wall to act as an edge.
                    wallMap.SetTile(new Vector3Int(tileNum + 1, nYMax - 1, 0), ts.GetWall("Right"));
                }
                if (tileNum == nXMin + 1)
                {
                    //we#re on the corner let's place a wall to act as an edge.
                    wallMap.SetTile(new Vector3Int(tileNum, nYMax - 1, 0), ts.GetWall("Left"));
                }
            }



            for (int i = nXMin + 1; i <= nXMax - 1; i++)
            {
                //Check bot edge.
                //For each tile in top edge check if there is a corridor (i.e. a floor tile.)
                if (floormap.GetTile(new Vector3Int(i, nYMin, 0)) != null)
                {
                    //If we've found a floor tile. that must mean there is a corridor close to this room.
                    tileChain = true;
                    numOftiles++;
                }
                else if (tileChain == true)
                {
                    //If there is no tile and we still have an existing tile chain. Lets create a door.
                    if (numOftiles != 1 && numOftiles != 0)
                    {
                        int tileNum = Random.Range((int)i - numOftiles, i - 2);
                        //place a door
                        floormap.SetTile(new Vector3Int(tileNum, nYMin + 1, 0), ts.GetFloorTile());
                        wallMap.SetTile(new Vector3Int(tileNum, nYMin + 1, 0), null);
                        floormap.SetTile(new Vector3Int(tileNum + 1, nYMin + 1, 0), ts.GetFloorTile());
                        wallMap.SetTile(new Vector3Int(tileNum + 1, nYMin + 1, 0), null);
                        Rect aDoor = new Rect(tileNum, nYMin + 1, 2, 1);
                        n.AddDoorway(aDoor);
                        if (tileNum + 1 == nXMax - 1)
                        {
                            //we#re on the corner let's place a wall to act as an edge.
                            wallMap.SetTile(new Vector3Int(tileNum + 1, nYMin + 1, 0), ts.GetWall("Right"));
                        }
                        if (tileNum == nXMin + 1)
                        {
                            //we#re on the corner let's place a wall to act as an edge.
                            wallMap.SetTile(new Vector3Int(tileNum , nYMin + 1, 0), ts.GetWall("Left"));
                        }
                    }
                    //else
                    //{
                    //    floormap.SetTile(new Vector3Int(i - 1, nYMax + 1, 0), ts.GetFloorTile());
                    //    wallMap.SetTile(new Vector3Int(i - 1, nYMax + 1, 0), null);
                    //}


                    numOftiles = 0;
                    tileChain = false;
                }
            }
            if (tileChain == true)
            {
                int tileNum = Random.Range((int)nXMax - numOftiles, nXMax - 2);
                //place a door
                floormap.SetTile(new Vector3Int(tileNum, nYMin + 1, 0), ts.GetFloorTile());
                wallMap.SetTile(new Vector3Int(tileNum, nYMin + 1, 0), null);
                floormap.SetTile(new Vector3Int(tileNum + 1, nYMin + 1, 0), ts.GetFloorTile());
                wallMap.SetTile(new Vector3Int(tileNum + 1, nYMin + 1, 0), null);
                numOftiles = 0;
                tileChain = false;
                Rect aDoor = new Rect(tileNum, nYMin + 1, 2, 1);
                n.AddDoorway(aDoor);
                if (tileNum + 1 == nXMax - 1)
                {
                    //we#re on the corner let's place a wall to act as an edge.
                    wallMap.SetTile(new Vector3Int(tileNum + 1, nYMin + 1, 0), ts.GetWall("Right"));
                }
                if (tileNum == nXMin + 1)
                {
                    //we#re on the corner let's place a wall to act as an edge.
                    wallMap.SetTile(new Vector3Int(tileNum, nYMin + 1, 0), ts.GetWall("Left"));
                }
            }

            for (int i = nYMin + 1; i <= nYMax - 1; i++)
            {
                //Check Left edge.
                //For each tile in top edge check if there is a corridor (i.e. a floor tile.)
                if (floormap.GetTile(new Vector3Int(nXMin, i, 0)) != null)
                {
                    //If we've found a floor tile. that must mean there is a corridor close to this room.
                    tileChain = true;
                    numOftiles++;
                }
                else if (tileChain == true)
                {
                    //If there is no tile and we still have an existing tile chain. Lets create a door.
                    if (numOftiles != 1 && numOftiles != 0)
                    {
                        int tileNum = Random.Range((int)i - numOftiles, i - 2);
                        //place a door
                        floormap.SetTile(new Vector3Int(nXMin + 1, tileNum, 0), ts.GetFloorTile());
                        wallMap.SetTile(new Vector3Int(nXMin + 1, tileNum, 0), null);
                        floormap.SetTile(new Vector3Int(nXMin + 1, tileNum + 1, 0), ts.GetFloorTile());
                        wallMap.SetTile(new Vector3Int(nXMin + 1, tileNum + 1, 0), null);
                        Rect aDoor = new Rect(nXMin + 1, tileNum, 1, 2);
                        n.AddDoorway(aDoor);
                        if (tileNum + 1 == nYMax - 1)
                        {
                            //we#re on the corner let's place a wall to act as an edge.
                            wallMap.SetTile(new Vector3Int(nXMin + 1, tileNum + 1, 0), ts.GetWall("Top"));
                        }
                        if (tileNum == nYMin + 1)
                        {
                            //we#re on the corner let's place a wall to act as an edge.
                            wallMap.SetTile(new Vector3Int(nXMin + 1, tileNum , 0), ts.GetWall("Bot"));
                        }

                    }
                    //else
                    //{
                    //    floormap.SetTile(new Vector3Int(nXMin + 1, i-1, 0), ts.GetFloorTile());
                    //    wallMap.SetTile(new Vector3Int(nXMin + 1, i - 1, 0), null);
                    //}

                    numOftiles = 0;
                    tileChain = false;
                }
            }

            if (tileChain == true)
            {
                int tileNum = Random.Range((int)nYMax - numOftiles, nYMax - 2);
                //place a door
                floormap.SetTile(new Vector3Int(nXMin + 1, tileNum, 0), ts.GetFloorTile());
                wallMap.SetTile(new Vector3Int(nXMin + 1, tileNum, 0), null);
                floormap.SetTile(new Vector3Int(nXMin + 1, tileNum + 1, 0), ts.GetFloorTile());
                wallMap.SetTile(new Vector3Int(nXMin + 1, tileNum + 1, 0), null);
                numOftiles = 0;
                tileChain = false;
                Rect aDoor = new Rect(nXMin + 1, tileNum, 1, 2);
                n.AddDoorway(aDoor);
                if (tileNum + 1 == nYMax - 1)
                {
                    //we#re on the corner let's place a wall to act as an edge.
                    wallMap.SetTile(new Vector3Int(nXMin + 1, tileNum + 1, 0), ts.GetWall("Top"));
                }
                if (tileNum == nYMin + 1)
                {
                    //we#re on the corner let's place a wall to act as an edge.
                    wallMap.SetTile(new Vector3Int(nXMin + 1, tileNum, 0), ts.GetWall("Bot"));
                }
            }


            for (int i = nYMin + 1; i <= nYMax - 1; i++)
            {
                //Check Right edge.
                //For each tile in top edge check if there is a corridor (i.e. a floor tile.)
                if (floormap.GetTile(new Vector3Int(nXMax, i, 0)) != null)
                {
                    //If we've found a floor tile. that must mean there is a corridor close to this room.
                    tileChain = true;
                    numOftiles++;
                }
                else if (tileChain == true)
                {
                    //If there is no tile and we still have an existing tile chain. Lets create a door.
                    if (numOftiles != 1 && numOftiles != 0)
                    {
                        int tileNum = Random.Range((int)i - numOftiles, i - 2);
                        //place a door
                        floormap.SetTile(new Vector3Int(nXMax - 1, tileNum, 0), ts.GetFloorTile());
                        wallMap.SetTile(new Vector3Int(nXMax - 1, tileNum, 0), null);
                        floormap.SetTile(new Vector3Int(nXMax - 1, tileNum + 1, 0), ts.GetFloorTile());
                        wallMap.SetTile(new Vector3Int(nXMax - 1, tileNum + 1, 0), null);
                        Rect aDoor = new Rect(nXMax - 1, tileNum, 1, 2);
                        n.AddDoorway(aDoor);
                        if (tileNum + 1 == nYMax - 1)
                        {
                            //we#re on the corner let's place a wall to act as an edge.
                            wallMap.SetTile(new Vector3Int(nXMax - 1, tileNum + 1, 0), ts.GetWall("Top"));
                        }
                        if (tileNum == nYMin + 1)
                        {
                            //we#re on the corner let's place a wall to act as an edge.
                            wallMap.SetTile(new Vector3Int(nXMax - 1, tileNum, 0), ts.GetWall("Bot"));
                        }
                    }
                    //else
                    //{
                    //    floormap.SetTile(new Vector3Int(nXMax - 1, i - 1, 0), ts.GetFloorTile());
                    //    wallMap.SetTile(new Vector3Int(nXMax - 1, i - 1, 0), null);
                    //}

                    numOftiles = 0;
                    tileChain = false;
                }
            }
            if (tileChain == true)
            {
                int tileNum = Random.Range((int)nYMax - numOftiles, nYMax - 2);
                //place a door
                floormap.SetTile(new Vector3Int(nXMax - 1, tileNum, 0), ts.GetFloorTile());
                wallMap.SetTile(new Vector3Int(nXMax - 1, tileNum, 0), null);
                floormap.SetTile(new Vector3Int(nXMax - 1, tileNum + 1, 0), ts.GetFloorTile());
                wallMap.SetTile(new Vector3Int(nXMax - 1, tileNum + 1, 0), null);
                numOftiles = 0;
                tileChain = false;
                Rect aDoor = new Rect(nXMax - 1, tileNum, 1, 2);
                n.AddDoorway(aDoor);
                if (tileNum + 1 == nYMax - 1)
                {
                    //we#re on the corner let's place a wall to act as an edge.
                    wallMap.SetTile(new Vector3Int(nXMax - 1, tileNum + 1, 0), ts.GetWall("Top"));
                }
                if (tileNum == nYMin + 1)
                {
                    //we#re on the corner let's place a wall to act as an edge.
                    wallMap.SetTile(new Vector3Int(nXMax - 1, tileNum, 0), ts.GetWall("Bot"));
                }
            }
        }

        BuildDoorways(n.left);
        BuildDoorways(n.right);

    }
    public void ChooseSpawnRoom()
    {
        //For each room get it's size.
        //Choose the smallest one as the spawn room.

        List<Node> currentSmall = new List<Node>();
        currentSmall.Add(rooms[0]);

        foreach(Node r in rooms)
        {
            if (r == currentSmall[0])
            {
                continue;
            }

            if (r.roomArea.width <= currentSmall[0].roomArea.width && r.roomArea.height <= currentSmall[0].roomArea.height)
            {
                if(r.roomArea.width == currentSmall[0].roomArea.width && r.roomArea.height == currentSmall[0].roomArea.height)
                {
                    //if they are equal we add it to the list and decide randomly
                    currentSmall.Add(r);
                }
                else
                {
                    //if not we remove the old one add the new one.
                    currentSmall.Clear();
                    currentSmall.Add(r);

                }


            }
        }

        if (currentSmall.Count > 1)
        {
            spawnRoom = currentSmall[Random.Range(0, currentSmall.Count)];
        }
        else
        {
            //spawnpoint found.
            spawnRoom = currentSmall[0];
        }


    }

    public void ChooseExit()
    {
        Node longest = rooms[0];
        foreach (Node r in rooms)
        {
            if(longest == r)
            {
                continue;
            }

            if (Vector2.Distance(spawnRoom.roomArea.position, r.roomArea.position) > Vector2.Distance(spawnRoom.roomArea.position, longest.roomArea.position))
            {
                longest = r;
            }
        }

        exitRoom = longest;
        Vector3Int exitPos = new Vector3Int((int)Random.Range(longest.roomArea.xMin+1,longest.roomArea.xMax-1), (int)Random.Range(longest.roomArea.yMin + 1, longest.roomArea.yMax - 1), 0);

        floormap.SetTile(exitPos, ts.GetExitTile());





    }


    public void PopulateRooms()
    {
        //Grab spawn lists.

        //The function that decides what's in each room and 
        foreach (Node r in rooms)
        {
            if (r != spawnRoom || r != exitRoom)
            {

                //CurseAndBoon(r);
                //AddMisc(r);
                SpawnEnemies(r);
            }

        }


    }

    public void CurseAndBoon(Node r)
    {

        bool curseTaken = false;

        if(totalNumOfCurses <  rooms.Count - 2)
        {
            if (Random.Range(0, 100) <= 60)
            {
                curseTaken = true;
                CurseTile ct = ts.GetRandomCurseTile();
                Rect ra = r.roomArea;

                if(ct.placementPerfence == PlacementPreference.FloorNearDoor)
                {
                    //for each doorway decide to spawn this tile.
                    foreach (Rect door in r.doorways)
                    {
                        if(door.width == 2)
                        {
                            //top or bottom
                            if(door.y == ra.yMax)
                            {
                                //top
                            }
                            else
                            {
                                //bottom

                            }

                        }
                        else if (door.height == 2)
                        {
                            //left or right.
                            if(door.x == ra.xMax)
                            {
                                //right

                            }
                            else
                            {
                                //left

                            }

                        }

                    }
                }


            }
        }

        if(totalNumOfBoons < rooms.Count / 2 && curseTaken == false)
        {

            if(Random.Range(0, 100) <= 20)
            {

            }

        }

    }



    public void AddMisc(Node r)
    {
        //Adds misc items like urns and crates to rooms

        int numOfObjects = (int)Random.Range(0, 4);
;

        for(int i = 0; i <= numOfObjects; i++)
        {
            LevelObject miscObject = ts.GetMiscObject();

            if(miscObject.sp  == SpawnPreference.Corner)
            {
                Vector2Int pos = new Vector2Int();
                if (Random.Range(0, 1) == 1)
                {
                    //xmin
                    pos.x = (int)Random.Range(r.roomArea.xMin + 1, r.roomArea.xMin + 2);
                    if (Random.Range(0, 1) == 1)
                    {
                        //ymin
                        pos.y = (int)Random.Range(r.roomArea.yMin + 1, r.roomArea.yMin + 2);
                    }
                    else
                    {
                        //ymax
                        pos.y = (int)Random.Range(r.roomArea.yMax - 2, r.roomArea.yMax - 1);
                    }
                }
                else {
                    //xmax
                    pos.x = (int)Random.Range(r.roomArea.xMax - 2, r.roomArea.xMax - 1);
                    if (Random.Range(0, 1) == 1)
                    {
                        //ymin
                        pos.y = (int)Random.Range(r.roomArea.yMin + 1, r.roomArea.yMin + 2);
                    }
                    else
                    {
                        //ymax
                        pos.y = (int)Random.Range(r.roomArea.yMax - 2, r.roomArea.yMax - 1);
                    }
                    
                }

                GameObject.Instantiate(ts.GetRandomStandardEnemy().gameObject, floormap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0)), Quaternion.identity);

            }
            else if (miscObject.sp == SpawnPreference.Tall)
            {

            }
            else if (miscObject.sp == SpawnPreference.Large)
            {

            }

        }


    }

    public void SpawnEnemies(Node r)
    {
        //Spawns enemies in every room based on a spawn chart.
        //Right now just spawns the only enemy we have.



        //Spawn chart stuff..

        //choose a tile to spawn it on which isn't on the edges.

        int numOfTiles = ((int)r.roomArea.xMin + 1 - (int)r.roomArea.xMax - 1) * ((int)r.roomArea.yMin+1 - (int)r.roomArea.yMax-1);
        int maxX = (int)r.roomArea.xMax - 1;
        int maxY = (int)r.roomArea.yMax - 1;
        int minX = (int)r.roomArea.xMin + 1;
        int minY = (int)r.roomArea.yMin + 1;

        List<Vector2Int> spawnPos = new List<Vector2Int>();

        for (int i = 0; i < numOfTiles - 4; i++)
        {

            if (i > maxNumEnemyPerRoom)
            {
                break;
            }
            else
            {
                //Pick an enemy from the spawn chart and spawn it in a random tile. That we've not already used.
                spawnPos.Add(new Vector2Int(Random.Range(minX, maxX), Random.Range(minY, maxY)));
            }
        }
      
        //this may have to be modified to spawn in large enemies.
        foreach(Vector2Int pos in spawnPos)
        {
            GameObject enemy = GameObject.Instantiate(ts.GetRandomStandardEnemy(),floormap.GetCellCenterWorld(new Vector3Int(pos.x,pos.y,0)), Quaternion.identity); 


        }

    }

    public void SetUpSpawnPoints()
    {

        //do this for a number of time equal to the number of players.

        List<Vector2Int> spawnPos = new List<Vector2Int>();

        int numOfTiles = ((int)spawnRoom.roomArea.xMin + 1 - (int)spawnRoom.roomArea.xMax - 1) * ((int)spawnRoom.roomArea.yMin + 1 - (int)spawnRoom.roomArea.yMax - 1);
        int maxX = (int)spawnRoom.roomArea.xMax - 1;
        int maxY = (int)spawnRoom.roomArea.yMax - 1;
        int minX = (int)spawnRoom.roomArea.xMin + 1;
        int minY = (int)spawnRoom.roomArea.yMin + 1;

        spawnPos.Add(new Vector2Int(Random.Range(minX, maxX), Random.Range(minY, maxY)));

        //spawn the players.
        //in this case we'll just change the postion of our only plyer
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        p.transform.position = floormap.CellToWorld(new Vector3Int (spawnPos[0].x,spawnPos[0].y,0));

        done = true;

    }

}

public enum Direction
{
    North, East, West, South
}



