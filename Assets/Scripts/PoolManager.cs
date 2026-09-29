using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour {
    public static PoolManager Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<PoolManager>();
                if (_instance == null) {
                    GameObject go = new GameObject("PoolManager");
                    _instance = go.AddComponent<PoolManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }
    private static PoolManager _instance;

    public GameObject weakEnemyPrefab;
    public GameObject toughEnemyPrefab;
    public GameObject coinPrefab;
    public GameObject healthPrefab;
    public GameObject pointsPrefab;
    public GameObject powerupPrefab;
    public GameObject bossProjectilePrefab;

    private Dictionary<string, Queue<GameObject>> poolQueues = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, List<GameObject>> allObjects = new Dictionary<string, List<GameObject>>();
    private bool isInitialized = false;

    void Awake() {
        if (_instance == null || _instance == this) {
            _instance = this;
            InitializePools();
        } else {
            Destroy(gameObject);
        }
    }

    public void InitializePools() {
        if (isInitialized) return;
        isInitialized = true;

        CreatePool("weak", weakEnemyPrefab, 100);
        CreatePool("tough", toughEnemyPrefab, 50);
        CreatePool("coin", coinPrefab, 50);
        CreatePool("health", healthPrefab, 30);
        CreatePool("points", pointsPrefab, 50);
        CreatePool("powerup", powerupPrefab, 20);
        CreatePool("boss_projectile", bossProjectilePrefab, 50);
    }

    private void CreatePool(string poolTag, GameObject prefab, int capacity) {
        string tag = NormalizeTag(poolTag);
        if (!poolQueues.ContainsKey(tag)) {
            poolQueues[tag] = new Queue<GameObject>(capacity);
            allObjects[tag] = new List<GameObject>(capacity);
        }

        GameObject poolRoot = new GameObject("Pool_" + tag);
        poolRoot.transform.SetParent(this.transform);

        for (int i = 0; i < capacity; i++) {
            GameObject obj = null;
            if (prefab != null) {
                obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            } else {
                obj = CreateFallbackObject(tag);
            }

            obj.name = tag + "_" + i;
            obj.transform.SetParent(poolRoot.transform);
            obj.SetActive(false);

            poolQueues[tag].Enqueue(obj);
            allObjects[tag].Add(obj);
        }
    }

    private GameObject CreateFallbackObject(string tag) {
        GameObject obj = new GameObject("Fallback_" + tag);
        if (tag == "weak") {
            EnemyAI ai = obj.AddComponent<EnemyAI>();
            ai.isTough = false;
            obj.AddComponent<EnemyHealth>();
            obj.AddComponent<CharacterController>();
        } else if (tag == "tough") {
            EnemyAI ai = obj.AddComponent<EnemyAI>();
            ai.isTough = true;
            obj.AddComponent<EnemyHealth>();
            obj.AddComponent<CharacterController>();
        } else if (tag == "coin") {
            Pickup p = obj.AddComponent<Pickup>();
            p.pickupType = "Coin";
            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.isTrigger = true;
        } else if (tag == "health") {
            Pickup p = obj.AddComponent<Pickup>();
            p.pickupType = "Health";
            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.isTrigger = true;
        } else if (tag == "points") {
            Pickup p = obj.AddComponent<Pickup>();
            p.pickupType = "Points";
            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.isTrigger = true;
        } else if (tag == "powerup") {
            Pickup p = obj.AddComponent<Pickup>();
            p.pickupType = "Power";
            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.isTrigger = true;
        } else if (tag == "boss_projectile") {
            obj.AddComponent<BossProjectile>();
            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.isTrigger = true;
        }
        return obj;
    }

    public GameObject Spawn(string poolTag, Vector3 position, Quaternion rotation) {
        string tag = NormalizeTag(poolTag);
        Queue<GameObject> queue = null;
        if (!poolQueues.TryGetValue(tag, out queue) || queue.Count == 0) {
            Debug.LogWarning("PoolManager: Pool exhausted or missing for tag: " + poolTag);
            return null;
        }

        GameObject obj = queue.Dequeue();
        if (obj == null) return null;

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void Recycle(string poolTag, GameObject obj) {
        if (obj == null) return;

        string tag = NormalizeTag(poolTag);
        obj.SetActive(false);

        Queue<GameObject> queue = null;
        if (poolQueues.TryGetValue(tag, out queue)) {
            if (!queue.Contains(obj)) {
                queue.Enqueue(obj);
            }
        }
    }

    public void RecycleAll() {
        foreach (KeyValuePair<string, List<GameObject>> kvp in allObjects) {
            string tag = kvp.Key;
            List<GameObject> list = kvp.Value;
            Queue<GameObject> queue = poolQueues[tag];

            for (int i = 0; i < list.Count; i++) {
                GameObject obj = list[i];
                if (obj != null && obj.activeSelf) {
                    obj.SetActive(false);
                }
                if (!queue.Contains(obj)) {
                    queue.Enqueue(obj);
                }
            }
        }
    }

    public int GetAvailableCount(string poolTag) {
        string tag = NormalizeTag(poolTag);
        Queue<GameObject> queue = null;
        if (poolQueues.TryGetValue(tag, out queue)) {
            return queue.Count;
        }
        return 0;
    }

    public int GetTotalCapacity(string poolTag) {
        string tag = NormalizeTag(poolTag);
        List<GameObject> list = null;
        if (allObjects.TryGetValue(tag, out list)) {
            return list.Count;
        }
        return 0;
    }

    private string NormalizeTag(string tag) {
        if (string.IsNullOrEmpty(tag)) return "";
        string lower = tag.ToLowerInvariant().Trim();
        if (lower == "power") return "powerup";
        return lower;
    }
}
