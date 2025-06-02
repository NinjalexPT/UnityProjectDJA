using UnityEngine;

public class PowerUpManager : MonoBehaviour
{

   public static PowerUpManager Instance { get; private set; }
   private void Awake()
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
   }

   public PowerUpType CurrentPowerUp { get; private set; } = PowerUpType.None;

}

public enum PowerUpType
{
   SpeedBoost,
   DoubleCoins,
   SeeEnemy,
   Gun,
   EnemySpeedBoost,
   SpeedSlow,
   None
}