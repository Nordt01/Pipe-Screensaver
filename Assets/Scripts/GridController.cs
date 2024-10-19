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

    private Grid grid;

    public GameObject pipePrefab; // Assign your cylinder prefab here
    public GameObject spherePrefab; // Assign your sphere prefab here

    public List<Vector3Int> path = new List<Vector3Int>();
    public List<Vector3Int> testpath = new List<Vector3Int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = new Grid(width, height, depth, cellSize, transform.position);
        //CreatePath(new Vector3Int(5, 5, 5));
        CratePipeInstance(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 9));
        testpath.Add(new Vector3Int(0, 0, 0));
        testpath.Add(new Vector3Int(0, 0, 4));
        testpath.Add(new Vector3Int(0, 0, 6));
        testpath.Add(new Vector3Int(0, 8, 6));
        testpath.Add(new Vector3Int(-5, 8, 6));
        //AddPath(testpath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreatePath(Vector3Int pos)
    {
        path.Add(pos);
        grid.SetCellOccupied(pos, true);

        List<Vector3Int> availablePossibilities = new List<Vector3Int>();
        Vector3Int[] possibilities = new Vector3Int[]
        {
            pos += new Vector3Int(0, 1, 0),
            pos += new Vector3Int(0, -1, 0),
            pos += new Vector3Int(1, 0, 0),
            pos += new Vector3Int(-1, 0, 0),
            pos += new Vector3Int(0, 0, 1),
            pos += new Vector3Int(0, 0, -1)
        };

        foreach (Vector3Int posi in possibilities)
        {
            if (!grid.occupiedCells[posi.x, posi.y, posi.z] && grid.IsValidGridPosition(new Vector3Int(posi.x, posi.y, posi.z)))
            {
                availablePossibilities.Add(posi);
            }
        }

        if (availablePossibilities.Count == 0)
        {
            Debug.Log("QQQQQQQQQQQQQQQQQQQQQQQ______________KEINENAUSWEGMEHR___________________QQQQQQQQQQQQQQQQQQQQQQQQQ");
        }
        else
        {
            foreach (var item in path)
            {
                Debug.Log(item); // Use Debug.Log to print to the Unity console
            }
            Debug.Log("---------------------ListenEnde----------------------");
            placedPipes++;
            CreatePath(availablePossibilities[Random.Range(0, availablePossibilities.Count)]);
        }


    }

    void AddPath(List<Vector3Int> pathA)
    {
        Vector3Int old = pathA[0];
        for (int i = 0, j = 1, k = 2; k < pathA.Count; i++, j++, k++)
        {
            if (IsCurve(pathA[i], pathA[j], pathA[k]))
            {
                CratePipeInstance(old, pathA[j]);
                CreateSphere(pathA[j]);
                old = pathA[j];
            }
        }
    }

    void CratePipeInstance(Vector3Int start, Vector3Int end)
    {
        GameObject pipe = Instantiate(pipePrefab);

        // Position the pipe at the midpoint
        Vector3 midPoint = (start + end) / 2;
        pipe.transform.position = midPoint;

        pipe.transform.LookAt(end);

        // Calculate the distance and scale the pipe
        float distance = Vector3.Distance(start, end);
        //TODO change that it works
        pipe.transform.localScale = new Vector3(pipe.transform.localScale.x, distance / 2, pipe.transform.localScale.y); // Assuming the cylinder's pivot is at its base

        // Rotate the pipe to face the direction from start to end
      
    }
    void CreateSphere(Vector3 position)
    {
        Instantiate(spherePrefab, position, Quaternion.identity); // Instantiate sphere at the position
    }

    bool IsCurve(Vector3Int prev, Vector3Int current, Vector3Int next)
    {
        Vector3Int direction1 = current - prev;
        Vector3Int direction2 = next - current;

        float angle = Vector3.Angle(direction1, direction2);
        return angle > 0f;
    }
}
