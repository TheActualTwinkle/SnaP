using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Player : NetworkBehaviour
{
    public string NickName => _nickName;
    [ReadOnly]
    [SerializeField] private string _nickName;

    [ReadOnly]
    [SerializeField] private List<CardObject> _pocketCards = new List<CardObject>(2);

    public uint Stack => _stack;
    [ReadOnly]
    [SerializeField] private uint _stack;

    public override void OnNetworkSpawn()
    {
        if (IsOwner == false)
        {
            Destroy(this);
            return;
        }
    }

    public bool TryBet(uint amount)
    {
        if (_stack < amount)
        {
            Debug.LogError($"{_nickName} has no chiphs");
            return false;
        }

        _stack -= amount;
        return true;
    }

    public void TakePot(uint amount)
    {
        _stack += amount;
    }
}
