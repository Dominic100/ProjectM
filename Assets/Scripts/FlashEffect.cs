using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashEffect : MonoBehaviour {
    [SerializeField] private Image flashImage;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float flashInDuration = 0.5f;
    [SerializeField] private float flashOutDuration = 1f;

    private void Awake() {
        Color initialColor = flashImage.color;
        initialColor.a = 0f;
        flashImage.color = initialColor;
    }

    public void Flash() {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine() {
        float elapsedTime = 0f;
        Color color = flashImage.color;
        Debug.Log("Inside coroutine");

        while(elapsedTime<flashInDuration) {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, maxAlpha, elapsedTime / flashInDuration);
            flashImage.color = color;
            yield return null;
        }

        elapsedTime = 0f;
        while(elapsedTime<flashOutDuration) {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(maxAlpha, 0f, elapsedTime / flashOutDuration);
            flashImage.color = color;
            yield return null;
        }

        color.a = 0f;
        flashImage.color = color;
    }
}
