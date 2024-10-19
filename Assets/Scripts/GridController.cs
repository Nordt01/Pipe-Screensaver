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
}
