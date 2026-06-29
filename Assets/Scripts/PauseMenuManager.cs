using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI 面板引用")]
    [Tooltip("暂停主面板")]
    public GameObject pauseMenuPanel;

    [Header("设置")]
    public string mainMenuSceneName = "MainMenuScene";

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {               
                ResumeGame();
                
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f; // 恢复游戏时间
        AudioListener.pause = false; // 恢复游戏音效（可选）

        isPaused = false;
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f; // 冻结游戏时间
        AudioListener.pause = true; // 暂停游戏音效（可选，防止暂停时还有环境音）

        isPaused = true;
    }


    /// <summary>
    /// 确认退出游戏 (绑定给确认面板的"确认"按钮)
    /// </summary>
    public void ConfirmQuit()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}