using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectionInputFields : MonoBehaviour
{
    public static ConnectionInputFields Instance { get; private set; }

    [SerializeField] private TMP_InputField _ipAddressInputField;
    [SerializeField] private TMP_InputField _portInputField;
    [SerializeField] private TMP_InputField _joinCodeInputField;
    
    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (_ipAddressInputField == null)
        {
            return;
        }
        
        _ipAddressInputField.text = (await ConnectionDataPresenter.GetLocalIpAddressAsync()).ToString();
    }

    public IReadOnlyList<string> GetConnectionData(NetworkConnectorType connectorType)
    {
        return connectorType switch
        {
            NetworkConnectorType.IpAddress => new[] { _ipAddressInputField.text, _portInputField.text },
            NetworkConnectorType.UnityRelay => new[] { _joinCodeInputField.text },
            _ => throw new ArgumentOutOfRangeException(nameof(connectorType), connectorType, null)
        };
    }
}
