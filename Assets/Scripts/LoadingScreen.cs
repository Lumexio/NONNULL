using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour {
    public Image progressBar;
    
    // Globals.nextScene should be set by the Main Menu before switching to LoadingScreen
    public static string nextScene = "Level1"; 

    void Start() {
        StartCoroutine(LoadLevelAsync(nextScene)); 
    }

    IEnumerator LoadLevelAsync(string sceneName) {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        
        while (op.progress < 0.9f) {
            if (progressBar != null) progressBar.fillAmount = op.progress / 0.9f;
            yield return null;
        }
        
        if (progressBar != null) progressBar.fillAmount = 1f;
        yield return new WaitForSeconds(0.5f); // Smooth transition
        op.allowSceneActivation = true;
    }
}
