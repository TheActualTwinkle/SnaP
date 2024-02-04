using System.Threading.Tasks;
using UnityEngine;

public class GameTitleTextAddressablesLoader : MonoBehaviour, IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 1;

    // Are we left of right?
    [Range(1, 2)] [SerializeField] private uint _part; 
    
    private GameObject _loadedGo;

    private void OnDestroy()
    {
        UnloadContent();
    }

    public async Task LoadContent()
    {
        _loadedGo = await AddressablesLoader.LoadAsync<GameObject>(Constants.Prefabs.GameTitleText + _part);
        LoadedCount = 1;

        InstantiateContent();
    }

    private void InstantiateContent()
    {
        if (_loadedGo == null)
        {
            return;
        }
        
        Instantiate(_loadedGo, gameObject.transform);
        
        Logger.Log($"Instantiated asset: {_loadedGo}", Logger.LogSource.AddressablesLoader);
    }

    public void UnloadContent()
    {
        AddressablesLoader.UnloadInstance(_loadedGo);
        LoadedCount = 0;
    }
}
