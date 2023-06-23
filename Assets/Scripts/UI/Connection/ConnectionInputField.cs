using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;

public class ConnectionInputField : MonoBehaviour
{
    public static ConnectionInputField Instance { get; private set; }

    [SerializeField] private TMP_InputField _ipAddressInputField;
    [SerializeField] private TMP_InputField _portInputField;
    [SerializeField] private TMP_InputField _joinCodeInputField;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _ipAddressInputField.text = Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }

    public IReadOnlyList<string> GetConnectionData(NetworkConnectorType connectorType)
    {
        return connectorType switch
        {
            NetworkConnectorType.LocalAddress => new[] { _ipAddressInputField.text, _portInputField.text },
            NetworkConnectorType.UnityRelay => new[] { _joinCodeInputField.text },
            _ => throw new ArgumentOutOfRangeException(nameof(connectorType), connectorType, null)
        };
    }
}
