using UnityEngine;

/// <summary>
/// Connect buttons handler.
/// </summary>
public class ConnectionButton : MonoBehaviour
{
    [SerializeField] private NetworkConnectorType _connectorType;
    
    // Button.
    public void CreateGame()
    {
        NetworkConnectorHandler.CreateGame(_connectorType);
    }
    
    // Button.
    public void JoinGame()
    {
        NetworkConnectorHandler.JoinGame(_connectorType);
    }
}
