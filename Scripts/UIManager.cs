using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Image healthBar;
    public Text pointsText;
    public Text roundBannerText;
    public GameObject gameOverScreen;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ShowBossWarning()
    {
        if (roundBannerText != null) roundBannerText.text = "WARNING: BOSS INCOMING";
    }

    public void UpdateRound()
    {
        if (roundBannerText != null) roundBannerText.text = "ROUND " + LevelManager.Instance.currentRound;
    }

    public void ShowGameOver()
    {
        if (gameOverScreen != null) gameOverScreen.SetActive(true);
    }
}
