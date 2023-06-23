using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class UISpawner : MonoBehaviour
{
    [SerializeField] private UI _ui;
    
    private void Awake()
    {
        if (NetworkManager.Singleton.IsClient == false)
        {
            return;
        }
        
        _ui.gameObject.SetActive(true);
    }
}
