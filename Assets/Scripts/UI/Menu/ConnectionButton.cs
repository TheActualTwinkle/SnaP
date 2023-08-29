using UnityEngine;

/// <summary>
/// Connect buttons handler.
/// </summary>
public class ConnectionButton : MonoBehaviour
{
    [SerializeField] private NetworkConnectorType _connectorType;

    // Button.
    private async void CreateGame()
    {
        await NetworkConnectorHandler.CreateGame(_connectorType);
    }

    // Button.
    private async void JoinGame()
    {
        await NetworkConnectorHandler.JoinGame(_connectorType);
    }
}
