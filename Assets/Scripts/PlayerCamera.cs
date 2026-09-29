using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour
{
    public Transform pivot;
    public Vector3 targetOffset = new Vector3(0, 2, -5);
    public float lookSmoothSpeed = 12f;
    public float cameraLagSpeed = 12f;
    public float margin = 0.2f;

    private Vector3 shakeOffset = Vector3.zero;

    private void LateUpdate()
    {
        if (pivot == null) return;

        Vector3 desiredPos = pivot.position + pivot.TransformDirection(targetOffset);
        
        // Raycast for collision
        RaycastHit hit;
        if (Physics.Raycast(pivot.position, desiredPos - pivot.position, out hit, targetOffset.magnitude))
        {
            desiredPos = hit.point + hit.normal * margin;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPos + shakeOffset, Time.deltaTime * cameraLagSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, pivot.rotation, Time.deltaTime * lookSmoothSpeed);
    }

    public void ScreenShake()
    {
        StartCoroutine(DoShake());
    }

    private IEnumerator DoShake()
    {
        float t = 0;
        while (t < 0.2f)
        {
            shakeOffset = Random.insideUnitSphere * 0.5f;
            t += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
