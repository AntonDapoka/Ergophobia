using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Gameplay/Enemy Config")]
public class EnemyPrefabConfig : ScriptableObject
{
    public EnemyType type;
    public AssetReferenceGameObject prefabReference;
}
