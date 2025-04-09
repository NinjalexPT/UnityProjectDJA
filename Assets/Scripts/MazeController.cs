using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
    void Start()
    {
        GenerateMaze();
        RemoveRandomWalls(100);
        CreateOpenSpace();
        CreateWallsOfOtherTypes(30, 3);
        Create3DWalls();
        GenerateCoinsOnRandomCells();
    }

    private void GenerateMaze()
    {
        Debug.Log("Generating maze...");
        #region Generate maze cells and Walls: 
        for (int i = 0; i < GameData.MazeRows; i++)
        {
            for (int j = 0; j < GameData.MazeColumns; j++)
            {
                GameData.Maze[i, j] = new Cell(j, i);
                // Generate maze walls using a random number between 0 and 14:
                int random = UnityEngine.Random.Range(0, 15);
                // according to the bits of the random number, choose which walls to include
                GameData.Maze[i, j].WallTop = ((random & 1) == 1) ? 2 : 0;
                GameData.Maze[i, j].WallRight = ((random & 2) == 2) ? 2 : 0;
                GameData.Maze[i, j].WallBottom = ((random & 4) == 4) ? 2 : 0;
                GameData.Maze[i, j].WallLeft = ((random & 8) == 8) ? 2 : 0;
                // Rectify Left/Right Walls and Top/Bottom Walls
                if (j > 0)
                {
                    GameData.Maze[i, j].WallLeft = Mathf.Max(GameData.Maze[i, j].WallLeft, GameData.Maze[i, j - 1].WallRight);
                    GameData.Maze[i, j - 1].WallRight = Mathf.Max(GameData.Maze[i, j].WallLeft, GameData.Maze[i, j - 1].WallRight);
                }
                if (i > 0)
                {
                    GameData.Maze[i, j].WallBottom = Mathf.Max(GameData.Maze[i, j].WallBottom, GameData.Maze[i - 1, j].WallTop);
                    GameData.Maze[i - 1, j].WallTop = Mathf.Max(GameData.Maze[i, j].WallBottom, GameData.Maze[i - 1, j].WallTop);
                }
            }
        }
        #endregion

        #region Generate maze Exterior walls: 
        for (int i = 0; i < GameData.MazeRows; i++)
        {
            GameData.Maze[i, 0].WallLeft = 1;
            GameData.Maze[i, GameData.MazeColumns - 1].WallRight = 1;
        }

        for (int i = 0; i < GameData.MazeColumns; i++)
        {
            GameData.Maze[0, i].WallBottom = 1;
            GameData.Maze[GameData.MazeRows - 1, i].WallTop = 1;
        }
        #endregion

        #region Generate maze entrance and exit using a random number between 0 and GameData.MazeColumns:
        GameData.EntrancePoint = UnityEngine.Random.Range(0, GameData.MazeColumns);
        GameData.Maze[0, GameData.EntrancePoint].WallBottom = 0;
        int exit = UnityEngine.Random.Range(0, GameData.MazeColumns);
        GameData.Maze[GameData.MazeRows - 1, exit].WallTop = 0;
        #endregion

        Debug.Log("Maze generated successfully!");
        PathFinder.ResolveMaze(); // Resolve the maze to ensure there is a path from the entrance to the exit
    }

    private void RemoveRandomWalls(int wallsToRemove)
    {
        int removed = 0;
        while (removed < wallsToRemove)
        {
            int i = UnityEngine.Random.Range(0, GameData.MazeRows);
            int j = UnityEngine.Random.Range(0, GameData.MazeColumns);
            List<string> possibleWalls = new List<string>();

            if (GameData.Maze[i, j].WallTop > 1) possibleWalls.Add("top");
            if (GameData.Maze[i, j].WallRight > 1) possibleWalls.Add("right");
            if (GameData.Maze[i, j].WallBottom > 1) possibleWalls.Add("bottom");
            if (GameData.Maze[i, j].WallLeft > 1) possibleWalls.Add("left");

            if (possibleWalls.Count > 0)
            {
                string selectedWall = possibleWalls[UnityEngine.Random.Range(0, possibleWalls.Count)];

                switch (selectedWall)
                {
                    case "top":
                        GameData.Maze[i, j].WallTop = 0; 
                        if (i > 0) GameData.Maze[i - 1, j].WallTop = 0;
                        break;
                    case "right":
                        GameData.Maze[i, j].WallRight = 0;
                        if (j + 1 < GameData.MazeColumns) GameData.Maze[i, j + 1].WallLeft = 0;
                        break;
                    case "bottom":
                        GameData.Maze[i, j].WallBottom = 0;
                        if (i + 1 < GameData.MazeRows) GameData.Maze[i + 1, j].WallBottom = 0;
                        break;
                    case "left":
                        GameData.Maze[i, j].WallLeft = 0;
                        if (j > 0) GameData.Maze[i, j - 1].WallRight = 0;
                        break;
                }
                removed++;
            }
        }
    }


    private void CreateWallsOfOtherTypes(int wallsToChange, int wallType) 
    {
        int changed = 0;
        while (changed < wallsToChange)
        {
            int i = UnityEngine.Random.Range(0, GameData.MazeRows);
            int j = UnityEngine.Random.Range(0, GameData.MazeColumns);
            List<string> possibleWalls = new List<string>();

            if (GameData.Maze[i, j].WallTop > 1) possibleWalls.Add("top");
            if (GameData.Maze[i, j].WallRight > 1) possibleWalls.Add("right");
            if (GameData.Maze[i, j].WallBottom > 1) possibleWalls.Add("bottom");
            if (GameData.Maze[i, j].WallLeft > 1) possibleWalls.Add("left");

            if (possibleWalls.Count > 0)
            {
                string selectedWall = possibleWalls[UnityEngine.Random.Range(0, possibleWalls.Count)];

                switch (selectedWall)
                {
                    case "top":
                        GameData.Maze[i, j].WallTop = wallType;
                        break;
                    case "right":
                        GameData.Maze[i, j].WallRight = wallType;
                        break;
                    case "bottom":
                        GameData.Maze[i, j].WallBottom = wallType;
                        break;
                    case "left":
                        GameData.Maze[i, j].WallLeft = wallType;
                        break;
                }
                changed++;
            }
        }
    }

    private void Create3DWalls()
    {
        // Get Prefabs for the walls
        GameObject wallPrefab = Resources.Load<GameObject>("Prefabs/InnerWall");
        GameObject outerPrefab = Resources.Load<GameObject>("Prefabs/OuterWalls");
        // Create 3D walls based on the maze data
        for (int i = 0; i < GameData.MazeRows; i++)
        {
            for (int j = 0; j < GameData.MazeColumns; j++)
            {
                if (GameData.Maze[i, j].WallTop > 0 && (i == 0 || i  == GameData.MazeRows - 1))
                {
                    GameObject prefab = (GameData.Maze[i, j].WallTop == 1) ? outerPrefab : wallPrefab;
                    GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize + GameData.HalfCellSize, 1.5f, i * GameData.CellSize + GameData.CellSize), Quaternion.identity);
                    wall.transform.Rotate(0, 0, 0);
                    if (GameData.Maze[i, j].WallTop == 1)
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
                }
                if (GameData.Maze[i, j].WallRight > 0)
                {
                    GameObject prefab = (GameData.Maze[i, j].WallRight == 1) ? outerPrefab : wallPrefab;
                    GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize + GameData.CellSize, 1.5f, i * GameData.CellSize + GameData.HalfCellSize), Quaternion.identity);
                    wall.transform.Rotate(0, 90, 0);
                    if (GameData.Maze[i, j].WallRight == 1)
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
                }
                if (GameData.Maze[i, j].WallBottom > 0)
                {
                    GameObject prefab = (GameData.Maze[i, j].WallBottom == 1) ? outerPrefab : wallPrefab;
                    GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize + GameData.HalfCellSize, 1.5f, i * GameData.CellSize), Quaternion.identity);
                    wall.transform.Rotate(0, 0, 0);
                    if (GameData.Maze[i, j].WallBottom == 1)
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
                }
                if (GameData.Maze[i, j].WallLeft > 0 && j == 0)
                {
                    GameObject prefab = (GameData.Maze[i, j].WallLeft == 1) ? outerPrefab : wallPrefab;
                    GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize, 1.5f, i * GameData.CellSize + GameData.HalfCellSize), Quaternion.identity);
                    wall.transform.Rotate(0, 90, 0);
                    if (GameData.Maze[i, j].WallLeft == 1)
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
                }
            }
        }

    }

    private void CreateOpenSpace()
    {
        //Create an open space in the maze randomly with 3x3 size
        int i = UnityEngine.Random.Range(1, GameData.MazeRows - 3);
        int j = UnityEngine.Random.Range(1, GameData.MazeColumns - 3);
        for (int row = i; row < i + 3; row++)
        {
            for (int col = j; col < j + 3; col++)
            {
                GameData.Maze[row, col].WallTop = 0;
                GameData.Maze[row - 1, col].WallBottom = 0;
                GameData.Maze[row, col].WallRight = 0;
                GameData.Maze[row, col + 1].WallLeft = 0;
                GameData.Maze[row, col].WallBottom = 0;
                GameData.Maze[row + 1, col].WallTop = 0;
                GameData.Maze[row, col].WallLeft = 0;
                GameData.Maze[row, col - 1].WallRight = 0;
            }
        }
    }

    private void GenerateCoinsOnRandomCells() 
    {
        //create the coins from a prefab
        GameObject coinPrefab = Resources.Load<GameObject>("Prefabs/PirateCoin");

        //define the number of coins spread in the maze as a percentage of the maze size
        int numberOfCoins = (int)(GameData.MazeRows * GameData.MazeColumns * 0.1f);
        for (int k = 0; k < numberOfCoins; k++) 
        {
            int randomRow = UnityEngine.Random.Range(0, GameData.MazeRows);
            int randomCol = UnityEngine.Random.Range(0, GameData.MazeColumns);

            GameObject coin = Instantiate(coinPrefab, new Vector3(randomCol * GameData.CellSize + GameData.HalfCellSize, 1f, randomRow * GameData.CellSize + GameData.HalfCellSize), Quaternion.identity);
            coin.transform.Rotate(90, 0, 0);
        }
    }

}
