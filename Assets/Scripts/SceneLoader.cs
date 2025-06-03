using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

public class SceneLoader : MonoBehaviour
{
   private GameObject OutTrans;
   private Animator animatorOutTrans;
   private string OutTransitionAnim;


   public void LoadMenuScene()
   {
      SceneManager.LoadScene("Mainmenu");
   }

   public void LoadSettingScene()
   {
      SceneManager.LoadScene("OptionsMenu");
   }

   public void NextScene()
   {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
   }

   public void StartGame()
   {
      SceneManager.LoadScene("MainGame");
   }

   public void ReloadScene()
   {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
   }

   public void ContinueButton()
   {
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
      Transform child = transform.Find("blabla");
      if (child != null)
      {
         child.gameObject.SetActive(false);
      }
      else
      {
         Debug.LogWarning("GameMenuScreen n�o foi encontrado como filho deste Canvas.");
      }
   }


   public void IntoGame()
   {
      StartCoroutine(Transitioning(StartGame));
   }

   public void IntoMenu()
   {
      StartCoroutine(Transitioning(LoadMenuScene));
   }

   public void IntoSettings()
   {
      StartCoroutine(Transitioning(LoadSettingScene));
   }

   public void RestartGame()
   {
      StartCoroutine(Transitioning(ReloadScene));
   }


   public void AppExit()
   {
      Debug.Log("Fechar app...");
      Application.Quit();
   }



   private IEnumerator Transitioning(Action onComplete)
   {
      OutTrans = GameObject.Find("Canvas")?.transform
  .Find("OutTransition")?.gameObject;
      OutTransitionAnim = "OutTransitionAnim";

      if (OutTrans == null)
      {
         Debug.LogError("GameObject 'OutTransition' n�o foi encontrado!");
         yield break;
      }

      OutTrans.SetActive(true);

      animatorOutTrans = OutTrans.GetComponent<Animator>();
      if (animatorOutTrans == null)
      {
         Debug.LogError(" Animator n�o encontrado em 'OutTransition'.");
         yield break;
      }

      if (string.IsNullOrEmpty(OutTransitionAnim))
      {
         Debug.LogError(" O nome da anima��o est� vazio.");
         yield break;
      }

      animatorOutTrans.Play(OutTransitionAnim);

      while (animatorOutTrans.GetCurrentAnimatorStateInfo(0).IsName(OutTransitionAnim) &&
             animatorOutTrans.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
      {
         yield return null;
      }

      onComplete?.Invoke();
   }



}
