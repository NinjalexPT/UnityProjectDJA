using System.Collections;
using UnityEngine;

public class PrototypeMachine : MonoBehaviour
{
  [Header("Main parts of the machine")]
  [SerializeField]
  private GameObject[] mainCylinders = new GameObject[3];
  [SerializeField] private float spinDuration = 3f;
  [SerializeField] private float spinSpeed = 720f;

  private Quaternion[] initialRotations;

  void Start()
  {
    // Store initial rotations (0, 90, 0)
    initialRotations = new Quaternion[mainCylinders.Length];
    for (int i = 0; i < mainCylinders.Length; i++)
    {
      initialRotations[i] = mainCylinders[i].transform.localRotation;
    }
  }

  public void PullLever()
  {
    StartCoroutine(SpinAnimation());
  }

  IEnumerator SpinAnimation()
  {
    float timer = 0f;

    while (timer < spinDuration)
    {
      foreach (GameObject cylinder in mainCylinders)
      {
        cylinder.transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
      }

      timer += Time.deltaTime;
      yield return null;
    }

    // Reset to initial rotations
    for (int i = 0; i < mainCylinders.Length; i++)
    {
      mainCylinders[i].transform.localRotation = initialRotations[i];
    }
  }
}