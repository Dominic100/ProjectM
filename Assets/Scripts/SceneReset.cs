using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReset : MonoBehaviour {
    public static SceneReset Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public void ResetScene() {
        // Reset time scale
        Time.timeScale = 1f;

        // Get the current scene and reload it
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
