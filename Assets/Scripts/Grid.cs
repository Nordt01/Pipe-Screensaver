using UnityEngine;

public class Grid
{
    private int width, height, depth;
    private float cellSize;
    private Vector3 origin;
    public bool[,,] occupiedCells;

    public Grid(int width, int height, int depth, Vector3 origin)
    {
        this.width = width;
        this.height = height;
        this.depth = depth;
        this.origin = origin;
        occupiedCells = new bool[width, height, depth];
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

    public bool IsValidGridPosition(Vector3Int gridPosition)
    {
        return gridPosition.x >= origin.x && gridPosition.x < width-1 &&
               gridPosition.y >= origin.y && gridPosition.y < height-1 &&
               gridPosition.z >= origin.y && gridPosition.z < depth-1;
    }
}
