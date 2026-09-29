using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
    public CanvasGroup pauseGroup;
    public Button resumeBtn;
    public Button controlsBtn;
    public Button mainMenuBtn;
    
    private bool isPaused = false;
    
    void Start() {
        if (resumeBtn != null) resumeBtn.onClick.AddListener(Resume);
        if (mainMenuBtn != null) mainMenuBtn.onClick.AddListener(GoToMainMenu);
    }
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Pause")) {
            if (isPaused) Resume();
            else Pause();
        }
    }
    
    void Pause() {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (pauseGroup != null) {
            pauseGroup.alpha = 1f;
            pauseGroup.interactable = true;
            pauseGroup.blocksRaycasts = true;
        }
    }
    
    void Resume() {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (pauseGroup != null) {
            pauseGroup.alpha = 0f;
            pauseGroup.interactable = false;
            pauseGroup.blocksRaycasts = false;
        }
    }
    
    void GoToMainMenu() {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
