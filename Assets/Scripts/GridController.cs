using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public int width;
    public int height;
    public int depth;
    private int pathAmount = 0;
    public int maxPaths;
    public float growDuration;
    public float emissionIntensity;

    private Grid grid;

    public GameObject pipePrefab;
    public GameObject spherePrefab;
    public GameObject parent;
    int x, y, z;

    public List<List<Vector3Int>> pathPath = new List<List<Vector3Int>>(); // List of path, which saves every path created during runtime

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        x = Random.Range(0, width - 1);
        y = Random.Range(0, height - 1);
        z = Random.Range(0, depth - 1);

        grid = new Grid(width, height, depth,  transform.position);

        List<Vector3Int> path = new List<Vector3Int>();
        pathPath.Add(path);

        createPipePath(new Vector3Int(x, y, z)); //Starts path creating and drawing process
    }

    void createPipePath(Vector3Int start)
    {
        Vector3Int pos = start;

        while (pos != new Vector3Int() && grid.IsValidGridPosition(pos)) {
            //Adds the position to the latest path in pathPath.
            pathPath[pathPath.Count - 1].Add(pos);
            grid.SetCellOccupied(pos, true);

            pos = CreatePath(pos); //creates new Position
        }

        StartCoroutine(AddPathCoroutine(pathPath[pathPath.Count - 1])); //Starts Process do draw pipes of path
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
            Vector3Int newPos = pos + dir;

            //Creates a list of all valid new path positions
            if (IsWithinBounds(newPos) && !grid.occupiedCells[newPos.x, newPos.y, newPos.z])
            {
                availablePossibilities.Add(newPos);
            }
        }
        //Stops if current position is sorounded by pipes
        if (availablePossibilities.Count == 0) {
            return new Vector3Int();
        }
        //Chooses new random next position from the valid possibilities
        int next = Random.Range(0, availablePossibilities.Count);
        return availablePossibilities[next];
    }

    //Function to see if Coordinate is out of bounds (Different from Grid.isValidGridPosition)
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

        //Loops through Pipe Positions and Positions Pipes and Spheres (when curve is detected)
        for (i = 0; i < pathA.Count -1; i++)
        {
            yield return StartCoroutine(ScalePipeOverTime(pathA, pathA[i], pathA[i+1], randomColor));

            if (i < pathA.Count - 2 && IsCurve(pathA[i], pathA[i+1], pathA[i+2]))
            {
                CreateSphere(pathA[i + 1], randomColor);
            }
        }

        i = 0;
        for (i = 0; i < 3; i++) 
        { 
            //chooses random x,y,z coordinates
            x = Random.Range(0, width - 1);
            y = Random.Range(0, height - 1);
            z = Random.Range(0, depth - 1);

            //checks if valid that new path is created at this position
            if (!grid.IsCellOccupied(new Vector3Int(x, y, z)) && pathAmount < maxPaths-1)
            {
                pathAmount++;
                List<Vector3Int> path2 = new List<Vector3Int>();
                pathPath.Add(path2);

                createPipePath(new Vector3Int(x, y, z)); //Creates new path at choosen start point
                break;
            }
            //Checks if already to many pathes exists and then clears worldspace and beginns creating pipes from the start again.
            if (pathAmount >= maxPaths - 1) //Neu start
            {
                //Reseting varaibles
                pathAmount = 0;
                grid.occupiedCells = new bool[width, height, depth];

                //Destroying all children
                KillChildren();

                //Starting from the beginning with new random spawnpoint
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

        //change color and makes pipes slightly glow
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

        //Gives color and glow to sphere
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

    //Destroys all children objects of an object (needed for clearing pipes)
    void KillChildren()
    {
        while (parent.transform.childCount > 0)
        {
            DestroyImmediate(parent.transform.GetChild(0).gameObject);
        }
    }
}
