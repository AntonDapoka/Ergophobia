using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))] // 【新增】强制要求挂载 AudioSource 组件
public class StoryController : MonoBehaviour
{
    [System.Serializable]
    public struct StoryPage
    {
        [Tooltip("当前页的图片")]
        public Sprite image;
        [Tooltip("当前页的文字内容")]
        [TextArea(3, 5)]
        public string text;
    }

    [Header("UI References")]
    [SerializeField] private Image storyImageDisplay;
    [SerializeField] private TextMeshProUGUI storyTextDisplay;

    [Header("Story Content")]
    [SerializeField] private StoryPage[] pages;

    // ==========================================
    // 【新增】主场景 BGM 控制
    // ==========================================
    [Header("Audio")]
    [Tooltip("主场景中负责播放游戏BGM的 AudioSource")]
    [SerializeField] private AudioSource mainBGMSource;

    [Header("Story & Progression")]
    [SerializeField] private int maxStage = 3;
    [Header("Audio Settings")]
    [Tooltip("剧情播放时的专属BGM")]
    [SerializeField] private AudioClip storyBGM;

    private AudioSource storyAudioSource;
    private int currentPageIndex = 0;
    private bool isFinished = false;

    private void Awake()
    {
        // 【新增】初始化音频组件
        storyAudioSource = GetComponent<AudioSource>();
        storyAudioSource.loop = true; // BGM 循环播放
        storyAudioSource.playOnAwake = false;

        // 核心魔法：让这个音频源免疫全局暂停！
        storyAudioSource.ignoreListenerPause = true;
    }

    private void Start()
    {
        // ==========================================
        // 【新增】暂停主游戏的所有声音，并播放剧情BGM
        // ==========================================
        AudioListener.pause = true; // 暂停底层主场景的所有声音（包括正在生成的新关卡）

        if (storyBGM != null)
        {
            storyAudioSource.clip = storyBGM;
            storyAudioSource.Play();
        }

        if (pages == null || pages.Length == 0)
        {
            Debug.LogWarning("[StoryController] 没有配置剧情页！直接结束剧情。");
            FinishStory();
            return;
        }

        // 初始化显示第一页
        currentPageIndex = 0;
        ShowPage(currentPageIndex);
    }

    private void Update()
    {
        if (isFinished) return;

        // 1. 监听鼠标左键点击 (或者空格键/回车键)
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            NextPage();
        }

        // 2. 监听 ESC 键跳过剧情
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FinishStory();
        }
    }

    private void ShowPage(int index)
    {
        if (storyImageDisplay != null)
        {
            storyImageDisplay.sprite = pages[index].image;
            storyImageDisplay.enabled = pages[index].image != null;
        }

        if (storyTextDisplay != null)
        {
            storyTextDisplay.text = pages[index].text;
        }
    }

    private void NextPage()
    {
        currentPageIndex++;

        if (currentPageIndex >= pages.Length)
        {
            FinishStory();
        }
        else
        {
            ShowPage(currentPageIndex);
        }
    }

    public void FinishStory()
    {
        if (isFinished) return;
        isFinished = true;

        // ==========================================
        // 【新增】恢复主游戏的声音，停止剧情BGM
        // ==========================================
        AudioListener.pause = false; // 恢复全局声音，新关卡的BGM会自然响起
        storyAudioSource.Stop();

        // 告诉主场景：剧情结束了！
        StageSwitchScript.IsStoryPlaying = false;
    }

    // 【新增】安全机制：防止场景被意外销毁时，游戏一直处于静音状态
    private void OnDestroy()
    {
        AudioListener.pause = false;
    }
}