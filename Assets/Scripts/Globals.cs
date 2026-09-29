using UnityEngine;

public static class Globals {
    public static void TriggerGameOver() {
        if (GameManager.Instance != null) {
            GameManager.Instance.TriggerGameOver();
        }
    }
}
