using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

///Level Generation Refences
///The following were used and adapted to assist me to produce this code.
/// Romain Beuadon (2018) Random 2D dungeon Generator source code and tutorial  [Source code]. http://www.rombdn.com/blog/2018/01/12/random-dungeon-bsp-unity/
/// Unkown (Unkown) Basic BSP  Dungeon Generation tutorial [Website].  http://www.roguebasin.com/index.php/Basic_BSP_Dungeon_generation

/// This class handles the dungeon generation function calls at start up. (So we generate a dungeon before the player can play.
/// IT also handles setting up the Pathfinding layer, so after we generate our dungeon we intalise the pathfinding net, so it knows what our AI agents
/// can and cannot walk on.

public class LevelGenerator : MonoBehaviour
{
    //Technically min room size and max room size are incorrect it actullary represents how far
    //Down the tree goes.
    public int levelRows , LevelColumns, minRoomSize, maxRoomSize;

    public RandomTile floorTile;

    public RuleTile wallTile;

    public RandomTile corridorFloorTile;

    public Tilemap floormap;
    public Tilemap wallmap;
    public Tileset ts; //make a function to change this.

    //public P_Grid pathfindingGrid;

    public AstarPath pfGrid;

    public bool updateGraph = false;

    private BSPTree d;
    

    // Start is called before the first frame update
    void Awake()
    {
        //pfGrid = GetComponent<AstarPath>();
        
        d = new BSPTree(levelRows, LevelColumns, maxRoomSize, minRoomSize, floormap, wallmap, ts);
        d.root.CreateRoom();
        d.BuildRooms(d.root);
        d.BuildCorridors(d.root);
        d.BuildDoorways(d.root);
        d.ChooseSpawnRoom();
        d.ChooseExit();
        d.PopulateRooms();
        d.SetUpSpawnPoints();

        floormap.RefreshAllTiles();
        wallmap.RefreshAllTiles();

        //pathfindingGrid.CreateGrid(new Vector2(levelRows,LevelColumns));
        AstarPath.active.data.gridGraph.center = floormap.GetCellCenterWorld(new Vector3Int(levelRows / 2, LevelColumns / 2, 0));
        AstarPath.active.data.gridGraph.SetDimensions(Mathf.RoundToInt(levelRows / 0.1f), Mathf.RoundToInt(LevelColumns / 0.1f), 0.1f);
        //AstarPath.active.Scan();
        
        //Redo tileset spawning rules. I.e. make unwalkable tiles unwalkable.
        //AstarPath.active.data.gridGraph.depth = Mathf.RoundToInt(LevelColumns / 0.1f);
    }

    IEnumerator Start()
    {
        while (d.done != true)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5f);

        AstarPath.active.Scan();

    }




}
