using UnityEngine;

public class AnimDebug : MonoBehaviour
{
    private void Start()
    {
        Animator[] animators = GetComponentsInChildren<Animator>(true);
        Debug.Log("[AnimDebug] Found " + animators.Length + " Animator(s) in children");

        foreach (Animator a in animators)
        {
            Debug.Log("[AnimDebug] Animator on: " + a.gameObject.name
                + " | Controller: " + (a.runtimeAnimatorController != null ? a.runtimeAnimatorController.name : "NONE")
                + " | Avatar: " + (a.avatar != null ? a.avatar.name : "NONE")
                + " | isValid: " + (a.avatar != null ? a.avatar.isValid.ToString() : "N/A")
                + " | hasRootMotion: " + a.hasRootMotion);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Animator a = GetComponentInChildren<Animator>();
            if (a != null)
            {
                Debug.Log("[AnimDebug] Playing Punch manually");
                a.Play("Punch", 0, 0f);
            }
            else
            {
                Debug.Log("[AnimDebug] No Animator found!");
            }
        }
    }
}
