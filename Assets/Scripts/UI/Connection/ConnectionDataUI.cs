using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ConnectionDataUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    
    private async void Start()
    {
        if (NetworkConnectorHandler.Connector == null)
        {
            _text.text = "N/A";
            return;
        }
        
        _text.text = await ConnectionDataFactory.Get(NetworkConnectorHandler.Connector.Type);
    }
}
