using UnityEngine;
using UnityEditor;
using System.IO;

public class WireUpMigration
{
    [MenuItem("Migration/Auto Wire Up")]
    public static void DoWireUp()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Migration")) AssetDatabase.CreateFolder("Assets/Prefabs", "Migration");

        // 1. Setup Systems
        GameObject systems = new GameObject("_Systems");
        systems.AddComponent<GameManager>();
        systems.AddComponent<InputManager>();
        systems.AddComponent<PoolManager>();
        systems.AddComponent<SaveManager>();
        systems.AddComponent<DropManager>();
        systems.AddComponent<FlockManager>();
        systems.AddComponent<LevelManager>();
        systems.AddComponent<UIManager>();
        PrefabUtility.CreatePrefab("Assets/Prefabs/Migration/Systems.prefab", systems);

        // 2. Setup Player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.AddComponent<CharacterController>();
        player.AddComponent<PlayerController>();
        PrefabUtility.CreatePrefab("Assets/Prefabs/Migration/Player.prefab", player);
        Object.DestroyImmediate(player);

        // 3. Setup Enemies
        GameObject weakEnemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        weakEnemy.name = "WeakEnemy";
        var weakAI = weakEnemy.AddComponent<EnemyAI>();
        weakAI.isTough = false;
        weakAI.poolTag = "weak";
        PrefabUtility.CreatePrefab("Assets/Prefabs/Migration/WeakEnemy.prefab", weakEnemy);
        Object.DestroyImmediate(weakEnemy);

        GameObject toughEnemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        toughEnemy.name = "ToughEnemy";
        var toughAI = toughEnemy.AddComponent<EnemyAI>();
        toughAI.isTough = true;
        toughAI.poolTag = "tough";
        PrefabUtility.CreatePrefab("Assets/Prefabs/Migration/ToughEnemy.prefab", toughEnemy);
        Object.DestroyImmediate(toughEnemy);

        GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        boss.name = "Boss";
        boss.AddComponent<BossAI>();
        PrefabUtility.CreatePrefab("Assets/Prefabs/Migration/Boss.prefab", boss);
        Object.DestroyImmediate(boss);

        Debug.Log("Migration Wire Up Complete! Prefabs created in Assets/Prefabs/Migration/");
    }
}
