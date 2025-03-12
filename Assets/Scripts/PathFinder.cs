using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class PathFinder 
{
    public static bool IsReachable(int startRow, int startCol, int targetRow, int targetCol)
    {
        int rows = GameData.MazeRows; // Get the number of rows in the maze
        int cols = GameData.MazeColumns; // Get the number of columns in the maze
        bool[,] visited = new bool[rows, cols]; // Create a 2D array to track visited cells

        return DFS(startRow, startCol, targetRow, targetCol, visited); // Start DFS search from the initial position
    }

    private static bool DFS(int entRow, int entCol, int targetRow, int targetCol, bool[,] visited)
    {
        // Check if the current position is out of bounds or already visited
        if (entRow < 0 || entCol < 0 || entRow >= GameData.MazeRows || entCol >= GameData.MazeColumns || visited[entRow, entCol])
            return false;

        // If we reached the target cell, return true
        if (entRow == targetRow && entCol == targetCol)
            return true;

        visited[entRow, entCol] = true; // Mark the current cell as visited

        // Try moving in all four possible directions if no wall blocks the path

        // Move UP if there's no top wall
        if (GameData.Maze[entRow, entCol].WallTop == 0 && DFS(entRow + 1, entCol, targetRow, targetCol, visited))
            return true;

        // Move DOWN if there's no bottom wall
        if (GameData.Maze[entRow, entCol].WallBottom == 0 && DFS(entRow - 1, entCol, targetRow, targetCol, visited))
            return true;

        // Move LEFT if there's no left wall
        if (GameData.Maze[entRow, entCol].WallLeft == 0 && DFS(entRow, entCol - 1, targetRow, targetCol, visited))
            return true;

        // Move RIGHT if there's no right wall
        if (GameData.Maze[entRow, entCol].WallRight == 0 && DFS(entRow, entCol + 1, targetRow, targetCol, visited))
            return true;

        return false; // No valid path found
    }

    public static bool ResolveCell(int i, int j)
    {
        if (PathFinder.IsReachable(0, GameData.EntrancePoint, i, j))
            return true;

        // If the cell is not reachable, try to remove a wall to make it reachable
        if (i + 1 < GameData.MazeRows && GameData.Maze[i, j].WallTop == 2)
        {
            GameData.Maze[i, j].WallTop = 0;
            GameData.Maze[i + 1, j].WallBottom = 0;
            if (PathFinder.IsReachable(0, GameData.EntrancePoint, i, j))
                return true;
            GameData.Maze[i, j].WallTop = 2;
            GameData.Maze[i + 1, j].WallBottom = 2;
        }
        if (j + 1 < GameData.MazeColumns && GameData.Maze[i, j].WallRight == 2)
        {
            GameData.Maze[i, j].WallRight = 0;
            GameData.Maze[i, j + 1].WallLeft = 0;
            if (PathFinder.IsReachable(0, GameData.EntrancePoint, i, j))
                return true;
            GameData.Maze[i, j].WallRight = 2;
            GameData.Maze[i, j + 1].WallLeft = 2;
        }
        if (i > 0 && GameData.Maze[i, j].WallBottom == 2)
        {
            GameData.Maze[i, j].WallBottom = 0;
            GameData.Maze[i - 1, j].WallTop = 0;
            if (PathFinder.IsReachable(0, GameData.EntrancePoint, i, j))
                return true;
            GameData.Maze[i, j].WallBottom = 2;
            GameData.Maze[i - 1, j].WallTop = 2;
        }
        if (j > 0 && GameData.Maze[i, j].WallLeft == 2)
        {
            GameData.Maze[i, j].WallLeft = 0;
            GameData.Maze[i, j - 1].WallRight = 0;
            if (PathFinder.IsReachable(0, GameData.EntrancePoint, i, j))
                return true;
            GameData.Maze[i, j].WallLeft = 2;
            GameData.Maze[i, j - 1].WallRight = 2;
        }
        return false;
    }

    private static void ClearVisited()
    {
        for (int i = 0; i < GameData.MazeRows; i++)
        {
            for (int j = 0; j < GameData.MazeColumns; j++)
            {
                GameData.Maze[i, j].Visited = false;
            }
        }
    }

    public static List<Cell> AddCellsToBeResolved()
    {
        int maxCells = GameData.MazeRows * GameData.MazeColumns;
        ClearVisited();
        var cells = new List<Cell>() { GameData.Maze[0, GameData.EntrancePoint] };
        GameData.Maze[0, GameData.EntrancePoint].Visited = true;
        // Add the adjacent cells in a loop
        while(cells.Count < maxCells) 
        {
            for (int i = 0; i < GameData.MazeRows; i++)
            {
                for (int j = 0; j < GameData.MazeColumns; j++)
                {
                    if (GameData.Maze[i, j].Visited)
                        continue;
                    if (j + 1 < GameData.MazeColumns && GameData.Maze[i, j + 1].Visited)
                        cells.Add(GameData.Maze[i, j]);
                    else if (i > 0 && GameData.Maze[i - 1, j].Visited)
                        cells.Add(GameData.Maze[i, j]);
                    else if (j > 0 && GameData.Maze[i, j - 1].Visited)
                        cells.Add(GameData.Maze[i, j]);
                }
            }
            foreach (Cell cell in cells)
            {
                cell.Visited = true;
            }
        }
        ClearVisited();
        return cells;
    }

    public static void ResolveMaze()
    {
        List<Cell> cells = AddCellsToBeResolved();
        int cellsCount = cells.Count;
        while (cells.Count > 0)
        {
            Debug.Log($"Resolving {cells.Count} cells...");
            List<Cell> cellsToBeResolved = new List<Cell>();
            foreach (Cell cell in cells)
            {
                if (!ResolveCell(cell.Y, cell.X))
                    cellsToBeResolved.Add(cell);
            }
            cells = cellsToBeResolved;
            if (cells.Count == cellsCount)
            {
                Debug.Log("Maze cannot be resolved!");
                break;
            }
            cellsCount = cells.Count;
        }   
    }
}
