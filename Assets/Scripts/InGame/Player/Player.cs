using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
public class Player : NetworkBehaviour
{
    public NetworkVariable<int> SeatNumber => _seatNumber;
    [SerializeField] private NetworkVariable<int> _seatNumber = new NetworkVariable<int>(-1);

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
        _seatNumber.OnValueChanged += OnSeatNumberCanged;
    }

    private void OnDisable()
    {
        PlayerSeatUI.Instance.PlayerClickJoinButton -= OnPlayerClickJoinButton;
        _seatNumber.OnValueChanged -= OnSeatNumberCanged;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner == false)
        {
            return;
        }

        PlayerData data = SaveLoadSystem.LoadPlayerData();
        _nickName = data?.NickName;
        _avatar = Resources.Load<Sprite>($"Sprites/{data?.ImageID}"); // Add image from window and after save it on persistentPath

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
    private void ChangeSeatServerRpc(int seatNumber)
    {
        _seatNumber.Value = seatNumber;
    }

    private void OnSeatNumberCanged(int oldValue, int newValue)
    {
        if (IsOwner == false && _seatNumber.Value != -1)
        {
                   
        }
    }

    private void OnPlayerClickJoinButton(int seatNumber)
    {
        if (IsOwner)
        {
            if (PlayerSeats.Instance.Players.Contains(this) == true)
            {
                PlayerSeats.Instance.Leave(this);
            }

            ChangeSeatServerRpc(seatNumber);

            PlayerSeats.Instance.TryTake(this);
        }
    }
}
