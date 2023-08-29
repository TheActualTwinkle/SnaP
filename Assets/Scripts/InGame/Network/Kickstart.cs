using UnityEngine;

public class Kickstart : MonoBehaviour
{
    [SerializeField] private NetworkConnectorType _connectorType;

    private async void Start()
    {
        await NetworkConnectorHandler.CreateGame(_connectorType);
    }
}
