using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///Level Generation Refences
///The following were used and adapted to assist me to produce this code.
/// Romain Beuadon (2018) Random 2D dungeon Generator source code and tutorial  [Source code]. http://www.rombdn.com/blog/2018/01/12/random-dungeon-bsp-unity/
/// Unkown (Unkown) Basic BSP  Dungeon Generation tutorial [Website].  http://www.roguebasin.com/index.php/Basic_BSP_Dungeon_generation

/// This class handles a single node within the BSP tree. 
/// Really, each node represents a room aside from our first node.  

public class Node
{

    //The root node will have no parent and will be the size of the entire board.
    public BSPTree tree;

    //perhaps store a grid of the 'spawnable' room space.

    public Node parent, left, right;

    public Rect rect;

    public Rect roomArea;

    public Corridor corridor;
    public List<Rect> connectingCorridors;

    //Not used
    public List<Direction> corridorDirection;

    public List<Rect> doorways;

    public bool[,] editableArea;

    public Node(Rect nrect, BSPTree aTree)
    {
        rect = nrect;
        tree = aTree;
        corridor = new Corridor();
        connectingCorridors = new List<Rect>();
        corridorDirection = new List<Direction>();
        doorways = new List<Rect>();
        

    }

    public bool LeafTest()
    {
        //If the node doesn't have a left or right node then it's the last node in the tree.
        return left == null && right == null;
    }

    public Rect GetRoomArea()
    {
        // Returns the room rect object - 
        if (LeafTest())
        {
            return roomArea;
        }
        if (left != null)
        {
            Rect lRoom = left.GetRoomArea();
            if (lRoom.x != -1 && lRoom.y != -1 && lRoom.width != 0)
            {
                return lRoom;
            }
        }
        if (right != null)
        {
            Rect rRoom = right.GetRoomArea();
            if (rRoom.x != -1 && rRoom.y != -1 && rRoom.width != 0)
            {
                return rRoom;
            }
        }


        return new Rect(-1, -1, 0, 0);
    }

    public void CreateCorridorBetween(Node left, Node right)
    {
        //Creates a corridor to the middle of one room to the middle of another.
        //Doesn't actually build the Corridor, just makes it in memory so the build corridor's function can make it look pretty later.
        Rect lRoom = left.GetRoomArea();
        Rect rRoom = right.GetRoomArea();
        corridor.lRoom = left;
        corridor.rRoom = right;

        //Debug.Log("Creating Corridor");

        Vector2 lPoint = new Vector2((int)Random.Range(lRoom.xMin + 3, lRoom.xMax - 3), (int)Random.Range(lRoom.yMin + 3, lRoom.yMax - 3));
        Vector2 rPoint = new Vector2((int)Random.Range(rRoom.xMin + 3, rRoom.xMax - 3), (int)Random.Range(rRoom.yMin + 3, rRoom.yMax - 3));
        //Vector2 lPoint = new Vector2(0,0);
        //Vector2 rPoint = new Vector2(0,0);


        if (lPoint.x > rPoint.x)
        {
            //Swap them if left point is on the right.
            Vector2 temp = lPoint;
            lPoint = rPoint;
            rPoint = temp;
        }

        int width = (int)(lPoint.x - rPoint.x);
        int height = (int)(lPoint.y - rPoint.y);


        //If width isn't 0 the points are not aligned horizontally. atleast not pefectly.
        if (width != 0)
        {

            //choose
            if (Random.Range(0, 1) == 1)
            {
                //Corridor to the right
                connectingCorridors.Add(new Rect(lPoint.x, lPoint.y, Mathf.Abs(width) + 1, 1));
                corridorDirection.Add(Direction.East);


                // if left point is below right point go up
                //otherwise go down

                if (height < 0)
                {
                    connectingCorridors.Add(new Rect(rPoint.x, lPoint.y, 1, Mathf.Abs(height)));
                    corridorDirection.Add(Direction.North);
                }
                else
                {
                    connectingCorridors.Add(new Rect(rPoint.x, lPoint.y, 1, -Mathf.Abs(height)));
                    corridorDirection.Add(Direction.South);
                }




            }
            else
            {
                //if points are horizontal go up or down.

                if (height < 0)
                {
                    connectingCorridors.Add(new Rect((int)lPoint.x, (int)lPoint.y, 1, Mathf.Abs(height)));
                    corridorDirection.Add(Direction.North);

                }
                else
                {
                    connectingCorridors.Add(new Rect((int)lPoint.x, (int)rPoint.y, 1, Mathf.Abs(height)));
                    corridorDirection.Add(Direction.South);
                }

                connectingCorridors.Add(new Rect((int)lPoint.x, (int)rPoint.y, Mathf.Abs(width) + 1, 1));
                corridorDirection.Add(Direction.East);
            }


        }
        else
        {
            if (height < 0)
            {
                connectingCorridors.Add(new Rect((int)lPoint.x, (int)lPoint.y, 1, Mathf.Abs(height)));
                corridorDirection.Add(Direction.North);
            }
            else
            {
                connectingCorridors.Add(new Rect((int)rPoint.x, (int)rPoint.y, 1, Mathf.Abs(height)));
                corridorDirection.Add(Direction.South);
            }
        }

        corridor.SetCorridorArea(connectingCorridors);

    }


