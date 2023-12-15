using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Random = UnityEngine.Random;
public class Robot : MonoBehaviour
{
    public GameObject fps_player_obj;
    public float robot_speed;
    public float rotationSpeed = 30.0f;
    public string type = "Sphere";
    public int life = 2;
    public float radius_of_search_for_player;
    private TMP_Text[] textComponents;
    private int currentValue;
    private List<Node> currPath;
    private float pathUpdateInterval = 2f; // Time in seconds to update the path
    private float lastPathUpdateTime;
    private readonly float nodeReachedThreshold = 1f;

    Vector3 gridOrigin;
    int gridWidth;
    int gridHeight;
    bool[,] grid;

    void Start()
    {
        textComponents = new TMP_Text[4];
        string expression = "";
        if (type == "Sphere")
        {
            int number = Random.Range(0, 10);
            expression = number.ToString();
            currentValue = number;
        }
        else if (type == "Rectangle")
        {
            string original = GenerateExpression();
            int idx = -1;
            for (int i = 0; i < original.Length; i++)
            {
                if (!char.IsDigit(original[i]))
                {
                    idx = i;
                    break;
                }
            }
            int A = int.Parse(original.Substring(0, idx));
            char op = original[idx];
            int B = int.Parse(original.Substring(idx + 1));
            expression = A + "\n" + op + "\n" + B;
            currentValue = EvaluateExpression(original);
        }
        else if (type == "Cube")
        {
            expression = GenerateExpression();
            currentValue = EvaluateExpression(expression);
        }
        else if (type == "Boss")
        {
            currentValue = 0;
            expression = "*";
        }
        for (int i = 0; i < textComponents.Length; i++)
        {
            textComponents[i] = transform.GetChild(i).GetComponent<TMP_Text>();
            textComponents[i].text = expression;
        }
        GameObject border1 = GameObject.Find("border 1");
        GameObject border2 = GameObject.Find("border 2");
        GameObject border3 = GameObject.Find("border 3");
        GameObject border4 = GameObject.Find("border 4");
        Vector2 gridSize;
        (gridSize, gridOrigin) = CalcMaze(border1, border2, border3, border4);
        gridWidth = (int)gridSize.x;
        gridHeight = (int)gridSize.y;

        // Initialize grid
        grid = new bool[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = true;
            }
        }
        // GameObject wall = GameObject.Find("maze wall 1");
        // CreateMaze(wall);
        GameObject[] mazeWalls = GameObject.FindGameObjectsWithTag("MazeWall");
        foreach (GameObject wall in mazeWalls)
        {
            CreateMaze(wall);
        }
        //grid[20, 0] = false;
        //grid[20, 1] = false;
        //grid[20, 2] = false;
        //grid[20, 3] = false;
        //grid[20, 4] = false;
        //grid[20, 5] = false;
        //grid[20, 6] = false;
        //grid[20, 7] = false;
        //PrintGrid();
    }

    void CreateMaze(GameObject wall)
    {
        if (wall != null)
        {
            // Calculate taken space for wall
            List<Vector2Int> takenSpaces = getTaken(wall, gridWidth, gridHeight, gridOrigin);
            // Mark space false for taken
            foreach (Vector2Int space in takenSpaces)
            {
                if (space.x >= 0 && space.x < gridWidth && space.y >= 0 && space.y < gridHeight)
                {
                    // Add invisible wall
                    grid[space.x, space.y] = false;
                }
            }
        }
        else
        {
            Debug.LogError("Wall object is null.");
        }
    }

    // For Debugging
    void PrintGrid()
    {
        string gridLayout = "";
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                gridLayout += grid[x, y] ? "O" : "X"; // O for free, X for taken
            }
            gridLayout += "\n";
        }
        Debug.Log(gridLayout);
    }


    List<Vector2Int> getTaken(GameObject wall, int gridWidth, int gridHeight, Vector3 origin)
    {
        List<Vector2Int> takenSpaces = new List<Vector2Int>();

        Vector3 wallPos = wall.transform.position - origin;

        float wallWidth = wall.transform.localScale.x;
        float wallHeight = wall.transform.localScale.z;

        // Find wall's orientation with its Y-axis rotation
        bool isVertical = Mathf.Approximately(wall.transform.eulerAngles.y, 90f);

        // Testing inflation for walls to prevent stuck collisions
        float inflateAmount = Mathf.Max(wallWidth, wallHeight) > 100f ? 0.5f : 0f;

        // Initialize Dimensions of Wall
        int startX, endX, startZ, endZ;

        // Check if wall is rotated 90 degrees
        if (isVertical)
        {
            startX = Mathf.FloorToInt(wallPos.x - (wallHeight / 2f + inflateAmount));
            endX = Mathf.CeilToInt(wallPos.x + (wallHeight / 2f + inflateAmount));
            startZ = Mathf.FloorToInt(wallPos.z - inflateAmount);
            endZ = Mathf.CeilToInt(wallPos.z + wallWidth + inflateAmount);
        }
        else
        {
            startX = Mathf.FloorToInt(wallPos.x - inflateAmount);
            endX = Mathf.CeilToInt(wallPos.x + wallWidth + inflateAmount);
            startZ = Mathf.FloorToInt(wallPos.z - (wallHeight / 2f + inflateAmount));
            endZ = Mathf.CeilToInt(wallPos.z + (wallHeight / 2f + inflateAmount));
        }

        // Assign taken space to takenSpaces 
        for (int x = startX; x < endX; x++)
        {
            for (int z = startZ; z < endZ; z++)
            {
                if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
                {
                    // Add wall dimension to takenSpaces
                    takenSpaces.Add(new Vector2Int(x, z));
                }
            }
        }
        return takenSpaces;
    }

    public class Node
    {
        public int X, Z;
        public float GScore, HScore;
        public Node Parent;
        public float FScore => GScore + HScore;
        public Node(int x, int z)
        {
            X = x;
            Z = z;
            GScore = float.MaxValue;
            HScore = 0f;
            Parent = null;
        }
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        // Check X and Z directions
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue; // Skip the node itself
                int nx = node.X + dx;
                int nz = node.Z + dz;
                // Makes sure boss doesn't go out the borders
                if (nx >= 0 && nx < gridWidth && nz >= 0 && nz < gridHeight && grid[nx, nz])
                {
                    neighbors.Add(new Node(nx, nz));
                }
            }
        }
        return neighbors;
    }

    public static string GenerateExpression()
    {
        int A, B;
        char[] operators = new char[] { '+', '-', 'x', '/' };
        char op = operators[Random.Range(0, operators.Length)];
        do
        {
            A = Random.Range(0, 10);
            B = Random.Range(0, 10);
            if (op == '/')
            {
                A = B * Random.Range(0, 10);
            }
        } while (!IsResultInRange(A, B, op));

        return $"{A}{op}{B}";
    }

    private static bool IsResultInRange(int A, int B, char op)
    {
        int result = 0;
        switch (op)
        {
            case '+':
                result = A + B;
                break;
            case '-':
                result = A - B;
                break;
            case 'x':
                result = A * B;
                break;
            case '/':
                if (B == 0)
                {
                    result = -1;
                }
                else
                {
                    result = A / B;
                }
                break;
        }
        return result >= 0 && result <= 9;
    }

    public static int EvaluateExpression(string expression)
    {
        int idx = -1;
        for (int i = 0; i < expression.Length; i++)
        {
            if (!char.IsDigit(expression[i]))
            {
                idx = i;
                break;
            }
        }
        int A = int.Parse(expression.Substring(0, idx));
        char op = expression[idx];
        int B = int.Parse(expression.Substring(idx + 1));
        switch (op)
        {
            case '+':
                return A + B;
            case '-':
                return A - B;
            case 'x':
                return A * B;
            case '/':
                return A / B;
        }
        return -1;
    }


    (Vector2, Vector3) CalcMaze(GameObject border1, GameObject border2, GameObject border3, GameObject border4)
    {
        // Find the min and max x and z positions from the borders
        float minX = Mathf.Min(border1.transform.position.x, border2.transform.position.x,
                               border3.transform.position.x, border4.transform.position.x);
        float maxX = Mathf.Max(border1.transform.position.x, border2.transform.position.x,
                               border3.transform.position.x, border4.transform.position.x);
        float minZ = Mathf.Min(border1.transform.position.z, border2.transform.position.z,
                               border3.transform.position.z, border4.transform.position.z);
        float maxZ = Mathf.Max(border1.transform.position.z, border2.transform.position.z,
                               border3.transform.position.z, border4.transform.position.z);

        float width = maxX - minX;
        float depth = maxZ - minZ;

        // Change float to int
        int gridSizeX = Mathf.CeilToInt(width);
        int gridSizeZ = Mathf.CeilToInt(depth);

        // Calculate the origin of the maze
        Vector3 origin = new Vector3(minX, 0, minZ);

        return (new Vector2(gridSizeX, gridSizeZ), origin);
    }

    public class PriorityQueue
    {
        private List<Node> nodes = new List<Node>();

        public void Enqueue(Node node)
        {
            nodes.Add(node);
            nodes.Sort((a, b) => a.FScore.CompareTo(b.FScore));
        }
        public bool Dequeue(out Node node)
        {
            if (nodes.Count == 0)
            {
                node = null;
                return false;
            }
            node = nodes[0];
            nodes.RemoveAt(0);
            return true;
        }
        public int Count => nodes.Count;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        if (fps_player_obj != null)
        {
            if (type != "Boss")
            {
                Vector3 playerDir = fps_player_obj.transform.position - transform.position;
                playerDir.y = 0;
                if (playerDir.magnitude < radius_of_search_for_player)
                {
                    Vector3 moveDir = playerDir.normalized * robot_speed * Time.deltaTime;
                    transform.position += moveDir;
                    transform.LookAt(new Vector3(fps_player_obj.transform.position.x, transform.position.y, fps_player_obj.transform.position.z));
                }
            }
            else
            {
                Vector3 playerDir = fps_player_obj.transform.position - transform.position;
                playerDir.y = 0;
                if (Time.time - lastPathUpdateTime > pathUpdateInterval)
                {
                    //Debug.Log("Update Path");
                    lastPathUpdateTime = Time.time;
                    UpdatePathToPlayer();
                }
                MoveAlongPath();
            }
        }
    }

    Vector3 ConvertGridToWorld(int gridX, int gridZ)
    {
        //Debug.Log("Converting Grid To World");
        float worldX = gridOrigin.x + gridX;
        float worldZ = gridOrigin.z + gridZ;
        return new Vector3(worldX, 3, worldZ);
    }

    Vector2Int ConvertToGrid(Vector3 worldPosition)
    {
        //Debug.Log("Converting Grid Coordinates");
        int gridX = Mathf.FloorToInt(worldPosition.x - gridOrigin.x);
        int gridZ = Mathf.FloorToInt(worldPosition.z - gridOrigin.z);
        return new Vector2Int(gridX, gridZ);
    }

    void UpdatePathToPlayer()
    {
        //Debug.Log("Updating Path To Player");
        Vector2Int robotGridPos = ConvertToGrid(transform.position);
        Vector2Int playerGridPos = ConvertToGrid(fps_player_obj.transform.position);

        Node startNode = new Node(robotGridPos.x, robotGridPos.y);
        Node goalNode = new Node(playerGridPos.x, playerGridPos.y);

        currPath = AStarSearch(startNode, goalNode);
        if (currPath != null && currPath.Count > 0)
        {
            for (int i = 0; i < currPath.Count - 1; i++)
            {
                Debug.DrawLine(ConvertGridToWorld(currPath[i].X, currPath[i].Z),
                               ConvertGridToWorld(currPath[i + 1].X, currPath[i + 1].Z),
                               Color.red, pathUpdateInterval);
            }
        }
        else
        {
            Debug.Log("No path found");
        }
    }

    void MoveAlongPath()
    {
        if (currPath != null && currPath.Count > 0)
        {
            Vector3 nextNodePos = ConvertGridToWorld(currPath[0].X, currPath[0].Z);
            Vector3 dirToNextNode = (nextNodePos - transform.position).normalized;
            float bossSize = 1f;

            // Check maxe wall collision
            if (Physics.BoxCast(transform.position, new Vector3(bossSize, bossSize, bossSize) / 2, dirToNextNode, out RaycastHit hit, Quaternion.identity, nodeReachedThreshold))
            {
                if (hit.collider.CompareTag("MazeWall"))
                {
                    Vector3 slideDir = Vector3.Cross(hit.normal, Vector3.up).normalized;
                    Vector3 combinedDir = (slideDir + dirToNextNode).normalized;
                    transform.position += combinedDir * robot_speed * Time.deltaTime;
                }
            }
            else
            {
                // Move to next node as no wall is detected
                transform.position += dirToNextNode * robot_speed * Time.deltaTime;
            }
            // Look to the next node
            transform.LookAt(nextNodePos);
            // Remove path if node is reached
            if (Vector3.Distance(transform.position, nextNodePos) < nodeReachedThreshold)
            {
                currPath.RemoveAt(0);
            }
        }
    }

    private List<Node> AStarSearch(Node start, Node goal)
    {
        var openSet = new PriorityQueue();
        var allNodes = new Dictionary<(int, int), Node>();

        start.GScore = 0;
        start.HScore = Heuristic(start, goal);
        openSet.Enqueue(start);
        allNodes.Add((start.X, start.Z), start);

        while (openSet.Count > 0)
        {
            if (!openSet.Dequeue(out Node current))
                break;
            if (current.X == goal.X && current.Z == goal.Z)
            {
                return ReconstructPath(current);
            }

            foreach (Node neighbor in GetNeighbors(current))
            {
                float tentativeGScore = current.GScore + Vector3.Distance(new Vector3(current.X, 0, current.Z), new Vector3(neighbor.X, 0, neighbor.Z)); // Distance between nodes
                if (tentativeGScore < neighbor.GScore)
                {
                    neighbor.Parent = current;
                    neighbor.GScore = tentativeGScore;
                    neighbor.HScore = Heuristic(neighbor, goal);

                    if (!allNodes.ContainsKey((neighbor.X, neighbor.Z)))
                    {
                        openSet.Enqueue(neighbor);
                        allNodes.Add((neighbor.X, neighbor.Z), neighbor);
                    }
                }
            }
        }
        return new List<Node>(); // No path was found
    }

    // use the parent self-join to reconstuct the path.
    private List<Node> ReconstructPath(Node node)
    {
        List<Node> path = new List<Node>();
        while (node != null)
        {
            path.Add(node);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }

    // Heuristic costs for A*
    private float Heuristic(Node a, Node b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Z - b.Z); // Manhattan distance
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.name.Contains("Number One") && currentValue == 1) ||
            (collision.gameObject.name.Contains("Number Two") && currentValue == 2) ||
            (collision.gameObject.name.Contains("Number Three") && currentValue == 3) ||
            (collision.gameObject.name.Contains("Number Four") && currentValue == 4) ||
            (collision.gameObject.name.Contains("Number Five") && currentValue == 5) ||
            (collision.gameObject.name.Contains("Number Six") && currentValue == 6) ||
            (collision.gameObject.name.Contains("Number Seven") && currentValue == 7) ||
            (collision.gameObject.name.Contains("Number Eight") && currentValue == 8) ||
            (collision.gameObject.name.Contains("Number Nine") && currentValue == 9) ||
            (collision.gameObject.name.Contains("Number Zero") && currentValue == 0))
        {
            if (type == "Cube")
            {
                string expression = "";
                if (life > 1)
                {
                    GetComponent<Renderer>().material = Resources.Load<Material>("Materials/Robot");
                    Destroy(GameObject.FindGameObjectWithTag("Shield"));
                    life = life - 1;
                    expression = GenerateExpression();
                    currentValue = EvaluateExpression(expression);
                    for (int i = 0; i < textComponents.Length; i++)
                    {
                        textComponents[i] = transform.GetChild(i).GetComponent<TMP_Text>();
                        textComponents[i].text = expression;
                    }
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
                //Debug.Log(currentValue + " Deleteing Current");
            }
        }
        else
        {
            if (collision.gameObject.name.Contains("FPSPlayer"))
            {
                Debug.Log("You Lost a Life");
            }
            Debug.Log("CurrentValue doesn't match " + currentValue);
        }
        //Debug.Log("Collision with " + collision.gameObject.name);
    }

}