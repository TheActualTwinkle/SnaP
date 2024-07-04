using UnityEngine;

[RequireComponent(typeof(LobbyServiceConnectionResultUI))]
public class LobbyServiceConnectionResultAddressablesProvider : MonoBehaviour, IAddressablesProvider
{
    private LobbyServiceConnectionResultUI _lobbyServiceUI;

    private void Awake()
    {
        _lobbyServiceUI = GetComponent<LobbyServiceConnectionResultUI>();
    }

    public void Set()
    {
        LobbyServiceConnectionResultAddressablesLoader loader = AddressablesLoaderFactory.Get<LobbyServiceConnectionResultAddressablesLoader>();
        
        _lobbyServiceUI.SetSprites(loader.Sprites);
    }
}