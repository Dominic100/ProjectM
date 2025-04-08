using UnityEngine;
using UnityEngine.UI;

public class PauseMenuButtons : MonoBehaviour {
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button mainMenuButton;

    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject controlsPanel;

    private void OnEnable() {
        SetupButtonListeners();
    }

    private void SetupButtonListeners() {
        if (resumeButton != null) {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(() => {
                if (GameUIManager.Instance != null)
                    GameUIManager.Instance.ResumeGame();
            });
        }

        if (soundButton != null) {
            soundButton.onClick.RemoveAllListeners();
            soundButton.onClick.AddListener(() => {
                if (GameUIManager.Instance != null)
                    GameUIManager.Instance.ShowSoundOptions();
            });
        }

        if (controlsButton != null) {
            controlsButton.onClick.RemoveAllListeners();
            controlsButton.onClick.AddListener(() => {
                if(GameUIManager.Instance != null)
                    GameUIManager.Instance.ShowControls();
            });
        }

        if (mainMenuButton != null) {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => {
                Time.timeScale = 1f; // Ensure time scale is reset
                if (GameManager.Instance != null)
                    GameManager.Instance.ReturnToMainMenu();
            });
        }
    }

    // Public methods for direct Inspector assignments
    public void OnResumeButton() {
        if (GameUIManager.Instance != null)
            GameUIManager.Instance.ResumeGame();
    }

    public void OnSoundButton() {
        if (soundPanel != null) {
            soundPanel.SetActive(true);
            transform.parent.gameObject.SetActive(false);
        }
    }

    public void OnControlsButton() {
        if (controlsPanel != null) {
            controlsPanel.SetActive(true);
            transform.parent.gameObject.SetActive(false);
        }
    }

    public void OnMainMenuButton() {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }
}
