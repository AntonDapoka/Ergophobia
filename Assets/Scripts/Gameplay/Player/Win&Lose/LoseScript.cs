using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoseScript : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image panel;
    [SerializeField] private Image imageLose;
    [SerializeField] private Button buttonExit;
    [SerializeField] private float fadeOutDuration = 5f;
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private void Start()
    {

        canvas.gameObject.SetActive(false);
        
    }

    public void Lose()
    {
        canvas.gameObject.SetActive(true);
        buttonExit.gameObject.SetActive(false);
        StartCoroutine(PlayFadeOut());
    }

    private IEnumerator PlayFadeOut()
    {
        StartCoroutine(PlayFade(panel, fadeOutDuration, fadeOutCurve));
        yield return PlayFade(imageLose, fadeOutDuration, fadeOutCurve);
        buttonExit.gameObject.SetActive(true);
    }

    private IEnumerator PlayFade(Image image, float duration, AnimationCurve curve)
    {
        if (image == null)
        {
            Debug.LogWarning("Fade image is not assigned");
            yield break;
        }

        image.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetAlpha(image, curve.Evaluate(t));
            yield return null;
        }

        SetAlpha(image, curve.Evaluate(1f));
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null) return;
        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }
}
