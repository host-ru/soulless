using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class Map : MonoBehaviour {

    // Size of the map (tiles)
    public int width = 12;
    public int height = 12;
    public float zOffset = -0.5f;
    public GameObject linePrefab;

    public TileType[] tileTypes;

    int[,] tiles;

    public class Node
    {
        public List<Node> neighbors;
        public int x;
        public int y;

        public Node()
        {
            neighbors = new List<Node>();
        }

        public float DistanceTo(Node n)
        {
            return Vector2.Distance(new Vector2(x, y), new Vector2(n.x, n.y));
        }
    }

    Node[,] graph;
    List<Node> currentPath = null;

	// Use this for initialization
	void Start () {
        GenerateMap();
        InstantiateMap();
        CameraController cameraController = Camera.main.GetComponent<CameraController>();
        cameraController.panLimit.x = width;
        cameraController.panLimit.y = height;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void GenerateMap()
    {
        tiles = new int[width, height];

        float[] distribution = new float[tileTypes.Length];

        // Set up probabilities of different terrains
        distribution[1] = 0.1f;
        distribution[2] = 0.02f;
        distribution[3] = 0f;

        distribution[0] = 1f;
        for (int i = 1; i < distribution.Length; i++)
        {
            distribution[0] -= distribution[i]; 
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float rnd = Random.Range(0f, 1f);

                if (rnd <= distribution[1])
                    tiles[i, j] = 1;
                else if (rnd <= distribution[1] + distribution[2])
                    tiles[i, j] = 2;
                else if (rnd <= distribution[1] + distribution[2] + distribution[3])
                    tiles[i, j] = 3;
                else
                    tiles[i, j] = 0;
            }
        }

    }

    void GeneratePathfindingGraph()
    {
        graph = new Node[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                graph[i, j] = new Node();
                graph[i, j].x = i;
                graph[i, j].y = j;
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (i > 0)
                    graph[i, j].neighbors.Add(graph[i - 1, j]);
                if (i < width - 1)
                    graph[i, j].neighbors.Add(graph[i + 1, j]);
                if (j > 0)
                    graph[i, j].neighbors.Add(graph[i, j - 1]);
                if (j < height - 1)
                    graph[i, j].neighbors.Add(graph[i, j + 1]);
            }
        }
    }

    void InstantiateMap()
    {
        for (int j = 0; j < height; j++)
        {
            // Setting up rows (don't really know why I need them)
            GameObject rowGameObject = new GameObject();
            rowGameObject.name = "Row_" + j;
            rowGameObject.transform.parent = this.transform;
            rowGameObject.transform.position = new Vector3(0, 0, j);

            for (int i = 0; i < width; i++)
            {
                // Instantiating tiles from prefab
                Vector3 position = new Vector3(i, zOffset, j);
                GameObject tileGameObject = (GameObject)Instantiate(tileTypes[tiles[i, j]].tilePrefab, position, Quaternion.identity);
                tileGameObject.name = "tile_" + i + "_" + j;
                tileGameObject.GetComponent<Tile>().x = i;
                tileGameObject.GetComponent<Tile>().y = j;
                tileGameObject.GetComponent<Tile>().tileType = tiles[i, j];
                tileGameObject.transform.parent = this.transform.Find("Row_" + j);
            }
        }
    }

    public float CostToEnterTile(Tile tile)
    {
        TileType tt = tileTypes[tile.tileType];
        if (tile.AssociatedUnit != null)
            return Mathf.Infinity;
        else
            return tt.movementCost;
    }

    public Tile GetTile(int x, int y)
    {
        return GameObject.Find("tile_" + x + "_" + y).GetComponent<Tile>();
    }

    public List<Tile> GeneratePath(Tile startTile, Tile destinationTile)
    {
        Dijkstra(startTile, destinationTile);
        List<Tile> path = new List<Tile>();
        if (currentPath == null)
            return null;
        for (int i = 0; i < currentPath.Count; i++)
        {
            path.Add(GetTile(currentPath[i].x, currentPath[i].y));
        }
        return path;
    }

    public List<Tile> GetAvailableTiles(Tile startTile, Tile destinationTile, float moves)
    {
        List<Tile> availableTiles = new List<Tile>();
        Dictionary<Node, float> distance = Dijkstra(startTile, destinationTile, moves);
        foreach(KeyValuePair<Node, float> n in distance)
        {
            if (n.Value <= moves)
            {
                availableTiles.Add(GetTile(n.Key.x, n.Key.y)); 
            }
        }
        
        return availableTiles;
    }

    Dictionary<Node, float> Dijkstra(Tile startTile, Tile destinationTile)
    {
        GeneratePathfindingGraph();
        currentPath = null;

        // Using Dijkstra's algorithm, calculate the path
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        List<Node> unvisited = new List<Node>();

        Node source = graph[startTile.x, startTile.y];
        Node target = graph[destinationTile.x, destinationTile.y];

        dist[source] = 0;
        prev[source] = null;

        // Initialize dist array with Inf
        foreach (Node v in graph)
        {
            if (v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }

            unvisited.Add(v);
        }

        while (unvisited.Count > 0)
        {
            Node u = null;

            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }
            if (u == target)
                break;

            unvisited.Remove(u);

            foreach (Node v in u.neighbors)
            {
                //float alt = dist[u] + u.DistanceTo(v);
                float alt = dist[u] + CostToEnterTile(GetTile(v.x, v.y));
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        if (prev[target] == null)
        {
            // No route to the target
            return dist;
        }

        currentPath = new List<Node>();

        Node curr = target;

        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        currentPath.Reverse();
        return dist;
    }

    Dictionary<Node, float> Dijkstra(Tile startTile, Tile destinationTile, float moves)
    {
        GeneratePathfindingGraph();
        currentPath = null;

        // Using Dijkstra's algorithm, calculate the path
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        List<Node> unvisited = new List<Node>();

        Node source = graph[startTile.x, startTile.y];
        Node target = graph[destinationTile.x, destinationTile.y];

        dist[source] = 0;
        prev[source] = null;

        // Initialize dist array with Inf
        foreach (Node v in graph)
        {
            if (v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }

            unvisited.Add(v);
        }

        while (unvisited.Count > 0)
        {
            Node u = null;

            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }
            if (dist[u] >= moves)
                break;

            unvisited.Remove(u);

            foreach (Node v in u.neighbors)
            {
                //float alt = dist[u] + u.DistanceTo(v);
                float alt = dist[u] + CostToEnterTile(GetTile(v.x, v.y));
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        if (prev[target] == null)
        {
            // No route to the target
            return dist;
        }

        currentPath = new List<Node>();

        Node curr = target;

        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        currentPath.Reverse();
        return dist;
    }

    public List<GameObject> ShowPath(List<Tile> desiredPath)
    {

        GameObject path = new GameObject();
        path.name = "Path";

        Vector3[] positions = new Vector3[desiredPath.Count];
        for (int i = 0; i < desiredPath.Count; i++)
        {
            positions[i] = new Vector3(desiredPath[i].transform.position.x, 0, desiredPath[i].transform.position.z);
        }
        List<GameObject> lines = new List<GameObject>();
        for (int i = 1; i < desiredPath.Count; i++)
        {
            GameObject pathLine = linePrefab;
            Vector3 desiredDirection = new Vector3(desiredPath[i].transform.position.x, 0, desiredPath[i].transform.position.z) - positions[i - 1];
            desiredDirection = desiredDirection.normalized;
            Quaternion q = Quaternion.Euler(90 * desiredDirection.z, 90 * desiredDirection.y, -90 * desiredDirection.x);
            lines.Add((GameObject)Instantiate(pathLine, positions[i - 1], q, path.transform));
        }
        return lines;
    }
}
