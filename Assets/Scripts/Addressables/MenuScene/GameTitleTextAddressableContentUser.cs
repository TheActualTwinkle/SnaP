using System.Threading.Tasks;
using UnityEngine;

public class GameTitleTextAddressableContentUser : MonoBehaviour, IAddressableContentUser
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 1;

    // Are we left of right?
    [Range(1, 2)] [SerializeField] private uint _part; 
    
    private GameObject _loadedGo;

    private async void Start()
    {
        await LoadContent();

        InstantiateContent();
    }

    private void InstantiateContent()
    {
        if (_loadedGo == null)
        {
            return;
        }
        
        Instantiate(_loadedGo, gameObject.transform);
        
        Logger.Log($"Instantiated asset: {_loadedGo}", Logger.LogSource.Addressables);
    }

    private void OnDestroy()
    {
        UnloadContent();
    }

    public async Task LoadContent()
    {
        _loadedGo = await AddressablesLoader.LoadAsync<GameObject>(Constants.Prefabs.GameTitleText + _part);
        LoadedCount = 1;
    }

    public void UnloadContent()
    {
        AddressablesLoader.UnloadInstance(_loadedGo);
        LoadedCount = 0;
    }
}