    public void SetRight(Node n)
    {
        right = n;
    }

    public void SetLeft(Node n)
    {
        left = n;
    }

    public bool split(int minRoomSize, int maxRoomSize)
    {
        if (!LeafTest())
        {
            return false;
        }
        //During the first stage of dungeon generation while we're still setting up our BSP fully, we wannt split our nodes in two.
        //In order to do this, we're going to cut the space within our own node randomly (each node represents some amount of 2D space we've got to play with.)

        //Choose to split the parition horizontally or vertically,
        //In order to make sure we end up with actulllay playable levels,
        //We do this based off the size i.e. too long split horizontal otherwise split vertical
        // If both are basically square, we'll choose randomly adds more variance.

        //look at the math here
        bool splitH;
        if (rect.width / rect.height >= 1.25)
        {
            splitH = false;
        }
        else if (rect.height / rect.width >= 1.25)
        {
            splitH = true;
        }
        else
        {
            splitH = Random.Range(0.0f, 1.0f) > 0.5;
        }


        if (Mathf.Min(rect.height, rect.width) / 2 < minRoomSize)
        {
            //Debug.Log("Dungeon Generator has reached a node that will become a leaf.");
            return false;
        }


        if (splitH)
        {
            // split so that the resulting paritions widths are not too small
            // Split Horizontally
            int split = Random.Range(minRoomSize, (int)(rect.width - minRoomSize));

            left = new Node(new Rect(rect.x, rect.y, rect.width, split), tree);
            right = new Node(
                new Rect(rect.x, rect.y + split, rect.width, rect.height - split), tree);
        }
        else
        {
            //Split Vertically
            int split = Random.Range(minRoomSize, (int)(rect.height - minRoomSize));

            left = new Node(new Rect(rect.x, rect.y, split, rect.height), tree);
            right = new Node(
                new Rect(rect.x + split, rect.y, rect.width - split, rect.height), tree);
        }

        return true;

    }


    public void CreateRoom()
    {
        //This function actually assigns our 2D space to our node.
        if (left != null)
        {
            left.CreateRoom();
        }
        if (right != null)
        {
            right.CreateRoom();
        }
        if (left != null && right != null)
        {
            // Create a corridor.
            CreateCorridorBetween(left, right);
        }
        if (LeafTest())
        {
            //Consider squaring these size
            int roomWidth = (int)Random.Range(rect.width / 2, rect.width - 2);
            int roomHeight = (int)Random.Range(rect.height / 2, rect.height - 2);
            int roomX = (int)Random.Range(1, rect.width - roomWidth - 1);
            int roomY = (int)Random.Range(1, rect.height - roomHeight - 1);

            // New code
            if (roomWidth <= 3)
            {
                roomWidth = Random.Range(4, 6);
            }

            if (roomHeight <= 4)
            {
                roomHeight = Random.Range(5, 7);
            }


            // position is abosulte in world that corrlates to the grid.
            roomArea = new Rect(rect.x + roomX, rect.y + roomY, roomWidth, roomHeight);

            //add +2 to the minium vales to get the actual cell position?
            editableArea = new bool[(int)roomArea.xMax - 2, (int)roomArea.yMax - 2];

            //map.SetTile(new Vector3Int(roomX,roomY,0), ft);
            //roomData = new Room();
            tree.rooms.Add(this);
            //Debug.Log("Created room.");

        }
    }

    public void AddDoorway(Rect aDoor)
    {
        //This code doesn't do anything, Yet. For trapped doorways.
        doorways.Add(aDoor);
    }

}
