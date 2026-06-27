using TMPro;
using UnityEngine;
using MG_BlocksEngine2.Environment;

public class HUDManager : MonoBehaviour
{
    [Header("UI Text Fields")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI bulletsText;

    [Header("Data Sources")]
    [SerializeField] private PlayerHealthDebug playerHealth;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private StageSwitchScript stageSwitch;

    private void Start()
    {
        Subscribe();
        RefreshAll();
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshAll();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += UpdateHealth;

        if (enemySpawner != null)
        {
            enemySpawner.OnEnemySpawned += _ => UpdateEnemies();
            enemySpawner.OnEnemyDestroyed += _ => UpdateEnemies();
            enemySpawner.OnAllEnemiesCleared += UpdateEnemies;
        }

        if (stageSwitch != null)
            stageSwitch.OnStageChanged += UpdateStage;

        BE2_TargetObjectSpacecraft3D.OnBulletSpawned += _ => UpdateBullets();
        BE2_TargetObjectSpacecraft3D.OnBulletDestroyed += _ => UpdateBullets();
    }

    private void Unsubscribe()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealth;

        if (enemySpawner != null)
        {
            enemySpawner.OnEnemySpawned -= _ => UpdateEnemies();
            enemySpawner.OnEnemyDestroyed -= _ => UpdateEnemies();
            enemySpawner.OnAllEnemiesCleared -= UpdateEnemies;
        }

        if (stageSwitch != null)
            stageSwitch.OnStageChanged -= UpdateStage;

        BE2_TargetObjectSpacecraft3D.OnBulletSpawned -= _ => UpdateBullets();
        BE2_TargetObjectSpacecraft3D.OnBulletDestroyed -= _ => UpdateBullets();
    }

    private void RefreshAll()
    {
        UpdateHealth(playerHealth != null ? playerHealth.CurrentHealth : 0f, playerHealth != null ? playerHealth.MaxHealth : 0f);
        UpdateEnemies();
        UpdateStage(stageSwitch != null ? stageSwitch.CurrentStage : 1);
        UpdateBullets();
    }

    private void UpdateHealth(float current, float max)
    {
        if (healthText != null)
            healthText.text = current.ToString();
    }

    private void UpdateEnemies()
    {
        if (enemiesText != null)
        {
            int count = enemySpawner != null ? enemySpawner.GetTotalLivingEnemyCount() : 0;
            enemiesText.text = count.ToString();
        }
    }

    private void UpdateStage(int stage)
    {
        if (stageText != null)
            stageText.text = stage.ToString();
    }

    private void UpdateBullets()
    {
        if (bulletsText != null)
            bulletsText.text = BE2_TargetObjectSpacecraft3D.ActiveBullets.Count.ToString();
    }
}
