using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public int width;
    public int height;
    public int depth;
    public float cellSize = 1f;
    private int placedPipes = 0;
    public int maxPipesPlaced = 100;
    private int amountPaths = 0;
    public int maxPaths = 5;
    public float growDuration = 1.0f;

    private Grid grid;

    public GameObject pipePrefab; // Assign your cylinder prefab here
    public GameObject spherePrefab; // Assign your sphere prefab here

    public List<Vector3Int> path = new List<Vector3Int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = new Grid(width, height, depth, cellSize, transform.position);
        CreatePath(new Vector3Int(5, 5, 5));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Creates a path by looking which positions are avalible next to the current position and choosing one randomly
    void CreatePath(Vector3Int pos)
    {
        //Adds the first position to the path.
        path.Add(pos);
        grid.SetCellOccupied(pos, true);

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
            if (IsWithinBounds(newPos) && !grid.occupiedCells[newPos.x, newPos.y, newPos.z] && grid.IsValidGridPosition(newPos))
            {
                availablePossibilities.Add(newPos);
            }
        }

        //Limits amount of placed Pipes
        if (placedPipes <= 50 && availablePossibilities.Count > 0)
        {
            placedPipes++;
            //calls function recursiv to add more Pipes
            CreatePath(availablePossibilities[Random.Range(0, availablePossibilities.Count)]);
        }
        else
        {
            //Adds 3D Pipes
            AddPath(path);
        }
    }

    //Function to see if Coordinate is out of bounds
    bool IsWithinBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < width &&
               pos.y >= 0 && pos.y < height &&
               pos.z >= 0 && pos.z < depth;
    }

    //Ads 4D Pipes to World
    public void AddPath(List<Vector3Int> pathA)
    {
        StartCoroutine(AddPathCoroutine(pathA));
    }

    //Loops through Pipe Positions and Positions Pipes and Spheres makes sure pipe is placed after pipe before finished scalling
    IEnumerator AddPathCoroutine(List<Vector3Int> pathA)
    {
        //chooses random color for pipe
        Color randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

        for (int i = 0; i < pathA.Count -1; i++)
        {
            yield return StartCoroutine(ScalePipeOverTime(pathA, pathA[i], pathA[i+1], randomColor));

            if (i < pathA.Count - 2 && IsCurve(pathA[i], pathA[i+1], pathA[i+2]))
            {
                CreateSphere(pathA[i + 1], randomColor);
            }
        }
    }

    //Scales and places Pipe between two Vector3Int coordinates.
    IEnumerator ScalePipeOverTime(List<Vector3Int> pathA, Vector3Int start, Vector3Int end, Color randomColor)
    {
        //Pipe instatiated at start-coordinate and rotated so it looks at end coordinate
        GameObject pipe = Instantiate(pipePrefab, start, Quaternion.identity);
        pipe.transform.LookAt(end);

        //change color
        Transform childTransform = pipe.transform.Find("pip");
        Renderer renderer = childTransform.GetComponent<Renderer>();
        renderer.material.color = randomColor;

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
        GameObject sph = Instantiate(spherePrefab, position, Quaternion.identity); // Instantiate sphere at the position
        Renderer renderer = sph.GetComponent<Renderer>();
        renderer.material.color = randomColor;
    }

    //Uses previous and next Coordinate to determain if current is a curve 
    bool IsCurve(Vector3Int prev, Vector3Int current, Vector3Int next)
    {
        Vector3Int direction1 = current - prev;
        Vector3Int direction2 = next - current;

        float angle = Vector3.Angle(direction1, direction2);
        return angle > 0f;
    }
}
