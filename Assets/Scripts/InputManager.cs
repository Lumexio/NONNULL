using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public bool Jump { get { return Input.GetButtonDown("Jump"); } }
    public bool Punch { get { return Input.GetKeyDown(KeyCode.I) || Input.GetButtonDown("Fire1"); } }
    public bool Kick { get { return Input.GetKeyDown(KeyCode.O) || Input.GetButtonDown("Fire2"); } }
    public bool Elbow { get { return Input.GetKeyDown(KeyCode.P); } }
    public bool Run { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Fire3"); } }
}
