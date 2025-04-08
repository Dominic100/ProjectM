using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour {
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button soundOptionsButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button backButton;

    private void OnEnable() {
        SetupButtonListeners();
    }

    private void SetupButtonListeners() {
        // Clear and set up Play button
        if (playButton != null) {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => {
                if (GameManager.Instance != null)
                    GameManager.Instance.StartGame();
            });
        }

        // Clear and set up Quit button
        if (quitButton != null) {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(() => {
                if (GameManager.Instance != null)
                    GameManager.Instance.QuitGame();
            });
        }

        // Clear and set up Sound Options button
        if (soundOptionsButton != null) {
            soundOptionsButton.onClick.RemoveAllListeners();
            soundOptionsButton.onClick.AddListener(() => {
                if (MainMenuUIManager.Instance != null)
                    MainMenuUIManager.Instance.ShowSoundOptions();
            });
        }

        // Clear and set up Controls button
        if (controlsButton != null) {
            controlsButton.onClick.RemoveAllListeners();
            controlsButton.onClick.AddListener(() => {
                if (MainMenuUIManager.Instance != null)
                    MainMenuUIManager.Instance.ShowControls();
            });
        }

        // Clear and set up Back button
        if (backButton != null) {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => {
                if (MainMenuUIManager.Instance != null)
                    MainMenuUIManager.Instance.ShowMainMenu();
            });
        }
    }

    // Keep these public methods for direct Inspector assignments if needed
    public void OnPlayButton() {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    public void OnSoundOptionsButton() {
        if (MainMenuUIManager.Instance != null)
            MainMenuUIManager.Instance.ShowSoundOptions();
    }

    public void OnControlsButton() {
        if (MainMenuUIManager.Instance != null)
            MainMenuUIManager.Instance.ShowControls();
    }

    public void OnBackToMainButton() {
        if (MainMenuUIManager.Instance != null)
            MainMenuUIManager.Instance.ShowMainMenu();
    }

    public void OnQuitButton() {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }
}
