using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileMap : MonoBehaviour {

    public GamePlayManager playManager;
    public GameObject tilesParent;

	public Unit selectedUnit;

	public TileType[] tileTypes;

	int[,] tiles;
	Node[,] graph;

    public List<ClickableTile> clickableTiles = new List<ClickableTile>();

	int mapSizeX = 10;
	int mapSizeY = 10;

    public static TileMap instance;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        GamePlayManager.OnGameplayEvent += OnGameplayEventRecieve;
    }

    private void OnDisable()
    {
        GamePlayManager.OnGameplayEvent -= OnGameplayEventRecieve;
    }

    private void OnGameplayEventRecieve(string message, GamePlayManager.GameplayEvent type)
    {
        if(type == GamePlayManager.GameplayEvent.IntroQuest)
        {
            GenerateMapData();
            GeneratePathfindingGraph();
            GenerateMapVisual();
        }

        if(type == GamePlayManager.GameplayEvent.MyPlayerPreMove)
        {
            
        }

        if(type == GamePlayManager.GameplayEvent.MyPlayerPreAttack)
        {
            
        }

        if(type == GamePlayManager.GameplayEvent.TurnEnd)
        {
            //DisableSpellRnage();
        }
    }

    void GenerateMapData()
    {

        FG_LevelAct actData = PF_GamePlay.ActiveQuest.Acts.First().Value;

        mapSizeY = actData.MapData.ToList().Count;
        mapSizeX = actData.MapData.First().Value.Count;
        tiles = new int[mapSizeX, mapSizeY];

        for (int i = 0; i < actData.MapData.ToList().Count; i++)
        {
            for (int j = 0; j < actData.MapData.ToList()[i].Value.Count; j++)
            {
                //Debug.Log(actData.MapData.ToList()[i].Value.ToList()[j]);
                tiles[j, i] = actData.MapData.ToList()[i].Value.ToList()[j];
            }
        }
    }

	public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY) {

		TileType tt = tileTypes[ tiles[targetX,targetY] ];

		if(UnitCanEnterTile(targetX, targetY) == false )
			return Mathf.Infinity;

        if(FindTile(targetX, targetY).isCharacter)
            return 999;

        float cost = tt.movementCost;

		if( sourceX!=targetX && sourceY!=targetY) {
			// We are moving diagonally!  Fudge the cost for tie-breaking
			// Purely a cosmetic thing!
			cost += 0.5f;
		}

		return cost;

	}

	void GeneratePathfindingGraph() {
		// Initialize the array
		graph = new Node[mapSizeX,mapSizeY];

		// Initialize a Node for each spot in the array
		for(int x=0; x < mapSizeX; x++) {
			for(int y=0; y < mapSizeY; y++) {
				graph[x,y] = new Node();
				graph[x,y].x = x;
				graph[x,y].y = y;
			}
		}

		// Now that all the nodes exist, calculate their neighbours
		for(int x=0; x < mapSizeX; x++) {
			for(int y=0; y < mapSizeY; y++) {

				// This is the 4-way connection version:
/*				if(x > 0)
					graph[x,y].neighbours.Add( graph[x-1, y] );
				if(x < mapSizeX-1)
					graph[x,y].neighbours.Add( graph[x+1, y] );
				if(y > 0)
					graph[x,y].neighbours.Add( graph[x, y-1] );
				if(y < mapSizeY-1)
					graph[x,y].neighbours.Add( graph[x, y+1] );
*/

				// This is the 8-way connection version (allows diagonal movement)
				// Try left
				if(x > 0) {
					graph[x,y].neighbours.Add( graph[x-1, y] );
					if(y > 0)
						graph[x,y].neighbours.Add( graph[x-1, y-1] );
					if(y < mapSizeY-1)
						graph[x,y].neighbours.Add( graph[x-1, y+1] );
				}

				// Try Right
				if(x < mapSizeX-1) {
					graph[x,y].neighbours.Add( graph[x+1, y] );
					if(y > 0)
						graph[x,y].neighbours.Add( graph[x+1, y-1] );
					if(y < mapSizeY-1)
						graph[x,y].neighbours.Add( graph[x+1, y+1] );
				}

				// Try straight up and down
				if(y > 0)
					graph[x,y].neighbours.Add( graph[x, y-1] );
				if(y < mapSizeY-1)
					graph[x,y].neighbours.Add( graph[x, y+1] );

				// This also works with 6-way hexes and n-way variable areas (like EU4)
			}
		}
	}

	void GenerateMapVisual() {
		for(int x=0; x < mapSizeX; x++) {
			for(int y=0; y < mapSizeY; y++) {
				TileType tt = tileTypes[ tiles[x,y] ];
                GameObject go = (GameObject)Instantiate(tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity, tilesParent.transform);
                go.name = x + "|" + y;
                go.GetComponent<SpriteRenderer>().sortingLayerName = "Ground";
                go.GetComponent<SpriteRenderer>().sortingOrder = -y;

				ClickableTile ct = go.GetComponent<ClickableTile>();
                clickableTiles.Add(ct);
                ct.tileX = x;
				ct.tileY = y;
				ct.map = this;

                //GameObject go2 = (GameObject)Instantiate(reachedTile, new Vector3(x, y, 0), Quaternion.identity, reachedParent.transform);
                //go2.SetActive(false);
                //reachedTileList.Add(go2);
            }
		}
	}

	public Vector3 TileCoordToWorldCoord(int x, int y) {
		return new Vector3(x, y, 0);
	}

    public ClickableTile FindTile(int tileX,int TileY)
    {
        return clickableTiles.Find(tx => tx.tileX == tileX && tx.tileY == TileY);
    }

    public void OnUnitMoving(int sourceX,int sourceY, int targetX,int targetY)
    {
        FindTile(sourceX, sourceY).isCharacter = false;
        FindTile(targetX, targetY).isCharacter = true;
    }

    public void ShowTilesCanBereached()
    {

        //Init();
        Unit uni = selectedUnit;
        
        for (int i = uni.tileY - 1; i < uni.tileY + 2; i++)
        {
            for (int j = uni.tileX - 1; j < uni.tileX + 2; j++)
            {
                ClickableTile ct = FindTile(j, i);

                if (ct == null)
                    continue;
                if (ct.mask == null)
                    continue;

                //Debug.LogError(uni.remainingMovement + "___" + CostToEnterTile(uni.tileX, uni.tileY, j, i));
                bool canEnter = uni.remainingMovement - CostToEnterTile(uni.tileX, uni.tileY, j, i) >= 0 ? true : false;
                if (canEnter)
                {
                    ct.mask.SetActive(true);

                    if (i == uni.tileY && j == uni.tileX)
                        ct.mask.SetActive(false);
                }
            }
        }
    }
    public void DisableTileMask()
    {
        foreach (var go in clickableTiles)
        {
            if(go.mask != null)
                go.mask.SetActive(false);

            if (go.atkMask != null)
                go.atkMask.SetActive(false);
        }
    }
    public void ShowSpellRange()
    {
        Unit unit = selectedUnit;
        FG_Spell spell = selectedUnit.selectedSpell;
        for (int i = 0; i < spell.RangeX.Count; i++)
        {
            for(int j = 0; j < spell.RangeY.Count; j++)
            {
                ClickableTile ct;
                ct = FindTile(unit.tileX + spell.RangeX[i], unit.tileY + spell.RangeY[j]);

                if (ct == null || ct.atkMask == null)
                    continue;
                ct.atkMask.SetActive(true);
            }
        }
    }
    public void DisableSpellRnage()
    {
        foreach (var tile in clickableTiles)
        {
            if(tile.atkMask != null)
                tile.atkMask.SetActive(false);
        }
            
    }

    public bool UnitCanEnterTile(int x, int y) {

        // We could test the unit's walk/hover/fly type against various
        // terrain flags here to see if they are allowed to enter the tile.

		return tileTypes[ tiles[x,y] ].isWalkable;
	}

	public void GeneratePathTo(int x, int y) {
		// Clear out our unit's old path.
		selectedUnit.currentPath = null;

		if( UnitCanEnterTile(x,y) == false ) {
			// We probably clicked on a mountain or something, so just quit out.
			return;
		}

		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// Setup the "Q" -- the list of nodes we haven't checked yet.
		List<Node> unvisited = new List<Node>();
		
		Node source = graph[
		                    selectedUnit.tileX, 
		                    selectedUnit.tileY
		                    ];
		
		Node target = graph[
		                    x, 
		                    y
		                    ];
		
		dist[source] = 0;
		prev[source] = null;

		// Initialize everything to have INFINITY distance, since
		// we don't know any better right now. Also, it's possible
		// that some nodes CAN'T be reached from the source,
		// which would make INFINITY a reasonable value
		foreach(Node v in graph) {
			if(v != source) {
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}

			unvisited.Add(v);
		}

		while(unvisited.Count > 0) {
			// "u" is going to be the unvisited node with the smallest distance.
			Node u = null;

			foreach(Node possibleU in unvisited) {
				if(u == null || dist[possibleU] < dist[u]) {
					u = possibleU;
				}
			}

			if(u == target) {
				break;	// Exit the while loop!
			}

			unvisited.Remove(u);

			foreach(Node v in u.neighbours) {
				//float alt = dist[u] + u.DistanceTo(v);
				float alt = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
				if( alt < dist[v] ) {
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}

        // If we get there, the either we found the shortest route
        // to our target, or there is no route at ALL to our target.

        //如果COST為無限，則TARGET會為NULL!!
        if (prev[target] == null) {
            // No route between our target and the source
            return;
		}

		List<Node> currentPath = new List<Node>();

		Node curr = target;

		// Step through the "prev" chain and add it to our path
		while(curr != null) {
			currentPath.Add(curr);
			curr = prev[curr];
		}

		// Right now, currentPath describes a route from out target to our source
		// So we need to invert it!

		currentPath.Reverse();

		selectedUnit.GetComponent<Unit>().currentPath = currentPath;
	}

}
