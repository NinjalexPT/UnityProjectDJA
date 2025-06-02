using UnityEngine;

public class RouletteTable : MonoBehaviour
{
   [Header("Main parts of the machine")]
   [SerializeField]
   private GameObject rouletteWheel;

   [SerializeField] private float spinDuration = 3f;
   [SerializeField] private float spinSpeed = 720f;

}
