using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioMixer mainMixer;

    [Header("Game Statistics")]
    public int staminaPotionsCollected = 0;
    public int flashbangsCollected = 0;
    public int commonKeysCollected = 0;
    public int finalKeysCollected = 0;
    public float gameTime = 0f;

    private bool isGameActive = false;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this) // Add this check
        {
            // If there's already an instance and it's not this one, destroy this one
            Destroy(gameObject);
            return;
        }
    }

    private void Update() {
        if (isGameActive) {
            gameTime += Time.deltaTime;
        }
    }

    private void Start() {
        LoadVolumeSetting();
    }

    private void LoadVolumeSetting() {
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 0.75f);
        float dB = savedVolume <= 0 ? -80f : Mathf.Log10(savedVolume) * 20f;
        mainMixer.SetFloat("MasterVolume", dB);
    }

    public void StartGame() {
        Time.timeScale = 1f;
        ResetStatistics();
        isGameActive = true;
        SceneManager.LoadScene("SampleScene");
    }

    public void GameOver() {
        isGameActive = false;
        Time.timeScale = 0f;
        GameUIManager.Instance.ShowGameOver(GetStatistics());
    }

    public void RestartGame() {
        StartCoroutine(RestartGameRoutine());
    }

    public void WinGame() {
        isGameActive = false;
        Time.timeScale = 0f;
        GameUIManager.Instance.ShowWinGame(GetStatistics());
    }

    private IEnumerator RestartGameRoutine() {
        // Reset time scale first
        Time.timeScale = 1f;

        // Wait for one frame to ensure everything is updated
        yield return null;

        // Load the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetVolume(float volume) {
        if (musicSource != null) {
            musicSource.volume = volume;
            PlayerPrefs.SetFloat("GameVolume", volume);
            PlayerPrefs.Save();
        }
    }

    public void CollectItem(string itemType) {
        switch (itemType) {
            case "StaminaPotion":
                staminaPotionsCollected++;
                break;
            case "Flashbang":
                flashbangsCollected++;
                break;
            case "CommonKey":
                commonKeysCollected++;
                break;
            case "FinalKey":
                finalKeysCollected++;
                break;
        }
    }

    private void ResetStatistics() {
        staminaPotionsCollected = 0;
        flashbangsCollected = 0;
        commonKeysCollected = 0;
        finalKeysCollected = 0;
        gameTime = 0f;
    }

    private string GetStatistics() {
        return string.Format("Time: {0:F2}s\nStamina Potions: {1}/3\nFlashbangs: {2}/3\nCommon Keys: {3}/5\nFinal Key: {4}/1",
            gameTime, staminaPotionsCollected, flashbangsCollected, commonKeysCollected, finalKeysCollected);
    }
}
