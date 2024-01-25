using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesLoader
{
    public static uint AddressableContentUsersCount { get; private set; }

    public static async Task<T> LoadAsync<T>(string assetId)
    {
        AddressableContentUsersCount++;

        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetId);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            throw new NullReferenceException($"Object of type {typeof(T)} with id {assetId} can`t be loaded via Addressables");
        }

        AddressableContentUsersCount--;
        Logger.Log($"Loaded asset: {assetId}", Logger.LogSource.Addressables);
        
        return handle.Result;
    }
    
    public static void Unload(object obj)
    {
        if (obj == null)
        {
            return;
        }
        
        string objectType = obj.GetType().Name;
        string objectName = obj.GetType().Name;

        Addressables.Release(obj);
        
        Logger.Log($"Unloaded asset: Type: {objectType}; Name: {objectName}", Logger.LogSource.Addressables);
    }
    
    public static void UnloadInstance(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        string goType = go.GetType().Name;
        string goName = go.name;
        
        go.SetActive(false);
        Addressables.ReleaseInstance(go);
        
        Logger.Log($"Unloaded instance of asset: Type: {goType}; Name: {goName}", Logger.LogSource.Addressables);
    }
}
