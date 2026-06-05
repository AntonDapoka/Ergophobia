using System.Collections;
using UnityEngine;

public class FadeSwitchSceneButtonScript : SwitchSceneButtonScript
{
    [SerializeField] private FadeInAndOutScript fadeScript;

    protected override void OnClick()
    {
        StartCoroutine(HandleClickWithFade());
    }

    private IEnumerator HandleClickWithFade()
    {
        if (fadeScript != null)
        {
            yield return StartCoroutine(fadeScript.PlayFadeOut());
        }

        base.OnClick();
    }
}