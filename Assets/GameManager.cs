using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
  public static GameManager Instance;

  [SerializeField]
  private int coinCount;

  [SerializeField]
  private TextMeshProUGUI coinCountText;

  public void AddCoins(int amount)
  {
    coinCount += amount;
    coinCountText.text = "Coin count: " + coinCount;
  }

  /// <summary>
  /// Start is called on the frame when a script is enabled just before
  /// any of the Update methods is called the first time.
  /// </summary>
  void Start()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(this);
    }
  }
}
