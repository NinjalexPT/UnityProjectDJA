using System.Collections;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
   [Header("UI")]
   [SerializeField] private GameObject blackScreen;
   [SerializeField] private GameObject fadeIn;
   [SerializeField] private GameObject fadeOut;

   [Header("Sound")]
   [SerializeField] private AudioSource audioSource;
   [SerializeField] private AudioClip audioClip;

   [Header("Animation")]
   [SerializeField] private GameObject cameraObj;
   [SerializeField] private AnimationClip animationClip;

   void Awake()
   {

      if (audioSource == null) { Debug.LogError("audio source not found"); audioSource = FindFirstObjectByType<AudioSource>(); }

      audioSource.volume = SoundManager.Instance.sfxVolume;

      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
   }

   void Start()
   {
      StartCoroutine(PlayCutscene());
   }

   IEnumerator PlayCutscene()
   {
      print("playing animation");
      audioSource.PlayOneShot(audioClip);
      yield return new WaitForSeconds(audioClip.length);
      blackScreen.SetActive(false);
      fadeIn.SetActive(true);
      cameraObj.GetComponent<Animator>().enabled = true;

      yield return new WaitForSeconds(animationClip.length - 1f); // animation duration
      cameraObj.GetComponent<Animator>().enabled = false;

      Cursor.lockState = CursorLockMode.None;

      FindFirstObjectByType<SceneLoader>().IntoMaze();

   }

}
