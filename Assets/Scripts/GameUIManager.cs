using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour {
    public static GameUIManager Instance { get; private set; }

    [Header("Game UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel; // If you need pause functionality
    [SerializeField] private GameObject soundOptionsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text gameOverStatsText;
    [SerializeField] private TMP_Text winStatsText;

    private bool isPaused = false;

    private void Awake() {
        Instance = this;
        // Ensure game over panel is hidden at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (soundOptionsPanel != null)
            soundOptionsPanel.SetActive(false);
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            TogglePause();
        }
    }

    public void TogglePause() {
        isPaused = !isPaused;

        if (pausePanel != null) {
            pausePanel.SetActive(isPaused);

            // Show/hide cursor based on pause state
            Cursor.visible = isPaused;
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

            // Freeze/unfreeze time
            Time.timeScale = isPaused ? 0f : 1f;
        }
    }

    public void ResumeGame() {
        if (isPaused) {
            TogglePause();
        }
    }

    public void ShowGameOver(string statistics) {
        if (gameOverPanel != null) {
            gameOverPanel.SetActive(true);

            if (gameOverStatsText != null) {
                gameOverStatsText.text = statistics;
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ShowWinGame(string statistics) {
        if (winPanel != null) {
            winPanel.SetActive(true);

            if (winStatsText != null) {
                winStatsText.text = statistics;
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void HideGameOver() {
        if (gameOverPanel != null) {
            gameOverPanel.SetActive(false);
        }
    }

    public void HideWinGame() {
        if (winPanel != null) {
            winPanel.SetActive(false);
        }
    }

    public void ShowSoundOptions() {
        pausePanel.SetActive(false);
        soundOptionsPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void ShowControls() {
        pausePanel.SetActive(false);
        soundOptionsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void BackToPause() {
        pausePanel.SetActive(true);
        soundOptionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }
}
