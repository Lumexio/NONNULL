using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public int Coins { get; set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
}
