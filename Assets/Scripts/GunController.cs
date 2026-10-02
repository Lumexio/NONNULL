using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour
{
    // TODO: wire in Inspector — drag player right hand bone Transform here
    [SerializeField] private Transform handBone;
    [SerializeField] private GameObject gunPrefab;
    [SerializeField] private GameObject muzzleFlashPrefab;

    private GameObject gunInstance;
    private Transform muzzleTip;

    private void Start()
    {
        if (handBone != null && gunPrefab != null)
        {
            gunInstance = Instantiate(gunPrefab, handBone);
            gunInstance.transform.localPosition = Vector3.zero;
            gunInstance.transform.localRotation = Quaternion.identity;
            muzzleTip = gunInstance.transform.Find("MuzzleTip");
            if (muzzleTip == null) muzzleTip = gunInstance.transform;
        }
    }

    public void OnFire()
    {
        if (muzzleFlashPrefab != null && muzzleTip != null)
        {
            StartCoroutine(ShowMuzzleFlash());
        }
    }

    private IEnumerator ShowMuzzleFlash()
    {
        GameObject flash = Instantiate(muzzleFlashPrefab, muzzleTip.position, muzzleTip.rotation);
        yield return new WaitForSeconds(0.05f);
        Destroy(flash);
    }
}
