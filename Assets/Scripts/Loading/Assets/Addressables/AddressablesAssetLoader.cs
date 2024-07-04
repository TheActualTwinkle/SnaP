using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Class for loading assets via Unity Addressables.
/// </summary>
public static class AddressablesAssetLoader
{
    /// <summary>
    /// Load asset from Addressables.
    /// Use only for assets that are not scene objects.
    /// </summary>
    /// <param name="assetId">Asset id.</param>
    /// <typeparam name="T">Type of asset.</typeparam>
    /// <returns>Loaded asset.</returns>
    /// <exception cref="FileLoadException">Thrown when asset can`t be loaded.</exception>
    public static async Task<T> LoadAsync<T>(string assetId)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetId);
        await handle.Task;
        
        stopwatch.Stop();

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Logger.Log($"Object of type {typeof(T)} with id {assetId} can`t be loaded via Addressables", Logger.LogLevel.Error, Logger.LogSource.Addressables);
            throw new FileLoadException();
        }

        Logger.Log($"Loaded asset: {assetId} for {stopwatch.Elapsed.TotalSeconds} seconds", Logger.LogSource.AddressablesLoader);

        return handle.Result;
    }
    
    /// <summary>
    /// Unload asset (object) from memory.
    /// </summary>
    /// <param name="obj">Object to unload.</param>
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
    
    /// <summary>
    /// Unload instance of asset from memory.
    /// </summary>
    /// <param name="go">Instance of asset (GameObject) to unload.</param>
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
