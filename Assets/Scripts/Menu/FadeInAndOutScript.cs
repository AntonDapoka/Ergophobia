using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInAndOutScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image panelFade;

    [Header("Curves")]
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Durations")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float startFadeInDuration = 5f;
    [SerializeField] private bool playStartFadeIn = true;

    private void Start()
    {
        if (playStartFadeIn && panelFade != null)
            StartCoroutine(PlayFadeIn(startFadeInDuration));
    }

    public IEnumerator PlayFadeOut(float? duration = null)
    {
        yield return PlayFade(duration ?? fadeOutDuration, fadeOutCurve);
    }

    public IEnumerator PlayFadeIn(float? duration = null)
    {
        yield return PlayFade(duration ?? fadeInDuration, fadeInCurve);
        panelFade?.gameObject.SetActive(false);
    }

    public IEnumerator PlayFadeOutAndIn(float? fadeOutDuration = null, float? fadeInDuration = null, Action onMidpoint = null)
    {
        yield return PlayFadeOut(fadeOutDuration);
        onMidpoint?.Invoke();
        yield return PlayFadeIn(fadeInDuration);
    }

    public void StartFadeIn(float? duration = null)
    {
        if (panelFade == null)
        {
            Debug.LogWarning("Fade image is not assigned");
            return;
        }

        StopAllCoroutines();
        panelFade.gameObject.SetActive(true);
        SetAlpha(1f);
        StartCoroutine(PlayFadeIn(duration));
    }

    private IEnumerator PlayFade(float duration, AnimationCurve curve)
    {
        if (panelFade == null)
        {
            Debug.LogWarning("Fade image is not assigned");
            yield break;
        }

        panelFade.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetAlpha(curve.Evaluate(t));
            yield return null;
        }

        SetAlpha(curve.Evaluate(1f));
    }

    private void SetAlpha(float alpha)
    {
        if (panelFade == null) return;
        Color color = panelFade.color;
        color.a = Mathf.Clamp01(alpha);
        panelFade.color = color;
    }
}
