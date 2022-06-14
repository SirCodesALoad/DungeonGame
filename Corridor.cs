using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor
{

    public Node lRoom;
    public Node rRoom;

    //enum direction { north,east,south,west }; //directions are conncered left to right

    public List<Rect> connectingCorridors;

    public Corridor()
    {
        connectingCorridors = new List<Rect>();
    }

    public List<Rect> GetCorridorArea()
    {
        return connectingCorridors;
    }

    public void SetCorridorArea(List<Rect> cc)
    {
        connectingCorridors = cc;
    }

}
