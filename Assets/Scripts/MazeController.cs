using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
    void Start()
    {
        GenerateMaze(); 
        Create3DWalls();
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
                if (GameData.Maze[i, j].WallTop > 0)
                {
                    GameObject prefab = (GameData.Maze[i, j].WallTop == 1) ? outerPrefab : wallPrefab;
                    GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize + GameData.HalfCellSize, 1.5f, i * GameData.CellSize + GameData.CellSize), Quaternion.identity);
                    wall.transform.Rotate(0, 0, 0);
                    if (GameData.Maze[i, j].WallTop == 1)
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
                    else
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 3f, 0.2f);
                }
                if (GameData.Maze[i, j].WallRight > 0)
                {
                    GameObject prefab = (GameData.Maze[i, j].WallRight == 1) ? outerPrefab : wallPrefab;
                    GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize + GameData.CellSize, 1.5f, i * GameData.CellSize + GameData.HalfCellSize), Quaternion.identity);
                    wall.transform.Rotate(0, 90, 0);
                    if (GameData.Maze[i, j].WallRight == 1)
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
                    else
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 3f, 0.2f);
                }
                if (GameData.Maze[i, j].WallBottom > 0)
                {
                    GameObject prefab = (GameData.Maze[i, j].WallBottom == 1) ? outerPrefab : wallPrefab;
                    GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize + GameData.HalfCellSize, 1.5f, i * GameData.CellSize), Quaternion.identity);
                    wall.transform.Rotate(0, 0, 0);
                    if (GameData.Maze[i, j].WallBottom == 1)
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
                    else
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 3f, 0.2f);
                }
                if (GameData.Maze[i, j].WallLeft > 0)
                {
                    GameObject prefab = (GameData.Maze[i, j].WallLeft == 1) ? outerPrefab : wallPrefab;
                    GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize, 1.5f, i * GameData.CellSize + GameData.HalfCellSize), Quaternion.identity);
                    wall.transform.Rotate(0, 90, 0);
                    if (GameData.Maze[i, j].WallLeft == 1)
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
                    else
                        wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 3f, 0.2f);
                }
            }
        }

    }

}
