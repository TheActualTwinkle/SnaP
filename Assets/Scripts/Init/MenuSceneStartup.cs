using System.Collections.Generic;
using UnityEngine;

public class MenuSceneStartup : MonoBehaviour
{
    [SerializeField] private AddressablesProviderHandler _addressablesProviderHandler;
    
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
        
        LoadingUI.Instance.Load(loadingOperations);
    }
}
