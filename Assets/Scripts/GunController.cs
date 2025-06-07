using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class GunController : MonoBehaviour
{

   [Header("Munição")]
   public int ammoSize = 3;
   public int shotsFired;

   [Header("Beam Settings")]
   public Transform muzzleTransform;
   public float range = 50f;
   public float beamDuration = 0.02f;

   private LineRenderer lineRenderer;

   void Awake()
   {
      lineRenderer = GetComponent<LineRenderer>();

      lineRenderer.positionCount = 2;
      lineRenderer.useWorldSpace = true;
      lineRenderer.startWidth = 0.04f;
      lineRenderer.endWidth = 0.04f;
      lineRenderer.enabled = false;
   }

   void Start()
   {
      shotsFired = 0;
   }

   void Update()
   {
      if (Input.GetMouseButtonDown(0) && !UIManager.isPauseScreenActive)
      {
         Fire();
      }
   }

   public void Fire()
   {
      if (shotsFired < ammoSize)
      {
         shotsFired++;
         GameManager.Instance.uIManager.SetPowerUpText(
             PowerUpType.Gun,
             $"{ammoSize - shotsFired}/{ammoSize}"
         );
         ShootBeam();
      }
      else
      {
         shotsFired = 0;
         GameManager.Instance.powerUpManager.DeactivatePowerUp(PowerUpType.Gun);
         GameManager.Instance.uIManager.SetPowerUpText(
             PowerUpType.Gun,
             "3/3");
      }
   }

   public void ResetAmmo()
   {
      shotsFired = 0;
      Debug.Log("Shots reset.");
   }

   private void ShootBeam()
   {
      Vector3 startPos = muzzleTransform.position;
      Vector3 endPos = startPos + (-muzzleTransform.forward) * range;

      RaycastHit hit;
      if (Physics.Raycast(startPos, -muzzleTransform.forward, out hit, range))
      {
         endPos = hit.point;
         if (hit.collider.CompareTag("Enemy"))
         {
            EnemyController enemy = hit.collider.GetComponent<EnemyController>();
            if (enemy != null)
            {
               enemy.Die();
            }
         }
      }

      lineRenderer.SetPosition(0, startPos);
      lineRenderer.SetPosition(1, endPos);

      SoundManager.Instance.sfxSource.PlayOneShot(
         SoundManager.Instance.gunSound
      );

      StartCoroutine(ShowBeam());
   }

   private IEnumerator ShowBeam()
   {
      lineRenderer.enabled = true;
      yield return new WaitForSeconds(beamDuration);
      lineRenderer.enabled = false;
   }
}