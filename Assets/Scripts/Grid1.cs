using UnityEngine;

public class Grid1
{
    private int width, height, depth;
    private float cellSize;
    private Vector3 origin;
    private bool[,,] occupiedCells;

    public Grid1(int width, int height, int depth, float cellSize, Vector3 origin)
    {
        this.width = width;
        this.height = height;
        this.depth = depth;
        this.cellSize = cellSize;
        this.origin = origin;
        occupiedCells = new bool[width, height, depth];
    }

    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - origin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.y - origin.y) / cellSize);
        int z = Mathf.FloorToInt((worldPosition.z - origin.z) / cellSize);
        return new Vector3Int(x, y, z);
    }

    public Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        float x = gridPosition.x * cellSize + origin.x;
        float y = gridPosition.y * cellSize + origin.y;
        float z = gridPosition.z * cellSize + origin.z;
        return new Vector3(x, y, z);
    }

    public void SetCellOccupied(Vector3Int gridPosition, bool isOccupied)
    {
        if (IsValidGridPosition(gridPosition))
        {
            occupiedCells[gridPosition.x, gridPosition.y, gridPosition.z] = isOccupied;
        }
    }

    public bool IsCellOccupied(Vector3Int gridPosition)
    {
        if (IsValidGridPosition(gridPosition))
        {
            return occupiedCells[gridPosition.x, gridPosition.y, gridPosition.z];
        }
        return false;
    }

    private bool IsValidGridPosition(Vector3Int gridPosition)
    {
        return gridPosition.x >= origin.x && gridPosition.x < width &&
               gridPosition.y >= origin.y && gridPosition.y < height &&
               gridPosition.z >= origin.y && gridPosition.z < depth;
    }
}
