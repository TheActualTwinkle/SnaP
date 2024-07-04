using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    public void LoadScene(string sceneToLoad, bool isNetworkSessionActive = true)
    {
        if (isNetworkSessionActive)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                LoadSceneNetwork(sceneToLoad);
            }
        }
        else
        {
            LoadSceneLocal(sceneToLoad);
        }
    }

    private void LoadSceneLocal(string sceneToLoad)
    {
        SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
    }

    private void LoadSceneNetwork(string sceneToLoad)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }
}
