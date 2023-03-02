using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuConncetion : MonoBehaviour
{
    [SerializeField] private TMP_InputField _ipAddressInputField;
    [SerializeField] private TMP_InputField _portInputField;

    private IEnumerator _connetctRoutine;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    // Button.
    private void StartHost()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        if (ushort.TryParse(_portInputField.text, out ushort port) == false)
        {
            return;
        }
        unityTransport.SetConnectionData(_ipAddressInputField.text, port);

        NetworkManager.Singleton.Shutdown();

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Desk", LoadSceneMode.Single);
    }

    // Button.
    private void Join()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        unityTransport.SetConnectionData(_ipAddressInputField.text, ushort.Parse(_portInputField.text));
        
        NetworkManager.Singleton.Shutdown();
        
        NetworkManager.Singleton.StartClient();
    }

    // Button.
    private void Exit()
    {
        Application.Quit();
    }
}
