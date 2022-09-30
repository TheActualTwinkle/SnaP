using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(NetworkObject))]
public class BoardUI : NetworkBehaviour
{
    [SerializeField] private Game _game;
    [SerializeField] private Animator _animator;

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
}
