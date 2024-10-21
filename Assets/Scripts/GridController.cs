using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public int width;
    public int height;
    public int depth;
    public float cellSize = 1f;
    public int placedPipes = 0;
    public int maxPipesPlaced = 100000;
    private int pathAmount = 0;
    public int maxPaths = 2;
    public float growDuration = 1.0f;
    public float emissionIntensity = 1.0f;

    private Grid grid;

    public GameObject pipePrefab; // Assign your cylinder prefab here
    public GameObject spherePrefab; // Assign your sphere prefab here
    public GameObject parent;

    public List<List<Vector3Int>> pathPath = new List<List<Vector3Int>>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<Vector3Int> path = new List<Vector3Int>();
        pathPath.Add(path);
        grid = new Grid(width, height, depth, cellSize, transform.position);
        createPipePath(new Vector3Int(5, 5, 5));
    }

    void createPipePath(Vector3Int start)
    {
        Vector3Int pos = start;
        while (placedPipes <= maxPipesPlaced && pos != new Vector3Int() && grid.IsValidGridPosition(pos)) {
            //Adds the position to the path.
            pathPath[pathPath.Count - 1].Add(pos);
            grid.SetCellOccupied(pos, true);
            placedPipes++;

            pos = CreatePath(pos);
        }
        StartCoroutine(AddPathCoroutine(pathPath[pathPath.Count - 1]));
    }
    //Creates a path by looking which positions are avalible next to the current position and choosing one randomly
    Vector3Int CreatePath(Vector3Int pos)
    {
        //List which includes all next possible positions
        List<Vector3Int> availablePossibilities = new List<Vector3Int>();

        //Coordinates needed for computing next Position
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1)
        };

        //Adds all avaliable Coordinates next tu current to list
        foreach (Vector3Int dir in directions)
        {
            //Coordinate next to current Position
            Vector3Int newPos = pos + dir;
            if (IsWithinBounds(newPos) && !grid.occupiedCells[newPos.x, newPos.y, newPos.z])
            {
                availablePossibilities.Add(newPos);
            }
        }
        //Limits amount of placed Pipes
        if (availablePossibilities.Count == 0) {
            return new Vector3Int();
        }
        int next = Random.Range(0, availablePossibilities.Count);
        return availablePossibilities[next];
    }

    //Function to see if Coordinate is out of bounds
    bool IsWithinBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < width &&
               pos.y >= 0 && pos.y < height &&
               pos.z >= 0 && pos.z < depth;
    }

    //Loops through Pipe Positions and Positions Pipes and Spheres makes sure pipe is placed after pipe before finished scalling
    IEnumerator AddPathCoroutine(List<Vector3Int> pathA)
    {
        int i;
        //chooses random color for pipe
        Color randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

        for (i = 0; i < pathA.Count -1; i++)
        {
            yield return StartCoroutine(ScalePipeOverTime(pathA, pathA[i], pathA[i+1], randomColor));

            if (i < pathA.Count - 2 && IsCurve(pathA[i], pathA[i+1], pathA[i+2]))
            {
                CreateSphere(pathA[i + 1], randomColor);
            }
        }
        int x, y, z;
        i = 0;
        for (i = 0; i < 3; i++) 
        { 
            x = Random.Range(0, width - 1);
            y = Random.Range(0, height - 1);
            z = Random.Range(0, depth - 1);

            if (!grid.IsCellOccupied(new Vector3Int(x, y, z)) && pathAmount < maxPaths-1)
            {
                pathAmount++;
                List<Vector3Int> path2 = new List<Vector3Int>();
                pathPath.Add(path2);
                createPipePath(new Vector3Int(x, y, z));
                break;
            }
            else if (pathAmount >= maxPaths - 1) //Neu start
            {
                pathAmount = 0;
                KillChildren();
                grid.occupiedCells = new bool[width, height, depth];
                List<Vector3Int> path3 = new List<Vector3Int>();
                pathPath.Add(path3);
                createPipePath(new Vector3Int(x, y, z));
                break;
            }
        }
    }

    //Scales and places Pipe between two Vector3Int coordinates.
    IEnumerator ScalePipeOverTime(List<Vector3Int> pathA, Vector3Int start, Vector3Int end, Color randomColor)
    {
        //Pipe instatiated at start-coordinate and rotated so it looks at end coordinate
        GameObject pipe = Instantiate(pipePrefab, start, Quaternion.identity, parent.transform);
        pipe.transform.LookAt(end);

        //change color
        Transform childTransform = pipe.transform.Find("pip");
        Renderer renderer = childTransform.GetComponent<Renderer>();
        renderer.material.color = randomColor;
        Material material = renderer.material;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", randomColor * emissionIntensity);
        material.color = randomColor;

        float elapsedTime = 0f;
        Vector3 initialScale = pipe.transform.localScale;

        Vector3 direction = end - start;
        float distance = direction.magnitude/2;

        //Scales pipe over time
        while (elapsedTime < growDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / growDuration); 
            float currentLength = Mathf.Lerp(0, distance, t); // for smother scaling

            pipe.transform.localScale = new Vector3(initialScale.x, initialScale.y, currentLength);
            yield return null;
        }
        // Ensure final scale is exact
        pipe.transform.localScale = new Vector3(initialScale.x, initialScale.y, distance);
    }

    //Creates a sphere
    void CreateSphere(Vector3 position, Color randomColor)
    {
        GameObject sph = Instantiate(spherePrefab, position, Quaternion.identity, parent.transform); // Instantiate sphere at the position
        Renderer renderer = sph.GetComponent<Renderer>();
        Material material = renderer.material;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", randomColor * emissionIntensity);
        material.color = randomColor;
    }

    //Uses previous and next Coordinate to determain if current is a curve 
    bool IsCurve(Vector3Int prev, Vector3Int current, Vector3Int next)
    {
        Vector3Int direction1 = current - prev;
        Vector3Int direction2 = next - current;

        float angle = Vector3.Angle(direction1, direction2);
        return angle > 0f;
    }

    void KillChildren()
    {
        while (parent.transform.childCount > 0)
        {
            DestroyImmediate(parent.transform.GetChild(0).gameObject);
        }
    }
}
