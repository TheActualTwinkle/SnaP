using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// Keeps only one instance of NetworkManager. 
/// </summary>
[RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
public class NetworkManagerSingleton : MonoBehaviour
{
    private static NetworkManagerSingleton Instance { get; set; }

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
}
