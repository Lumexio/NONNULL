using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void CreatePool(string tag, GameObject prefab, int size)
    {
        if (!pools.ContainsKey(tag))
        {
            pools[tag] = new Queue<GameObject>();
            for (int i = 0; i < size; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pools[tag].Enqueue(obj);
            }
        }
    }

    public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
    {
        if (pools.ContainsKey(tag) && pools[tag].Count > 0)
        {
            GameObject obj = pools[tag].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public void Despawn(string tag, GameObject obj)
    {
        obj.SetActive(false);
        if (pools.ContainsKey(tag))
        {
            pools[tag].Enqueue(obj);
        }
    }
}
