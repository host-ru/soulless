using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Map : MonoBehaviour {

    // Size of the map (tiles)
    public int width = 12;
    public int height = 12;
    public float zOffset = -0.5f;
    public GameObject meshPrefab;
    public GameObject linePrefab;

    public TileType[] tileTypes;

    Tile[,] tiles;

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
	void Awake () {
        GenerateMap();
        //CameraController cameraController = Camera.main.GetComponent<CameraController>();
        //cameraController.panLimit.x = width;
        //cameraController.panLimit.y = height;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void GenerateMap()
    {
        tiles = new Tile[width, height];

        float[] distribution = new float[tileTypes.Length];

        // Setting up coordinates
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j] = new Tile();
                tiles[i, j].x = i;
                tiles[i, j].y = j;
                tiles[i, j].tileType = 0;
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
        return tiles[x, y];
    }

    public Vector3 GetTileWorldSize()
    {
        return new Vector3(GetMeshWorldSize().x / width, 0, GetMeshWorldSize().z / height);  
    }

    public Vector3 GetTileWorldPosition(Tile tile)
    {
        Vector3 meshWorldSize = GetMeshWorldSize();
        Vector3 bottomLeftCorner = meshPrefab.transform.position - meshWorldSize / 2;
        float tileWidth = GetTileWorldSize().x;
        float tileHeight = GetTileWorldSize().z;
        Vector3 tileWorldPosition = bottomLeftCorner + new Vector3(tileWidth * tile.x, 0, tileHeight * tile.y) + new Vector3(tileWidth / 2, 0, tileHeight / 2);

        // Setting up Y coordinate for a tile using mesh
        //Mesh mesh = meshPrefab.GetComponentInChildren<Mesh>();
        float maxHeight = meshPrefab.GetComponent<MapPreview>().heightMapSettings.heightMultiplier + 1;

        RaycastHit raycastHit;
        //int layer_mask = LayerMask.GetMask("Map"); // We're only hitting tiles here
        Ray ray = new Ray(new Vector3(tileWorldPosition.x, maxHeight, tileWorldPosition.z), Vector3.down);

        if (meshPrefab.GetComponentInChildren<MeshCollider>().Raycast(ray, out raycastHit, maxHeight + 1))
        {
            tileWorldPosition = raycastHit.point;
        }

        //float tileY = meshPrefab.GetComponentInChildren<Mesh>().vertices[1].y;


        return tileWorldPosition;
    }

    public Vector3 GetMeshBorder()
    {
        return new Vector3(5, 0, 5) / 2;
    }

    public Vector3 GetMeshWorldSize()
    {
        Bounds meshBounds = meshPrefab.transform.GetComponentInChildren<MeshRenderer>().bounds;
        return new Vector3(meshBounds.size.x, 0, meshBounds.size.z) - GetMeshBorder() * 2;
    }

    public Tile GetTileByCoord(Vector3 coord)
    {
        Vector2 position = new Vector2(Mathf.Floor((coord.x * width) / GetMeshWorldSize().x), Mathf.Floor((coord.z * height) / GetMeshWorldSize().z));
        return GetTile((int)position.x, (int)position.y);
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
            positions[i] = new Vector3(GetTileWorldPosition(desiredPath[i]).x, 0, GetTileWorldPosition(desiredPath[i]).z);
        }
        List<GameObject> lines = new List<GameObject>();
        for (int i = 1; i < desiredPath.Count; i++)
        {
            GameObject pathLine = linePrefab;
            Vector3 desiredDirection = new Vector3(GetTileWorldPosition(desiredPath[i]).x, 0, GetTileWorldPosition(desiredPath[i]).z) - positions[i - 1];
            desiredDirection = desiredDirection.normalized;
            Quaternion q = Quaternion.Euler(90 * desiredDirection.z, 90 * desiredDirection.y, -90 * desiredDirection.x);
            GameObject newPath = (GameObject)Instantiate(pathLine, positions[i - 1], q, path.transform);
            newPath.transform.localScale = GetTileWorldSize() + Vector3.up * GetTileWorldSize().z;
            lines.Add(newPath);
        }
        return lines;
    }
}
