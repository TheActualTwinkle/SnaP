using UnityEngine;

/// <summary>
/// Connect buttons handler.
/// </summary>
public class ConnectionButton : MonoBehaviour
{
    [SerializeField] private NetworkConnectorType _connectorType;

    // Button.
    private void CreateGame()
    {
        NetworkConnectorHandler.CreateGame(_connectorType);
    }

    // Button.
    private void JoinGame()
    {
        NetworkConnectorHandler.JoinGame(_connectorType);
    }
}
