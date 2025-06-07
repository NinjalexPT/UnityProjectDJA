using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExitController : MonoBehaviour
{
   private GameObject player;

   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
   {
      //Get the player GameObject
      player = GameObject.FindGameObjectWithTag("Player");


   }

   private void OnTriggerEnter(Collider other)
   {
      //Check if the player has entered the exit trigger
      if (other.gameObject == player)
      {
         FindFirstObjectByType<SceneLoader>().IntoEnd();
      }
   }

}
