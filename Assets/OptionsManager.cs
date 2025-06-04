using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{

   public static OptionsManager Instance;

   void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
      }
      else
      {
         Destroy(gameObject);
      }
   }

   public void OnChangeVolume()
   {
      SoundManager.Instance.musicVolume = GameObject.Find("MusicSlider").GetComponent<Slider>().value;
      SoundManager.Instance.sfxVolume = GameObject.Find("SFXSlider").GetComponent<Slider>().value;
   }

   public void OnVSyncChange(bool isEnabled)
   {
      QualitySettings.vSyncCount = isEnabled ? 1 : 0;
      if (isEnabled)
      {
         Debug.Log("VSync enabled");
      }
      else
      {
         Debug.Log("VSync disabled");
      }
   }

   public void OnResultionChange(int index)
   {
      Resolution[] resolutions = Screen.resolutions;
      // if (index >= 0 && index < resolutions.Length)
      // {
      //    Resolution selectedResolution = resolutions[index];
      //    Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
      //    Debug.Log($"Resolution changed to: {selectedResolution.width}x{selectedResolution.height}");
      // }
      // else
      // {
      //    Debug.LogWarning("Invalid resolution index selected.");
      // }
      foreach (Resolution resolution in resolutions)
      {
         // if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height)
         // {
         // Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
         Debug.Log($"Resolution changed to: {resolution.width}x{resolution.height}");
         return;
         // }
      }
   }

}
