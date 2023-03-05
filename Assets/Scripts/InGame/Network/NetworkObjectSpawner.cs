using Unity.Netcode;
using UnityEngine;

public static class NetworkObjectSpawner
{
    public static GameObject SpawnNetworkObjectChangeOwnershipToClient(GameObject prefab, Vector3 position, ulong newClientOwnerId, bool destroyWithScene)
    {
#if UNITY_EDITOR
        if (NetworkManager.Singleton.IsServer == false)
        {
            Debug.LogError("Spawning not happening in the server!");
        }
#endif
        GameObject newGameObject = Object.Instantiate(prefab, position, Quaternion.identity);

        NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
        newGameObjectNetworkObject.SpawnWithOwnership(newClientOwnerId, destroyWithScene);

        return newGameObject;
    }
}
