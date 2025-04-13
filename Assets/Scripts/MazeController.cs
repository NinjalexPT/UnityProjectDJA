using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.AI.Navigation;
using UnityEngine;

public class MazeController : MonoBehaviour
{
  [SerializeField]
  private List<GameObject> slotMachinePrefabsInScene = new List<GameObject>();

  [SerializeField]
  private float amountPerWidth = 0.33f;

  [SerializeField]
  private float MIN_DISTANCE_BETWEEN_SLOTS = 20f; // Minimo de distância entre slots

  void Start()
  {
    GenerateMaze();
    RemoveRandomWalls(100);
    CreateOpenSpace(); // Agora este método gera múltiplas áreas abertas com slots
    CreateWallsOfOtherTypes(30, 3);
    Create3DWalls();
    GenerateCoinsOnRandomCells();

    slotMachinePrefabsInScene = FindObjectsByType<SlotMachineController>(FindObjectsSortMode.None).Select(slot => slot.gameObject).ToList();
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

    // #region Generate maze entrance and exit using a random number between 0 and GameData.MazeColumns:
    // GameData.EntrancePoint = UnityEngine.Random.Range(0, GameData.MazeColumns);
    // GameData.Maze[0, GameData.EntrancePoint].WallBottom = 0;
    // int exit = UnityEngine.Random.Range(0, GameData.MazeColumns);
    // GameData.Maze[GameData.MazeRows - 1, exit].WallTop = 0;
    // #endregion

    Debug.Log("Maze generated successfully!");
    PathFinder.ResolveMaze(); // Resolve the maze to ensure there is a path from the entrance to the exit

    // Find the NavMeshSurface component in the scene
    NavMeshSurface navMeshSurface = FindObjectOfType<NavMeshSurface>();

    if (navMeshSurface != null)
    {
      // Build the NavMesh at runtime
      navMeshSurface.BuildNavMesh();
      Debug.Log("NavMesh baked after maze generation.");
    }
    else
    {
      Debug.LogError("NavMeshSurface not found in the scene. Make sure you've added it to a GameObject.");
    }

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

    // Create a parent object for the walls
    GameObject wallsParent = new GameObject("Walls");
    wallsParent.transform.parent = transform;
    wallsParent.transform.localPosition = Vector3.zero;

    // Create 3D walls based on the maze data
    for (int i = 0; i < GameData.MazeRows; i++)
    {
      for (int j = 0; j < GameData.MazeColumns; j++)
      {
        if (GameData.Maze[i, j].WallTop > 0 && (i == 0 || i == GameData.MazeRows - 1))
        {
          GameObject prefab = (GameData.Maze[i, j].WallTop == 1) ? outerPrefab : wallPrefab;
          GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize + GameData.HalfCellSize, 1.3f, i * GameData.CellSize + GameData.CellSize), Quaternion.identity, wallsParent.transform);
          wall.transform.Rotate(0, 0, 0);
          if (GameData.Maze[i, j].WallTop == 1)
            wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
        }
        if (GameData.Maze[i, j].WallRight > 0)
        {
          GameObject prefab = (GameData.Maze[i, j].WallRight == 1) ? outerPrefab : wallPrefab;
          GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize + GameData.CellSize, 1.3f, i * GameData.CellSize + GameData.HalfCellSize), Quaternion.identity, wallsParent.transform);
          wall.transform.Rotate(0, 90, 0);
          if (GameData.Maze[i, j].WallRight == 1)
            wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
        }
        if (GameData.Maze[i, j].WallBottom > 0)
        {
          GameObject prefab = (GameData.Maze[i, j].WallBottom == 1) ? outerPrefab : wallPrefab;
          GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize + GameData.HalfCellSize, 1.3f, i * GameData.CellSize), Quaternion.identity, wallsParent.transform);
          wall.transform.Rotate(0, 0, 0);
          if (GameData.Maze[i, j].WallBottom == 1)
            wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
        }
        if (GameData.Maze[i, j].WallLeft > 0 && j == 0)
        {
          GameObject prefab = (GameData.Maze[i, j].WallLeft == 1) ? outerPrefab : wallPrefab;
          GameObject wall = Instantiate(prefab, new Vector3(j * GameData.CellSize, 1.3f, i * GameData.CellSize + GameData.HalfCellSize), Quaternion.identity, wallsParent.transform);
          wall.transform.Rotate(0, 90, 0);
          if (GameData.Maze[i, j].WallLeft == 1)
            wall.transform.localScale = new Vector3(GameData.CellSize + 0.2f, 10f, 0.2f);
        }
      }
    }

  }

  private void CreateOpenSpace()
  {
    int numberOfOpenSpaces = Mathf.FloorToInt((float)GameData.MazeColumns * amountPerWidth);
    Debug.Log($"Generating {numberOfOpenSpaces} open spaces.");

    // Get existing slot machines
    List<GameObject> existingSlotMachines = GameObject.FindObjectsByType<SlotMachineController>(FindObjectsSortMode.None).Select(sm => sm.gameObject).ToList();

    for (int k = 0; k < numberOfOpenSpaces; k++)
    {
      // Tenta gerar um espaço aberto e colocar uma slot até encontrar uma posição válida
      bool spaceAndSlotCreated = false;
      int attempts = 0;
      while (!spaceAndSlotCreated && attempts < 20) // Aumentei as tentativas para dar mais chances com a verificação de distância
      {
        int startRow = UnityEngine.Random.Range(1, GameData.MazeRows - 3);
        int startCol = UnityEngine.Random.Range(1, GameData.MazeColumns - 3);

        bool overlaps = false;
        // Aqui você poderia adicionar lógica para verificar se este novo espaço se sobrepõe a espaços já criados.

        if (!overlaps)
        {
          // Create the open space
          for (int row = startRow; row < startRow + 3; row++)
          {
            for (int col = startCol; col < startCol + 3; col++)
            {
              GameData.Maze[row, col].WallTop = 0;
              if (row > 0) GameData.Maze[row - 1, col].WallBottom = 0;
              GameData.Maze[row, col].WallRight = 0;
              if (col < GameData.MazeColumns - 1) GameData.Maze[row, col + 1].WallLeft = 0;
              GameData.Maze[row, col].WallBottom = 0;
              if (row < GameData.MazeRows - 1) GameData.Maze[row + 1, col].WallTop = 0;
              GameData.Maze[row, col].WallLeft = 0;
              if (col > 0) GameData.Maze[row, col - 1].WallRight = 0;
            }
          }

          if (slotMachinePrefabsInScene.Count == 0)
          {
            Debug.LogWarning("No Slot Machine prefabs found in the scene, cannot instantiate one in the open space.");
            return;
          }

          // Create a parent object for the slot machines if it doesn't exist
          GameObject slotMachinesParent = GameObject.Find("SlotMachines");
          if (slotMachinesParent == null)
          {
            slotMachinesParent = new GameObject("SlotMachines");
            slotMachinesParent.transform.parent = transform;
            slotMachinesParent.transform.localPosition = Vector3.zero;
          }

          // Calculate the position to instantiate the slot machine (center of the 3x3 area)
          float centerX = (startCol + 1) * GameData.CellSize + GameData.HalfCellSize;
          float centerZ = (startRow + 1) * GameData.CellSize + GameData.HalfCellSize;
          Vector3 potentialSlotPosition = new Vector3(centerX, 1.3f, centerZ);

          // Check if the new slot position is too close to existing slots
          bool tooClose = false;
          foreach (var existingSlot in existingSlotMachines)
          {
            if (Vector3.Distance(potentialSlotPosition, existingSlot.transform.position) < MIN_DISTANCE_BETWEEN_SLOTS)
            {
              tooClose = true;
              break;
            }
          }

          if (!tooClose)
          {
            // Logic to choose and instantiate a slot machine prefab
            List<GameObject> availablePrefabs = new List<GameObject>(slotMachinePrefabsInScene);
            GameObject selectedPrefab = null;
            bool allPrefabsExist = true;

            if (availablePrefabs.Count > 0)
            {
              for (int i = 0; i < availablePrefabs.Count; i++)
              {
                bool found = false;
                foreach (var existingMachine in existingSlotMachines)
                {
                  if (existingMachine.name.StartsWith(availablePrefabs[i].name)) // Compare by name to identify prefab instances
                  {
                    found = true;
                    break;
                  }
                }
                if (!found)
                {
                  allPrefabsExist = false;
                  break;
                }
              }

              // Choose a random prefab
              int randomIndex = Random.Range(0, availablePrefabs.Count);
              selectedPrefab = availablePrefabs[randomIndex];

              // If not all prefabs exist, try to find one that doesn't exist yet
              if (!allPrefabsExist)
              {
                int attemptsInner = 0;
                while (attemptsInner < availablePrefabs.Count * 2) // Try a few times to find a non-existing one
                {
                  randomIndex = Random.Range(0, availablePrefabs.Count);
                  GameObject potentialPrefab = availablePrefabs[randomIndex];
                  bool exists = false;
                  foreach (var existingMachine in existingSlotMachines)
                  {
                    if (existingMachine.name.StartsWith(potentialPrefab.name))
                    {
                      exists = true;
                      break;
                    }
                  }
                  if (!exists)
                  {
                    selectedPrefab = potentialPrefab;
                    break;
                  }
                  attemptsInner++;
                }
                // If after attempts, we still haven't found a non-existing one, we'll use the randomly selected one anyway
              }

              if (selectedPrefab != null)
              {
                GameObject slotMachine = Instantiate(selectedPrefab, potentialSlotPosition, Quaternion.identity, slotMachinesParent.transform);
                slotMachine.transform.Rotate(90, 0, 0);
                Debug.Log($"Created open space {k + 1} and instantiated a Slot Machine: {slotMachine.name} at {potentialSlotPosition}");
                spaceAndSlotCreated = true;
              }
            }
          }
          else
          {
            Debug.Log($"Skipping slot creation for open space {k + 1} as it's too close to existing slots at {potentialSlotPosition}");
          }
        }
        attempts++;
      }
      if (!spaceAndSlotCreated)
      {
        Debug.LogWarning($"Could not find a valid position to create open space {k + 1} and a slot machine after multiple attempts.");
      }
    }
  }

  private void GenerateCoinsOnRandomCells()
  {
    GameObject RedCoinPrefab = GameObject.Find("RedCoin");
    GameObject BlueCoinPrefab = GameObject.Find("BlueCoin");

    GameObject coinPrefab = null;

    // Create a parent object for the coins
    GameObject coinsParent = new GameObject("Coins");
    coinsParent.transform.parent = transform;
    coinsParent.transform.localPosition = Vector3.zero;

    //define the number of coins spread in the maze as a percentage of the maze size
    int numberOfCoins = (int)(GameData.MazeRows * GameData.MazeColumns * 0.1f);
    for (int k = 0; k < numberOfCoins; k++)
    {
      int randomRow = Random.Range(0, GameData.MazeRows);
      int randomCol = Random.Range(0, GameData.MazeColumns);

      int random = Random.Range(0, 2);

      coinPrefab = random == 1 ? RedCoinPrefab : BlueCoinPrefab;

      GameObject coin = Instantiate(coinPrefab, new Vector3(randomCol * GameData.CellSize + GameData.HalfCellSize, 1f, randomRow * GameData.CellSize + GameData.HalfCellSize), Quaternion.identity, coinsParent.transform);
      coin.transform.Rotate(0, 90, 0);
    }
  }

}