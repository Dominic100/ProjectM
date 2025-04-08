using UnityEngine;

public class MainMenuUIManager : MonoBehaviour {
    public static MainMenuUIManager Instance { get; private set; }

    [Header("Main Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject soundOptionsPanel;
    [SerializeField] private GameObject controlsPanel;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        ShowMainMenu();
    }

    public void ShowMainMenu() {
        mainMenuPanel.SetActive(true);
        soundOptionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void ShowSoundOptions() {
        mainMenuPanel.SetActive(false);
        soundOptionsPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void ShowControls() {
        mainMenuPanel.SetActive(false);
        soundOptionsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }
}