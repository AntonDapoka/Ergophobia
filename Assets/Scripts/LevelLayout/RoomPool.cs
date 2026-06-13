using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RoomPool : MonoBehaviour
{
    [SerializeField] private Transform poolParent;

    private Dictionary<string, Queue<GameObject>> pools = new();
    private Dictionary<string, AsyncOperationHandle<GameObject>> loadedPrefabs = new Dictionary<string, AsyncOperationHandle<GameObject>>();

    public IEnumerator InitializeAsync(List<RoomPrefabConfig> configs)
    {
        foreach (var config in configs)
        {
            if (config.roomPrefabReference == null || !config.roomPrefabReference.RuntimeKeyIsValid())
            {
                Debug.LogError($"RoomPrefabConfig '{config.name}' has invalid AssetReference.");
                continue;
            }

            string key = config.roomPrefabReference.RuntimeKey.ToString();
            if (loadedPrefabs.ContainsKey(key)) continue;

            var handle = config.roomPrefabReference.LoadAssetAsync<GameObject>();
            loadedPrefabs[key] = handle;
            yield return handle;
        }
    }

    public IEnumerator GetAsync(RoomPrefabConfig config, Transform parent, System.Action<GameObject> onComplete)
    {
        if (config.roomPrefabReference == null || !config.roomPrefabReference.RuntimeKeyIsValid())
        {
            Debug.LogError($"RoomPrefabConfig '{config.name}' has invalid AssetReference.");
            onComplete?.Invoke(null);
            yield break;
        }

        string key = config.roomPrefabReference.RuntimeKey.ToString();

        if (pools.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            GameObject reused = queue.Dequeue();
            reused.transform.SetParent(parent);
            reused.transform.localPosition = Vector3.zero;
            reused.transform.localRotation = Quaternion.identity;
            reused.SetActive(true);
            onComplete?.Invoke(reused);
            yield break;
        }

        if (!loadedPrefabs.TryGetValue(key, out var handle))
        {
            handle = config.roomPrefabReference.LoadAssetAsync<GameObject>();
            loadedPrefabs[key] = handle;
            yield return handle;
        }

        if (handle.Result == null)
        {
            Debug.LogError($"Failed to load room prefab with key '{key}'.");
            onComplete?.Invoke(null);
            yield break;
        }

        GameObject instance = Instantiate(handle.Result, parent);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        onComplete?.Invoke(instance);
    }

    public void Return(string key, GameObject go)
    {
        if (go == null) return;

        go.SetActive(false);
        go.transform.SetParent(poolParent != null ? poolParent : transform);

        if (!pools.ContainsKey(key))
            pools[key] = new Queue<GameObject>();
        pools[key].Enqueue(go);
    }

    public void Return(RoomPrefabConfig config, GameObject go)
    {
        if (config?.roomPrefabReference == null) return;
        string key = config.roomPrefabReference.RuntimeKey.ToString();
        Return(key, go);
    }

    public void ReleaseAll()
    {
        foreach (var kvp in loadedPrefabs)
            Addressables.Release(kvp.Value);

        loadedPrefabs.Clear();
        pools.Clear();
    }
}
