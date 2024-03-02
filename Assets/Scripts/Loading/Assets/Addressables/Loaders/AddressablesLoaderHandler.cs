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
        
        foreach (IAddressablesLoader loader in loaders)
        {
            await loader.LoadContent();
            
            // Report to the loading operation.
            long loadedCount = loaders.Sum(x => x.LoadedCount);
            onProgress?.Invoke(loadedCount/(float)assetsCount);
        }
    }

    private string GetSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}