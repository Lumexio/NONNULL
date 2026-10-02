using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour
{
    // TODO: wire in Inspector — drag Player root transform here
    public Transform pivot;

    [SerializeField] private float distance = 4f;
    [SerializeField] private float heightOffset = 1.5f;
    [SerializeField] private float mouseSensitivityX = 3f;
    [SerializeField] private float mouseSensitivityY = 2f;
    [SerializeField] private float smoothSpeed = 12f;
    [SerializeField] private float margin = 0.15f;

    private float yaw = 0f;
    private float pitch = 15f;
    private Vector3 shakeOffset = Vector3.zero;

    private void Start()
    {
        if (pivot == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) pivot = p.transform;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (pivot == null) return;

        // Read mouse input
        yaw   += Input.GetAxis("Mouse X") * mouseSensitivityX;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivityY;
        pitch  = Mathf.Clamp(pitch, -20f, 70f);

        // Calculate desired camera position by orbiting around pivot
        Quaternion rotation   = Quaternion.Euler(pitch, yaw, 0f);
        Vector3    pivotPos   = pivot.position + Vector3.up * heightOffset;
        Vector3    desiredPos = pivotPos - rotation * Vector3.forward * distance;

        // Raycast to pull camera in when geometry is in the way
        RaycastHit hit;
        Vector3 dir = desiredPos - pivotPos;
        float dist = dir.magnitude;
        if (Physics.Raycast(pivotPos, dir.normalized, out hit, dist))
        {
            desiredPos = hit.point + hit.normal * margin;
        }

        // Smooth position and rotation
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos + shakeOffset,
            Time.deltaTime * smoothSpeed
        );
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotation,
            Time.deltaTime * smoothSpeed
        );
    }

    public void ScreenShake()
    {
        StartCoroutine(DoShake());
    }

    private IEnumerator DoShake()
    {
        float t = 0f;
        while (t < 0.2f)
        {
            shakeOffset  = Random.insideUnitSphere * 0.5f;
            t           += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
