using System.Collections;
using UnityEngine;

public class FadeQuitButtonScript : MonoBehaviour
{
    [SerializeField] private FadeInAndOutScript fadeScript;

    public void OnClick()
    {
        StartCoroutine(HandleClickWithFade());
    }

    private IEnumerator HandleClickWithFade()
    {
        if (fadeScript != null)
        {
            yield return StartCoroutine(fadeScript.PlayFadeOut());
        }

        QuitGame();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}