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

    [Header("UI")]
    [SerializeField] private GameObject stageHUD;

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

    private IEnumerator PlayStoryRoutine(string storySceneName)
    {
        if (string.IsNullOrEmpty(storySceneName)) yield break;

        IsStoryPlaying = true;

        stageHUD?.SetActive(false); 
        yield return SceneManager.LoadSceneAsync(storySceneName, LoadSceneMode.Additive);

        if (fade != null) yield return fade.PlayFadeIn();

        yield return new WaitUntil(() => !IsStoryPlaying);

        if (fade != null) yield return fade.PlayFadeOut();

        yield return SceneManager.UnloadSceneAsync(storySceneName);
        stageHUD?.SetActive(true);
        if (mainBGMSource != null)
        {
            mainBGMSource.Stop(); 
            mainBGMSource.Play(); 
        }
    }

    private IEnumerator SwitchStageRoutine()
    {
        isSwitching = true;

        if (fade != null) yield return fade.PlayFadeOut();

        currentStage++;
        OnStageChanged?.Invoke(currentStage);

        string storySceneToLoad = GetStorySceneName(currentStage);
        yield return PlayStoryRoutine(storySceneToLoad);

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

        TeleportPlayerToStart(generatedRooms);

        if (fade != null) yield return fade.PlayFadeIn();

        isSwitching = false;
    }

    private IEnumerator GameEndRoutine()
    {
        isSwitching = true;
        if (fade != null) yield return fade.PlayFadeOut();

        string endingStory = GetStorySceneName(maxStage + 1);
        yield return PlayStoryRoutine(endingStory);


        // TODO: 在这里添加返回主菜单的代码，例如：
        // SceneManager.LoadScene("MainMenuScene");

        isSwitching = false;
    }

    private string GetStorySceneName(int stage)
    {
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