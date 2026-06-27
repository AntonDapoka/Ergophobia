using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void SwitchStage()
    {
        if (isSwitching) return;
        StartCoroutine(SwitchStageRoutine());
    }

    private IEnumerator SwitchStageRoutine()
    {
        isSwitching = true;
        currentStage++;
        OnStageChanged?.Invoke(currentStage);

        if (fade != null)
            yield return fade.PlayFadeOut();
        transitionManager?.TurnOffBlocks();

        bool generationComplete = false;
        List<RoomScript> generatedRooms = null;
        Action<List<RoomScript>> onGenerated = rooms =>
        {
            generatedRooms = rooms;
            generationComplete = true;
        };

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

                if (!generationComplete)
                    Debug.LogError("[StageSwitchScript] Level generation did not complete in time.", this);
            }
            finally
            {
                levelGenerator.OnLevelGenerated -= onGenerated;
            }
        }
        else
        {
            Debug.LogError("[StageSwitchScript] Level generator is not assigned.", this);
        }

        TeleportPlayerToStart(generatedRooms);
        if (fade != null)
            yield return fade.PlayFadeIn();

        isSwitching = false;
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
