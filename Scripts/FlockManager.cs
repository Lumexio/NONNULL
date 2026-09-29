using UnityEngine;
using System.Collections.Generic;

public class FlockManager : MonoBehaviour
{
    public static FlockManager Instance { get; private set; }
    
    public List<Transform> activeEnemies = new List<Transform>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Register(Transform enemy) { if (!activeEnemies.Contains(enemy)) activeEnemies.Add(enemy); }
    public void Unregister(Transform enemy) { activeEnemies.Remove(enemy); }
}
