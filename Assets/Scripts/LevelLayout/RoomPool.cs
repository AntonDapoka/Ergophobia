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
    private HashSet<string> ownedKeys = new HashSet<string>();

    private void OnDestroy()
    {
        ReleaseAll();
    }

    public IEnumerator InitializeAsync(List<RoomPrefabConfig> configs)
    {
        foreach (var config in configs)
        {
            if (config == null || config.roomPrefabReference == null || !config.roomPrefabReference.RuntimeKeyIsValid())
            {
                Debug.LogError($"RoomPrefabConfig '{config?.name}' has invalid AssetReference.");
                continue;
            }

            var handle = GetOrCreateHandle(config);
            yield return handle;
        }
    }

    public IEnumerator GetAsync(RoomPrefabConfig config, Transform parent, System.Action<GameObject> onComplete)
    {
        if (config == null || config.roomPrefabReference == null || !config.roomPrefabReference.RuntimeKeyIsValid())
        {
            Debug.LogError($"RoomPrefabConfig '{config?.name}' has invalid AssetReference.");
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

        var handle = GetOrCreateHandle(config);
        if (!handle.IsDone)
            yield return handle;

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

    private AsyncOperationHandle<GameObject> GetOrCreateHandle(RoomPrefabConfig config)
    {
        string key = config.roomPrefabReference.RuntimeKey.ToString();

        if (loadedPrefabs.TryGetValue(key, out var existingHandle))
            return existingHandle;

        // AssetReference keeps an internal OperationHandle. Calling LoadAssetAsync again while it
        // is valid throws "Attempting to load AssetReference that has already been loaded.".
        // Reuse the existing handle or load once and track that we own the load so we can release it.
        AsyncOperationHandle<GameObject> handle;
        if (config.roomPrefabReference.OperationHandle.IsValid())
        {
            handle = config.roomPrefabReference.OperationHandle.Convert<GameObject>();
        }
        else
        {
            handle = config.roomPrefabReference.LoadAssetAsync<GameObject>();
            ownedKeys.Add(key);
        }

        loadedPrefabs[key] = handle;
        return handle;
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
        foreach (string key in ownedKeys)
        {
            if (loadedPrefabs.TryGetValue(key, out var handle) && handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        loadedPrefabs.Clear();
        ownedKeys.Clear();
        pools.Clear();
    }
}
