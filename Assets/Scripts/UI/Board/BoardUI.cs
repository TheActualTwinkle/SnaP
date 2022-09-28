using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(NetworkObject))]
public class BoardUI : NetworkBehaviour
{
    [SerializeField] private Game _game;
    [SerializeField] private Animator _animator;

    public override void OnNetworkSpawn()
    {
        Debug.Log("Board spawned");
        Debug.Log($"IsOwner: {IsOwner}, IsHost: {IsHost}, IsServer: {IsServer}, IsClient:{IsClient}");
    }

    private void OnEnable()
    {
        if (_game == null)
        {
            _game = FindObjectOfType<Game>();
        }
    }

    private void OnDisable()
    {
        if (_game == null)
        {
            return;
        }
    }

    // TODELETE!
    // Button.
    private void ActivateAniamtion(string name)
    {
        _animator.SetTrigger(name);
    }
}
