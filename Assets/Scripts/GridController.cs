using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public int width;
    public int height;
    public int depth;
    public int maxPipes;
    public int pipeAmount;
    public float growDuration;
    public float emissionIntensity;

    private Grid grid;

    public GameObject pipePrefab;
    public GameObject spherePrefab;
    public GameObject parent;
    Vector3Int vnone = new Vector3Int(-1, -1, -1);

    public List<List<Vector3Int>> pathPath = new List<List<Vector3Int>>(); // List of path, which saves every path created during runtime
    List<Vector3Int> freeCells = new List<Vector3Int>();

    public static readonly Vector3Int None = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxPipes = height * width * depth;
        grid = new Grid(width, height, depth,  transform.position);

        //Creates a list of freeCells
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                for (int d = 0; d < depth; d++)
                {
                    //checks if valid that new path is created at this position
                    if (!grid.IsCellOccupied(new Vector3Int(w, h, d)))
                    {
                        freeCells.Add(new Vector3Int(w, h, d));
                    }
                }
            }
        }

        createPipePath(); //Starts path creating and drawing process
    }

    void createPipePath()
    {
        //Checks if all cells are occupied and then resets
        
        if (pipeAmount >= maxPipes) {return;}

        //chooses random color for path
        Color randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        Vector3Int pos = CreatPathStart();

        while (pos != vnone && grid.IsValidGridPosition(pos)) {
            //Adds the position to the latest path in pathPath.
            pipeAmount++;
            pathPath[pathPath.Count - 1].Add(pos);
            grid.SetCellOccupied(pos, true);

            pos = CreateCell(pos, randomColor); //creates new Position
        }

        StartCoroutine(AddPathCoroutine(pathPath[pathPath.Count - 1], randomColor)); //Starts Process do draw pipes of path
    }

    //Creates random start point by shuffeling occuppiedcells list and choosing one.
    Vector3Int CreatPathStart()
    {
        //creates next path
        if(freeCells.Count > 0)
        {
            List<Vector3Int> path2 = new List<Vector3Int>();
            pathPath.Add(path2);

            int r = Random.Range(0, freeCells.Count);
            Vector3Int next = freeCells[r];

            //createPipePath(new Vector3Int(w, h, d)); //Creates new path at choosen start point
            pathPath[pathPath.Count - 1].Add(next);
            freeCells.RemoveAt(r);
            return next;
        }
        return vnone;
    }

    //Creates a path by looking which positions are avalible next to the current position and choosing one randomly
    Vector3Int CreateCell(Vector3Int pos, Color randomColor)
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
            //CreateSphere(pos, randomColor);
            return vnone;
        }
        //Chooses new random next position from the valid possibilities
        int next = Random.Range(0, availablePossibilities.Count);
        freeCells.Remove(availablePossibilities[next]);
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
    IEnumerator AddPathCoroutine(List<Vector3Int> pathA, Color randomColor)
    {
        int i = 0;
        CreateSphere(pathA[i], randomColor); //start point sphere
        //Loops through Pipe Positions and Positions Pipes and Spheres (when curve is detected)
        for (i = 0; i < pathA.Count -1; i++)
        {
            yield return StartCoroutine(ScalePipeOverTime(pathA, pathA[i], pathA[i+1], randomColor));
            //TODO add that it creates sphere when locations around already full
            if (i < pathA.Count - 2 && IsCurve(pathA[i], pathA[i+1], pathA[i+2]))
            {
                CreateSphere(pathA[i + 1], randomColor);
            }
        }
        CreateSphere(pathA[i], randomColor); //endpointsphere
        createPipePath();
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
}
