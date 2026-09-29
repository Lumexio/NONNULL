using UnityEngine;

public class DropManager : MonoBehaviour
{
    public static DropManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void HandleDrops(Vector3 position)
    {
        float roll = Random.value;
        string dropTag = "";

        if (roll < 0.2f) dropTag = "coin";
        else if (roll < 0.3f) dropTag = "health";
        else if (roll < 0.4f) dropTag = "powerup";
        else dropTag = "points";

        if (!string.IsNullOrEmpty(dropTag))
        {
            PoolManager.Instance.Spawn(dropTag, position, Quaternion.identity);
        }
    }
}
