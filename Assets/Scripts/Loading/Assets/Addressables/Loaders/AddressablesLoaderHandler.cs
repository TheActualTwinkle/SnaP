using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

/// <summary>
/// Handler for loading assets via IAddressablesLoader.
/// </summary>
public class AddressablesLoaderHandler : ILoadingOperation
{
    public string Description => "Loading assets...";
    
    public async Task Load(Action<float> onProgress)
    {
        string sceneName = GetSceneName();
        List<IAddressablesLoader> loaders = AddressablesLoaderFactory.GetAllForScene(sceneName).ToList();

        long assetsCount = loaders.Sum(x => x.AssetsCount);
        long loadedCount = 0;
        
        foreach (IAddressablesLoader loader in loaders)
        {
            try
            {
                await loader.LoadContent();
            }
            catch (Exception)
            {
                // ignore
            }
            
            // Report to the loading operation.
            loadedCount = loaders.Sum(x => x.LoadedCount);
            onProgress?.Invoke(loadedCount/(float)assetsCount);
        }
        
        // TODO: https://github.com/TheActualTwinkle/SnaP/issues/42
        if (loadedCount < assetsCount)
        {
            while (true) await Task.Delay(1000);
        }
    }

    private string GetSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}