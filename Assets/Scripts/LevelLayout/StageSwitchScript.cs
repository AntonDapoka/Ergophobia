using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSwitchScript : MonoBehaviour
{
    [Header("Level Generation")]
    [SerializeField] private MonoBehaviour levelGeneratorSource;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 playerSpawnOffset;

    [Header("Room State")]
    [SerializeField] private LevelTransitionManager transitionManager;

    [Header("Fade")]
    [SerializeField] private FadeInAndOutScript fade;

    [Header("Safety")]
    [SerializeField] private float generationTimeout = 10f;

    [Header("Audio")]
    [Tooltip("主场景中负责播放游戏BGM的 AudioSource")]
    [SerializeField] private AudioSource mainBGMSource;

    // ==========================================
    // 【新增】剧情与关卡进度配置
    // ==========================================
    [Header("Story & Progression")]
    [Tooltip("总共有几关？(例如3关，打完第3关后播放结局)")]
    [SerializeField] private int maxStage = 3;

    [Tooltip("配置剧情场景名。\nElement 0: 开场剧情(第1幕前)\nElement 1: 第2幕前\nElement 2: 第3幕前\nElement 3: 结局剧情(通关后)")]
    [SerializeField] private string[] storySceneNames;

    public static bool IsStoryPlaying = false;

    private ILevelGenerator levelGenerator;
    private bool isSwitching;
    private int currentStage = 1;

    public int CurrentStage => currentStage;
    public event Action<int> OnStageChanged;

    private void Awake()
    {
        levelGenerator = levelGeneratorSource as ILevelGenerator;
        if (levelGenerator == null && levelGeneratorSource != null)
            Debug.LogError($"[StageSwitchScript] '{levelGeneratorSource.name}' does not implement {nameof(ILevelGenerator)}.", this);
    }

    // ==========================================
    // 【新增】游戏启动时：播放开场剧情
    // ==========================================
    private IEnumerator Start()
    {
        // 游戏刚开始时，先确保屏幕是黑的
        if (fade != null) yield return fade.PlayFadeOut();

        // 播放开场剧情 (对应 Element 0)
        string openingStory = GetStorySceneName(1);
        if (!string.IsNullOrEmpty(openingStory))
        {
            yield return PlayStoryRoutine(openingStory);
        }

        // 剧情结束后，屏幕是黑的，此时淡入显示第一关，让玩家开始游玩
        if (fade != null) yield return fade.PlayFadeIn();
    }

    public void SwitchStage()
    {
        if (isSwitching) return;

        // 判断是否已经打完了最后一关
        if (currentStage >= maxStage)
        {
            StartCoroutine(GameEndRoutine()); // 播放结局
        }
        else
        {
            StartCoroutine(SwitchStageRoutine()); // 正常进入下一关
        }
    }

    private void DestroyAllTreasures()
    {
        var treasures = new List<TreasureScript>(TreasureScript.ActiveTreasures);
        foreach (var treasure in treasures)
        {
            if (treasure != null) treasure.ForceDestroy();
        }
    }

    // ==========================================
    // 【新增】通用的剧情播放协程 (让代码更整洁)
    // ==========================================
    private IEnumerator PlayStoryRoutine(string storySceneName)
    {
        if (string.IsNullOrEmpty(storySceneName)) yield break;

        IsStoryPlaying = true;

        // 1. 叠加加载剧情场景
        yield return SceneManager.LoadSceneAsync(storySceneName, LoadSceneMode.Additive);

        // 2. 屏幕亮起，玩家观看剧情
        if (fade != null) yield return fade.PlayFadeIn();

        // 3. 死循环等待，直到剧情场景里的 StoryController 把 IsStoryPlaying 设为 false
        yield return new WaitUntil(() => !IsStoryPlaying);

        // 4. 剧情看完了，屏幕再次变黑
        if (fade != null) yield return fade.PlayFadeOut();

        // 5. 卸载剧情场景
        yield return SceneManager.UnloadSceneAsync(storySceneName);

        if (mainBGMSource != null)
        {
            mainBGMSource.Stop(); // 先停止当前的进度
            mainBGMSource.Play(); // 从头开始播放
        }
    }

    // ==========================================
    // 幕间切换逻辑 (第1关->第2关，第2关->第3关)
    // ==========================================
    private IEnumerator SwitchStageRoutine()
    {
        isSwitching = true;

        // 1. 屏幕变黑
        if (fade != null) yield return fade.PlayFadeOut();

        currentStage++;
        OnStageChanged?.Invoke(currentStage);

        // 2. 播放下一幕的剧情 (例如 currentStage = 2，对应 Element 1)
        string storySceneToLoad = GetStorySceneName(currentStage);
        yield return PlayStoryRoutine(storySceneToLoad);

        // 3. 后台打扫卫生
        DestroyAllTreasures();
        transitionManager?.TurnOffBlocks();

        // 4. 后台生成新关卡
        bool generationComplete = false;
        List<RoomScript> generatedRooms = null;
        Action<List<RoomScript>> onGenerated = rooms => { generatedRooms = rooms; generationComplete = true; };

        if (levelGenerator != null)
        {
            levelGenerator.OnLevelGenerated += onGenerated;
            try
            {
                levelGenerator.GenerateLevel(playFadeIn: false);
                float timer = 0f;
                while (!generationComplete && timer < generationTimeout)
                {
                    timer += Time.unscaledDeltaTime;
                    yield return null;
                }
                if (!generationComplete) Debug.LogError("[StageSwitchScript] Level generation did not complete in time.", this);
            }
            finally { levelGenerator.OnLevelGenerated -= onGenerated; }
        }

        // 5. 传送玩家到新起点
        TeleportPlayerToStart(generatedRooms);

        // 6. 屏幕亮起，玩家出现在新关卡！
        if (fade != null) yield return fade.PlayFadeIn();

        isSwitching = false;
    }

    // ==========================================
    // 【新增】结局逻辑 (打完最后一关后触发)
    // ==========================================
    private IEnumerator GameEndRoutine()
    {
        isSwitching = true;

        // 1. 屏幕变黑
        if (fade != null) yield return fade.PlayFadeOut();

        // 2. 播放结局剧情 (对应 Element 3，即 maxStage + 1)
        string endingStory = GetStorySceneName(maxStage + 1);
        yield return PlayStoryRoutine(endingStory);

        // 3. 结局播放完毕后的处理
        Debug.Log("<color=yellow>游戏通关！所有剧情播放完毕。</color>");

        // TODO: 在这里添加返回主菜单的代码，例如：
        // SceneManager.LoadScene("MainMenuScene");

        isSwitching = false;
    }

    // ==========================================
    // 辅助方法：根据阶段获取对应的剧情场景名
    // ==========================================
    private string GetStorySceneName(int stage)
    {
        // 数组索引映射：
        // stage 1 -> index 0 (开场)
        // stage 2 -> index 1 (第二幕前)
        // stage 3 -> index 2 (第三幕前)
        // stage 4 -> index 3 (结局)
        int index = stage - 1;
        if (storySceneNames != null && index >= 0 && index < storySceneNames.Length)
        {
            return storySceneNames[index];
        }
        return null;
    }

    private void TeleportPlayerToStart(List<RoomScript> rooms)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("[StageSwitchScript] Player transform is not assigned", this);
            return;
        }

        Vector3 spawnPosition = GetStartPosition(rooms);
        spawnPosition += playerSpawnOffset;

        if (playerTransform.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = spawnPosition;
            rb.rotation = Quaternion.identity;
        }
        else
        {
            playerTransform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        }
    }

    private Vector3 GetStartPosition(List<RoomScript> rooms)
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("[StageSwitchScript] No rooms were generated. Teleporting to origin.", this);
            return Vector3.zero;
        }
        foreach (var room in rooms)
        {
            if (room != null && room.CurrentSlot.X == 0 && room.CurrentSlot.Lane == 0)
                return room.transform.position;
        }

        return rooms[0].transform.position;
    }
}