using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
   public static GameManager Instance;

   [Header("Game Settings")]

   [SerializeField] private int coinCount;
   private int startingCoinCount;
   public bool gameOver;

   public bool skipIntro = false;
   public float coinReappearingTimer = 30f;
   public int chanceForKey = 5;

   [Space]
   [Header("Cached Managers / Controllers")]
   public GunController gunController;
   public FirstPersonController firstPersonController;
   public EnemyController enemyController;
   public PowerUpManager powerUpManager;
   public UIManager uIManager;
   public FinishController finishController;

   public void FetchCachedControllers()
   {
      print("fetching cached controllers");

      gunController = FindFirstObjectByType<GunController>();
      firstPersonController = FindFirstObjectByType<FirstPersonController>();
      enemyController = FindFirstObjectByType<EnemyController>();
      powerUpManager = FindFirstObjectByType<PowerUpManager>();
      uIManager = FindFirstObjectByType<UIManager>();
      finishController = FindFirstObjectByType<FinishController>();

   }

   void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
         DontDestroyOnLoad(gameObject);
      }
      else
      {
         Destroy(gameObject);
      }

      startingCoinCount = coinCount;
   }

   void Start()
   {
      if (GameManager.Instance.uIManager != null)
         GameManager.Instance.uIManager.UpdateCoinCount(coinCount);
   }

   public void AddCoins(int amount)
   {
      coinCount += amount;
      if (GameManager.Instance.uIManager != null)
         GameManager.Instance.uIManager.UpdateCoinCount(coinCount);
   }

   public int CoinCount()
   {
      return coinCount;
   }

   public void PlayerDied()
   {

      Destroy(GameManager.Instance.enemyController);

      gameOver = true;
      var playerController = FindFirstObjectByType<FirstPersonController>();
      coinCount = startingCoinCount;

      if (GameManager.Instance.uIManager != null)
      {
         GameManager.Instance.uIManager.ShowDeathScreen();
         GameManager.Instance.uIManager.HideInteractText();
         GameManager.Instance.uIManager.SetCoinObjectActive(false);
         GameManager.Instance.uIManager.SetPlayerUIActive(false);
      }

      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;

      Destroy(playerController);
      SoundManager.Instance.StopMovementSound();
   }

   public void RestartGame()
   {
      SceneManager.LoadScene(SceneManager
        .GetActiveScene().buildIndex);
   }
}