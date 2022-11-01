using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConncetButtons : MonoBehaviour // Rename class
{
    [SerializeField] private TMP_InputField _ipAddressInputField;
    [SerializeField] private TMP_InputField _portInputField;

    // Button.
    private void StartHost()
    {
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        unityTransport.SetConnectionData(_ipAddressInputField.text, 12345); // ConnectionHandler.LocalArdess, (ushort)Random.Range(ushort.MinValue + 10000, ushort.MaxValue)

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Desk", LoadSceneMode.Single);
    }

    // Button.
    private void Join()
    {
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        unityTransport.SetConnectionData(_ipAddressInputField.text, ushort.Parse(_portInputField.text));

        NetworkManager.Singleton.StartClient();
    }

    // Button.
    private void Exit()
    {
        Application.Quit();
    }
}
