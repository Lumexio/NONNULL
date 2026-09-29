using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SaveData {
    public int coins = 0;
    public List<string> unlockedItems = new List<string>();
}

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance { get; private set; }

    public int coins = 0;
    public List<string> unlockedItems = new List<string>();

    private const string SAVE_KEY = "SaveData";

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    public void LoadData() {
        SaveData data = LoadGame();
        coins = data.coins;
        unlockedItems = data.unlockedItems != null ? data.unlockedItems : new List<string>();
    }

    public void SaveData() {
        SaveData data = new SaveData();
        data.coins = coins;
        data.unlockedItems = unlockedItems != null ? unlockedItems : new List<string>();
        SaveGame(data);
    }

    public static void SaveGame(SaveData data) {
        if (data == null) return;
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public static SaveData LoadGame() {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json)) {
            SaveData defaultData = new SaveData();
            defaultData.coins = 0;
            defaultData.unlockedItems = new List<string>();
            return defaultData;
        }

        SaveData loaded = JsonUtility.FromJson<SaveData>(json);
        if (loaded == null) {
            loaded = new SaveData();
        }
        if (loaded.unlockedItems == null) {
            loaded.unlockedItems = new List<string>();
        }
        return loaded;
    }

    public void AddCoins(int amount) {
        coins += amount;
        SaveData();
    }

    public int GetCoins() {
        return coins;
    }

    public bool IsUnlocked(string itemName) {
        if (string.IsNullOrEmpty(itemName)) return false;
        return unlockedItems != null && unlockedItems.Contains(itemName);
    }

    public bool UnlockItem(string itemName, int price = 0) {
        if (string.IsNullOrEmpty(itemName)) return false;
        if (IsUnlocked(itemName)) return true;

        if (coins >= price) {
            coins -= price;
            if (unlockedItems == null) {
                unlockedItems = new List<string>();
            }
            unlockedItems.Add(itemName);
            SaveData();
            return true;
        }
        return false;
    }
}
