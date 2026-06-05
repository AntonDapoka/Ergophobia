using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SwitchSceneButtonScript : MonoBehaviour
{
    [SerializeField] protected int sceneIndex;
    [SerializeField] protected Button button;

    protected virtual void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    protected virtual void OnClick()
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogWarning("Scene index out of range: " + sceneIndex);
        }
    }
}