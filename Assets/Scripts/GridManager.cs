using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public int depth = 10;
    public float cellSize = 1f;

    private Grid1 grid;

    void Start()
    {
        grid = new Grid1(width, height, depth, cellSize, transform.position);
    }

    private void InitializeInstance()
    {
        if(!PlaceObject(new Vector3(Random.Range(0, width), Random.Range(0, height), Random.Range(0, height)))){
            InitializeInstance();
        }
    }

    // Methode zum Platzieren von Objekten
    public bool PlaceObject(Vector3 worldPosition)
    {
        Vector3Int gridPosition = grid.WorldToGridPosition(worldPosition);
        if (!grid.IsCellOccupied(gridPosition))
        {
            grid.SetCellOccupied(gridPosition, true);
            // Hier Objekt instantiieren
            return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        if (grid == null) return;

        Gizmos.color = Color.white;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 pos = grid.GridToWorldPosition(new Vector3Int(x, y, z));
                    Gizmos.DrawWireCube(pos + Vector3.up * height * cellSize / 2, new Vector3(cellSize, height * cellSize, cellSize));
                }
            }
        }
    }
}
