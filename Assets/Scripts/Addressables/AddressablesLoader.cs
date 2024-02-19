using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesLoader
{
    public static async Task<T> LoadAsync<T>(string assetId)
    {
        float startTime = Time.realtimeSinceStartup;
        
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetId);
        await handle.Task;
        
        float endTime = Time.realtimeSinceStartup;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Logger.Log($"Object of type {typeof(T)} with id {assetId} can`t be loaded via Addressables", Logger.LogLevel.Error, Logger.LogSource.Addressables);
            return default;
        }
        
        Logger.Log($"Loaded asset: {assetId} for {endTime - startTime} seconds", Logger.LogSource.AddressablesLoader);
        
        return handle.Result;
    }
    
    public static void Unload(object obj)
    {
        if (obj == null)
        {
            return;
        }
        
        string objectType = obj.GetType().Name;
        var objectName = obj.ToString();

        Addressables.Release(obj);
        
        Logger.Log($"Unloaded asset: Type: {objectType}; Name: {objectName}", Logger.LogSource.AddressablesLoader);
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
        
        Logger.Log($"Unloaded instance of asset: Type: {goType}; Name: {goName}", Logger.LogSource.AddressablesLoader);
    }
}
