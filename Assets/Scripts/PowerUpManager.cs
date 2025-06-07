using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
   public static PowerUpManager Instance { get; private set; }

   public List<PowerUp> allPowerUps;
   public List<PowerUp> activePowerUps;
   public Dictionary<PowerUpType, Coroutine> powerUpCoroutines;

   public GameObject gun;

   private void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
         DontDestroyOnLoad(gameObject);

         allPowerUps = new List<PowerUp>()
            {
                new PowerUp(PowerUpType.SpeedBoost,     10f),
                new PowerUp(PowerUpType.DoubleCoins,    15f),
                new PowerUp(PowerUpType.SeeEnemy,       20f),
                new PowerUp(PowerUpType.Gun,            -1f),
                new PowerUp(PowerUpType.EnemySpeedBoost,25f),
                new PowerUp(PowerUpType.SpeedSlow,       5f)
            };
         activePowerUps = new List<PowerUp>();
         powerUpCoroutines = new Dictionary<PowerUpType, Coroutine>();

         gun.SetActive(false);

      }
      else
      {
         Destroy(gameObject);
      }
   }

   private void Update()
   {
      foreach (var pw in activePowerUps.ToList())
      {
         switch (pw.Type)
         {
            case PowerUpType.SpeedBoost:
               FirstPersonController.Instance.speedModifier = 1.5f;
               break;
            case PowerUpType.SpeedSlow:
               FirstPersonController.Instance.speedModifier = 0.25f;
               break;
            case PowerUpType.Gun:
               if (gun != null && !gun.activeSelf)
                  gun.SetActive(true);
               break;
            case PowerUpType.EnemySpeedBoost:
               EnemyController.speedModifier = 1.5f;
               break;
            case PowerUpType.DoubleCoins:
               CoinManager.valueModifier = 2;
               break;
            case PowerUpType.SeeEnemy:
               EnemyController.seeEnemy = true;
               break;
         }
      }
   }

   public void ActivateRandomPowerUp()
   {
      int randomIndex = Random.Range(0, 4);
      ActivatePowerUp(allPowerUps[randomIndex].Type);
      SoundManager.Instance.sfxSource.PlayOneShot(
         SoundManager.Instance.winSound
      );
   }

   public void ActivateRandomDebuff()
   {
      int randomIndex = Random.Range(4, 6);
      ActivatePowerUp(allPowerUps[randomIndex].Type);
      SoundManager.Instance.sfxSource.PlayOneShot(
         SoundManager.Instance.loseSound
      );
   }

   public void ActivatePowerUp(PowerUpType type)
   {
      var pw = allPowerUps.FirstOrDefault(p => p.Type == type);
      if (pw == null)
      {
         Debug.LogWarning($"PowerUp {type} não existe!");
         return;
      }

      activePowerUps.RemoveAll(p => p.Type == type);
      activePowerUps.Add(pw);

      if (type == PowerUpType.Gun)
      {
         gun?.SetActive(true);
         UIManager.Instance.SetPowerUpText(type,
             $"{GunController.Instance.ammoSize}/" +
             $"{GunController.Instance.ammoSize}"
         );
         return;
      }

      if (pw.Duration > 0f)
      {
         if (powerUpCoroutines.TryGetValue(type, out var old))
         {
            StopCoroutine(old);
         }
         var co = StartCoroutine(PowerUpTimer(pw));
         powerUpCoroutines[type] = co;
      }
   }

   public void DeactivatePowerUp(PowerUpType type)
   {
      activePowerUps.RemoveAll(p => p.Type == type);

      if (powerUpCoroutines.TryGetValue(type, out var co))
      {
         StopCoroutine(co);
         powerUpCoroutines.Remove(type);
      }

      UIManager.Instance.SetPowerUpText(type, string.Empty);

      if (type == PowerUpType.Gun)
      {
         gun?.SetActive(false);
         GunController.Instance.ResetAmmo();
      }
      else if (type == PowerUpType.SpeedBoost ||
               type == PowerUpType.SpeedSlow)
      {
         FirstPersonController.Instance.speedModifier = 1f;
      }
      else if (type == PowerUpType.EnemySpeedBoost)
      {
         EnemyController.speedModifier = 1f;
      }
      else if (type == PowerUpType.SeeEnemy)
      {
         EnemyController.seeEnemy = false;
      }
      else if (type == PowerUpType.DoubleCoins)
      {
         CoinManager.valueModifier = 1;
      }
   }

   private IEnumerator PowerUpTimer(PowerUp pw)
   {
      float timeLeft = pw.Duration;

      while (timeLeft > 0f)
      {
         UIManager.Instance.SetPowerUpText(
             pw.Type,
             timeLeft.ToString("F1") + "s"
         );
         yield return new WaitForSeconds(0.1f);
         timeLeft -= 0.1f;
      }

      UIManager.Instance.SetPowerUpText(pw.Type, string.Empty);
      powerUpCoroutines.Remove(pw.Type);
      DeactivatePowerUp(pw.Type);
   }

   public bool IsPowerUpActive(PowerUpType type)
   {
      return activePowerUps.Any(p => p.Type == type);
   }

   public float GetPowerUpDuration(PowerUpType type)
   {
      PowerUp pw = activePowerUps.FirstOrDefault(p => p.Type == type);
      return pw != null ? pw.DurationLeft : 0f;
   }
}

public class PowerUp
{
   public PowerUpType Type { get; private set; }
   public float Duration { get; private set; }
   public float DurationLeft { get; set; }

   public PowerUp(PowerUpType type, float duration)
   {
      Type = type;
      Duration = duration;
      DurationLeft = duration;
   }
}

public enum PowerUpType
{
   SpeedBoost,
   DoubleCoins,
   SeeEnemy,
   Gun,
   EnemySpeedBoost,
   SpeedSlow,
}