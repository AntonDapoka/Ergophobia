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
    [SerializeField] private AudioSource mainBGMSource;

    [Header("UI")]
    [SerializeField] private GameObject stageHUD;

    [Header("Story & Progression")]
    [SerializeField] private int maxStage = 3;

    [SerializeField] private string[] storySceneNames;

    public static bool IsStoryPlaying = false;

    private ILevelGenerator levelGenerator;
    private bool isSwitching;
    public int currentStage = 1;

    public int CurrentStage => currentStage;
    public event Action<int> OnStageChanged;

    public static StageSwitchScript Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        levelGenerator = levelGeneratorSource as ILevelGenerator;
        if (levelGenerator == null && levelGeneratorSource != null)
            Debug.LogError($"[StageSwitchScript] '{levelGeneratorSource.name}' does not implement {nameof(ILevelGenerator)}.", this);
    }

    private IEnumerator Start()
    {
        // ��Ϸ�տ�ʼʱ����ȷ����Ļ�Ǻڵ�
        if (fade != null) yield return fade.PlayFadeOut();

        // ���ſ������� (��Ӧ Element 0)
        string openingStory = GetStorySceneName(1);
        if (!string.IsNullOrEmpty(openingStory))
        {
            yield return PlayStoryRoutine(openingStory);
        }

        // �����������Ļ�Ǻڵģ���ʱ������ʾ��һ�أ�����ҿ�ʼ����
        if (fade != null) yield return fade.PlayFadeIn();
    }

    public void SwitchStage()
    {
        if (isSwitching) return;

        // �ж��Ƿ��Ѿ����������һ��
        if (currentStage >= maxStage)
        {
            StartCoroutine(GameEndRoutine()); // ���Ž��
        }
        else
        {
            StartCoroutine(SwitchStageRoutine()); // ����������һ��
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

        // 4. ��̨�����¹ؿ�
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


        // TODO: ���������ӷ������˵��Ĵ��룬���磺
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