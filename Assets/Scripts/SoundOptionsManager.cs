using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SoundOptionsManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private TextMeshProUGUI volumeText;

    private const string MIXER_MASTER = "MasterVolume";
    private const string VOLUME_KEY = "GameVolume";
    private const float DEFAULT_VOLUME = 0.75f;

    private void Awake() {
        // Load and apply volume setting before anything else plays
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);
        ApplyVolume(savedVolume);
    }

    private void Start() {
        InitializeVolumeSlider();
    }

    private void InitializeVolumeSlider() {
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);
        volumeSlider.value = savedVolume;
        volumeSlider.onValueChanged.AddListener(HandleVolumeChange);

        // Update the volume text
        if (volumeText != null) {
            volumeText.text = $"{Mathf.Round(savedVolume * 100)}%";
        }
    }

    private void HandleVolumeChange(float sliderValue) {
        ApplyVolume(sliderValue);

        // Save the slider value
        PlayerPrefs.SetFloat(VOLUME_KEY, sliderValue);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float volumeValue) {
        // Convert slider value (0 to 1) to decibels
        float dB;
        if (volumeValue <= 0) {
            dB = -80f;
        }
        else {
            // Convert from linear (0-1) to logarithmic scale
            dB = 20f * Mathf.Log10(volumeValue);
        }

        // Set volume using the exposed parameter name
        mainMixer.SetFloat(MIXER_MASTER, dB);

        // Update volume text if assigned
        if (volumeText != null) {
            volumeText.text = $"{Mathf.Round(volumeValue * 100)}%";
        }
    }
}
