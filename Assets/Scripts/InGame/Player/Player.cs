using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    private const int NullSeatNumber = -1;

    [SerializeField] private NetworkVariable<int> _seatNumber = new NetworkVariable<int>(NullSeatNumber);

    public string NickName => _nickName.Value.ToString();
    private NetworkVariable<FixedString32Bytes> _nickName = new NetworkVariable<FixedString32Bytes>();

    public Sprite Avatar { get; private set; }

    [ReadOnly]
    [SerializeField] private List<CardObject> _pocketCards = new List<CardObject>(2);

    public uint Stack => _stack;
    [ReadOnly]
    [SerializeField] private uint _stack;

    private PlayerSeats _playerSeats => PlayerSeats.Instance;
    private PlayerSeatsUI _playerSeatUI => PlayerSeatsUI.Instance;

    private void OnEnable()
    {
        _playerSeatUI.PlayerClickTakeButton += OnPlayerClickTakeButton;
        _seatNumber.OnValueChanged += OnSeatNumberCanged;
    }

    private void OnDisable()
    {
        _playerSeatUI.PlayerClickTakeButton -= OnPlayerClickTakeButton; 
        _seatNumber.OnValueChanged -= OnSeatNumberCanged;
    }

    private void Start()
    {
        // Set data and UI to non owner players.
        if (IsOwner == false && _seatNumber.Value != -1)
        {
            TakeSeat(_seatNumber.Value);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && IsOwner == true)
        {
            if (_playerSeats.Players.Contains(this) == true)
            {
                ChangeSeatServerRpc(NullSeatNumber);

                LeaveSeat();
            }

            else
            {
                if (IsServer)
                {
                    StartCoroutine(HostShutdown());
                }
                else
                {
                    Shutdown();
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner == false)
        {
            return;
        }

        PlayerData data = SaveLoadSystemFactory.Instance.Get().Load<PlayerData>();
        _nickName.Value = NickName;
        Avatar = null; // Refactor.

        CangePlayerNickNameServerRpc(data.NickName);

        Log.WriteLine($"Player ('{data.NickName}') connected to {ConnectionHandler.ConnectionFullAdress}.");
    }

    public bool TryBet(uint amount)
    {
        if (_stack < amount)
        {
            Log.WriteLine($"Player ('{_nickName}') is missing '{amount - _stack}' chiphs.");
            return false;
        }

        _stack -= amount;
        return true;
    }

    private void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        Application.Quit(); // Remove this when do next line.
        //NetworkManager.Singleton.SceneManager.LoadScene("Menu", LoadSceneMode.Single); Uncomment when create local scene transitions.
    }

    private IEnumerator HostShutdown()
    {
        ShutdownClientRpc();

        yield return new WaitForSeconds(0.5f);

        Shutdown();
    }

    // Set data to owner players.
    private void OnPlayerClickTakeButton(int seatNumber)
    {
        if (IsOwner == false)
        {
            return;
        }

        if (_playerSeats.IsFree(seatNumber) == true)
        {
            ChangeSeatServerRpc(seatNumber);

            TakeSeat(seatNumber);
        }
    }

    // Set data to NON owner players.
    private void OnSeatNumberCanged(int oldValue, int newValue)
    {
        if (IsOwner == false)
        {
            if (newValue != NullSeatNumber)
            {
                TakeSeat(newValue);
            }
            else
            {
                LeaveSeat();
            }
        }
    }

    private void TakeSeat(int seatNumber)
    {
        _playerSeats.TryTake(this, seatNumber);
    }

    private void LeaveSeat()
    {
        _playerSeats.TryLeave(this);
    }

    #region Rpc
    [ServerRpc]
    private void ChangeSeatServerRpc(int seatNumber)
    {
        _seatNumber.Value = seatNumber;
    }

    [ServerRpc]
    private void CangePlayerNickNameServerRpc(string nickName)
    {
        _nickName.Value = nickName;
    }

    [ClientRpc]
    private void ShutdownClientRpc()
    {
        if (IsServer)
        {
            return;
        }

        Shutdown();
    }
    #endregion
}