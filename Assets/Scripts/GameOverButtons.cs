using UnityEngine;
using UnityEngine.UI;

public class GameOverButtons : MonoBehaviour {
    [SerializeField] private Button mainMenuButton;

    private void OnEnable() {
        SetupButtonListeners();
    }

    private void SetupButtonListeners() {

        // Clear and set up Main Menu button
        if (mainMenuButton != null) {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => {
                if (GameManager.Instance != null)
                    GameManager.Instance.ReturnToMainMenu();
            });
        }
    }

    public void OnMainMenuButton() {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }
}
