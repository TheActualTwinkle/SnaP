using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkObject))]
public class Player : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<int> _seatNumber = new NetworkVariable<int>(-1);

    public string NickName => _nickName.Value.ToString();
    private NetworkVariable<FixedString64Bytes> _nickName = new NetworkVariable<FixedString64Bytes>();

    [ReadOnly]
    [SerializeField] private List<CardObject> _pocketCards = new List<CardObject>(2);

    public uint Stack => _stack;
    [ReadOnly]
    [SerializeField] private uint _stack;

    public Sprite Avatar => _avatar;
    [ReadOnly]
    [SerializeField] private Sprite _avatar;

    private void OnEnable()
    {
        PlayerSeatUI.Instance.PlayerClickTakeButton += OnPlayerClickTakeButton;
        _seatNumber.OnValueChanged += OnSeatNumberCanged;
    }

    private void OnDisable()
    {
        PlayerSeatUI.Instance.PlayerClickTakeButton -= OnPlayerClickTakeButton;
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

    public override void OnNetworkSpawn()
    {
        if (IsOwner == false)
        {
            return;
        }

        PlayerData? data = SaveLoadSystem.LoadPlayerData();
        _avatar = Resources.Load<Sprite>($"Sprites/{data?.ImageID}"); // Change somehow :)
        InitializePlayerNickNameServerRpc(data?.NickName);

        Debug.Log($"Player '{data?.NickName}' is spawned");
    }

    public bool TryBet(uint amount)
    {
        if (_stack < amount)
        {
            Debug.LogError($"{_nickName} is missing {amount - _stack} chiphs");
            return false;
        }

        _stack -= amount;
        return true;
    }

    private IEnumerator HostShutdown()
    {
        ShutdownClientRpc();

        yield return new WaitForSeconds(0.5f);

        Shutdown();
    }

    private void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        Application.Quit(); // Remove this line when do line above.
        //NetworkManager.Singleton.SceneManager.LoadScene("Menu", LoadSceneMode.Single); Uncomment when create local scene transitions.
    }

    private void OnPlayerClickTakeButton(int seatNumber)
    {
        if (IsOwner == false)
        {
            return;
        }

        if (PlayerSeats.Instance.IsFree(seatNumber) == true)
        {
            ChangeSeatServerRpc(seatNumber);

            TakeSeat(seatNumber);
        }
    }

    // Set data to non owner players.
    private void OnSeatNumberCanged(int oldValue, int newValue)
    {
        if (IsOwner == false && _seatNumber.Value != -1)
        {
            TakeSeat(newValue);
        }
    }

    private void TakeSeat(int seatNumber)
    {
        PlayerSeats.Instance.Take(this, seatNumber);
    }

    [ServerRpc]
    private void ChangeSeatServerRpc(int seatNumber)
    {
        _seatNumber.Value = seatNumber;
    }

    [ServerRpc]
    private void InitializePlayerNickNameServerRpc(string nickName)
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
}