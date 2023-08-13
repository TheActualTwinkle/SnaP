using UnityEngine;

public class Kickstart : MonoBehaviour
{
    [SerializeField] private NetworkConnectorType _connectorType;

    private void Start()
    {
        NetworkConnectorHandler.CreateGame(_connectorType);
    }
}
