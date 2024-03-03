using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Handler for setting up assets via IAddressablesProvider.
/// </summary>
public class AddressablesProviderHandler : MonoBehaviour, ILoadingOperation
{
    // Used by Unity Addressables.
    [UsedImplicitly] public static string LoadTarget => GetLoadTarget();
    
    public string Description => "Setup assets...";
    
    private void Start()
    {
#if UNITY_SERVER
        Destroy(gameObject);
        return;
#endif
    }
    
    public Task Load(Action<float> onProgress)
    {
        onProgress?.Invoke(Constants.LoadingUI.FakeLoadStartValue);
        
        SetAssets();
        
        onProgress?.Invoke(1.0f);

        return Task.CompletedTask;
    }

    private void SetAssets()
    {
        IEnumerable<IAddressablesProvider> addressablesProviders = GetAllProviders();

        SetFor(addressablesProviders);
    }
    
    private IEnumerable<IAddressablesProvider> GetAllProviders()
    {
        // Take all objects with IAddressablesProvider component
        List<IAddressablesProvider> providers = new(FindObjectsOfType<MonoBehaviour>().OfType<IAddressablesProvider>());
        return providers;
    }

    private void SetFor(IEnumerable<IAddressablesProvider> providers)
    {
        foreach (IAddressablesProvider provider in providers)
        {
            provider.Set();
        }
    }

    /// <summary>
    /// Get load target for Addressables.
    /// </summary>
    /// <returns>Subdirectory in /ServerData/</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when platform is not supported by Addressables.</exception>
    private static string GetLoadTarget()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
                return "StandaloneWindows64";
            case RuntimePlatform.LinuxPlayer:
                return "StandaloneLinux64";
            case RuntimePlatform.OSXPlayer:
                return "StandaloneOSX";
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.WindowsEditor:
                return "StandaloneWindows64";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
