using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _progressText;
    
    [Tooltip("Interval between fetching for loaded assets count")]
    [Range(0, 1000)]
    [SerializeField] private uint _pollingIntervalMs;
    
    [Tooltip("Delay before destroying this object after all assets loaded")]
    [Range(0, 2000)]
    [SerializeField] private uint _disposeDelayMs; 
    
    private static AddressablesLoaderHandler LoadingHandler => AddressablesLoaderHandler.Instance;

    private void Awake()
    {
        _backgroundImage.gameObject.SetActive(true);
        _slider.gameObject.SetActive(true);
        _progressText.gameObject.SetActive(true);
    }

    private async void Start()
    {
        if (LoadingHandler == null)
        {
            Logger.Log("LoadingHandler is null", Logger.LogLevel.Warning, Logger.LogSource.Addressables);
            Destroy(gameObject);
        }
        
        while (true)
        {
            Setup();

            if (LoadingHandler.LoadedAssetsCount == LoadingHandler.AssetsCount)
            {
                break;
            }
            
            await Task.Delay((int)_pollingIntervalMs);
        }

        await Task.Delay((int)_disposeDelayMs);

        
        Destroy(gameObject);
    }

    private void Setup()
    {
        uint assetsCount = LoadingHandler.AssetsCount;
        uint loadedAssetsCount = LoadingHandler.LoadedAssetsCount;

        _progressText.text = $"Loaded assets: {loadedAssetsCount}/{assetsCount}";
        
        _slider.maxValue = assetsCount;
        _slider.value = loadedAssetsCount;
    }
}
