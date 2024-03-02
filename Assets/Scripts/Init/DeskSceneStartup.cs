using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeskSceneStartup : MonoBehaviour
{
    [SerializeField] private AddressablesProviderHandler _addressablesProviderHandler;
    [SerializeField] private PlayerSpawner _playerSpawner;
    
    private void Start()
    {
#if UNITY_SERVER
        Destroy(gameObject);
        return;
#endif
        
        Queue<ILoadingOperation> loadingOperations = new();
        
        // Load assets.
        loadingOperations.Enqueue(new AddressablesLoaderHandler());
        
        // Setup assets.
        loadingOperations.Enqueue(_addressablesProviderHandler);    
        
        loadingOperations.Enqueue(_playerSpawner);    
        
        LoadingUI.Instance.Load(loadingOperations);
    }

    private void OnDestroy()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        IEnumerable<IAddressablesLoader> loaders = AddressablesLoaderFactory.GetExclusiveForScene(sceneName);
        
        foreach (IAddressablesLoader loader in loaders)
        {
            loader.UnloadContent();
        }
    }
}