using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ConnectionDataUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private string _dataSeparator;
    
    private void Start()
    {
        IEnumerable<string> connectionData = NetworkConnectorHandler.CurrentConnector.ConnectionData;
        _text.text = string.Join(_dataSeparator, connectionData);
    }
}
