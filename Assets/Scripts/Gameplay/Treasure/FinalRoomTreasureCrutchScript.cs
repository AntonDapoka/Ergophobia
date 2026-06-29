using UnityEngine;

public class FinalRoomTreasureCrutchScript : MonoBehaviour
{
    [SerializeField] private SpawnerPoint[] points;
    private int stage = 1;

    public void OnEnterSpawnTreasures()
    {
        if (StageSwitchScript.Instance.currentStage != stage) return;

        if (TreasureSpawnerScript.Instance != null)
        {
            foreach (SpawnerPoint point in points)
                TreasureSpawnerScript.Instance.SpawnTreasure(point);
        }
        else
        {
            Debug.LogWarning("TreasureSpawnerScript.Instance is null.");
        }
        stage++;
    }
}

