using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public Transform sphereBg;

    void Start() {
        Time.timeScale = 1f; // Ensure time isn't frozen from a previous game over/pause
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update() {
        if (sphereBg != null) {
            sphereBg.Rotate(0, 15f * Time.deltaTime, 0);
        }
    }

    public void StartGame() {
        LoadingScreen.nextScene = "Level1";
        SceneManager.LoadScene("LoadingScreen");
    }
    
    public void OpenShop() {
        Debug.Log("Shop not yet implemented.");
    }

    public void QuitGame() {
        Application.Quit();
        Debug.Log("Quit Requested");
    }
}
