using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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

    public Sprite Avatar => _avatar;
    [SerializeField] private Sprite _avatar;

    private void OnEnable()
    {
        PlayerSeatUI.Instance.PlayerClickJoinButton += OnPlayerClickJoinButton;
    }

    private void OnDisable()
    {
        PlayerSeatUI.Instance.PlayerClickJoinButton -= OnPlayerClickJoinButton;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner == false)
        {
            return;
        }

        PlayerData data = SaveLoadSystem.LoadPlayerData();
        _nickName = data.Name;
        _avatar = Resources.Load<Sprite>($"Sprites/{data.ImageID}");

        DontDestroyOnLoad(gameObject);
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

    [ServerRpc]
    private void ChangeSeatServerRpc()
    {

    }

    // Refactor mb?
    private void OnPlayerClickJoinButton(int seatNumber)
    {
        Debug.Log($"Player {OwnerClientId} sit on {seatNumber} seat. Is owner: {IsOwner}");

        PlayerSeatData data = new PlayerSeatData(this, seatNumber);

        if (IsOwner)
        {
            ChangeSeatServerRpc();
        }

        //PlayerSeats.(Instance).TryTake(data);
    }
}
