using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class Dialogue
{
   [TextArea(2, 5)]
   public string text;
   public float duration = 2f;
}

public class CutsceneManager : MonoBehaviour
{
   [Header("UI")]
   [SerializeField] private GameObject blackScreen;
   [SerializeField] private GameObject fadeIn;
   [SerializeField] private GameObject fadeOut;

   [Space]
   [Header("Dialog")]
   [SerializeField] private GameObject dialogObject;
   [SerializeField] private TextMeshProUGUI dialogTMP;
   [SerializeField] private List<Dialogue> dialogues = new List<Dialogue>();

   [Space]
   [Header("Sound")]
   [SerializeField] private AudioSource audioSource;
   [SerializeField] private AudioClip audioClip;

   [Space]
   [Header("Animation")]
   [SerializeField] private GameObject cameraObj;
   [SerializeField] private AnimationClip animationClip;

   private void Awake()
   {
      if (audioSource == null)
      {
         Debug.LogError("audioSource não está atribuído!");
         audioSource = GetComponent<AudioSource>();
      }
      audioSource.volume = SoundManager.Instance.sfxVolume;
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
   }

   private void Start()
   {
      if (dialogObject != null)
         dialogObject.SetActive(false);

      StartCoroutine(PlayCutscene());
      StartCoroutine(ShowDialogs());
   }

   private IEnumerator ShowDialogs()
   {
      if (dialogObject == null ||
          dialogTMP == null ||
          dialogues.Count == 0)
         yield break;

      dialogObject.SetActive(true);
      foreach (var dlg in dialogues)
      {
         dialogTMP.text = dlg.text;
         yield return new WaitForSeconds(dlg.duration);
      }
      dialogObject.SetActive(false);
   }

   private IEnumerator PlayCutscene()
   {
      audioSource.PlayOneShot(audioClip);
      yield return new WaitForSeconds(audioClip.length);

      blackScreen.SetActive(false);
      fadeIn.SetActive(true);

      var anim = cameraObj?.GetComponent<Animator>();
      if (anim != null) anim.enabled = true;

      yield return new WaitForSeconds(animationClip.length - 1f);

      if (anim != null) anim.enabled = false;

      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;

      FindObjectOfType<SceneLoader>()?.IntoMaze();
   }
}