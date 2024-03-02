using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public const int NullSeatNumber = -1;

    public int SeatNumber => _seatNumber.Value;
    [SerializeField] private NetworkVariable<int> _seatNumber = new(NullSeatNumber);
    
    public string NickName => _nickName.Value.ToString();
    private readonly NetworkVariable<FixedString32Bytes> _nickName = new();

    public PlayerAvatarData AvatarData => _avatarData.Value;
    private readonly NetworkVariable<PlayerAvatarData> _avatarData = new();

    public BetAction BetAction => _selectedBetAction.Value;
    private readonly NetworkVariable<BetAction> _selectedBetAction = new();

    public NetworkVariable<uint> BetNetworkVariable => _betAmount;
    public uint BetAmount => _betAmount.Value;
    private readonly NetworkVariable<uint> _betAmount = new();

    public uint BetInputFieldValue => _betInputFieldValue.Value;
    private readonly NetworkVariable<uint> _betInputFieldValue = new();

    public NetworkVariable<uint> StackNetworkVariable => _stack;
    public uint Stack => _stack.Value;
    private readonly NetworkVariable<uint> _stack = new();

    public bool IsAvatarImageReady => _isAvatarImageReady.Value;
    private readonly NetworkVariable<bool> _isAvatarImageReady = new();
    
    public CardObject PocketCard1 => _pocketCard1.Value;
    public CardObject PocketCard2 => _pocketCard2.Value;
    
    private readonly NetworkVariable<CardObject> _pocketCard1 = new();
    private readonly NetworkVariable<CardObject> _pocketCard2 = new();

    public CardObject LocalPocketCard1 { get; private set; }
    public CardObject LocalPocketCard2 { get; private set; }

    private static Game Game => Game.Instance;
    private static Betting Betting => Betting.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static PlayerSeatsUI PlayerSeatsUI => PlayerSeatsUI.Instance;

    private float _lastSeatActionTime;
    private const float SeatActionCooldownSeconds = 2f;

    private void OnEnable()
    {
        Game.GameStageOverEvent += OnGameStageOver;
        Game.EndDealEvent += OnEndDeal;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
        OwnerBetUI.BetInputFieldValueChangedEvent += OnBetInputFieldValueChanged;
        _seatNumber.OnValueChanged += OnSeatNumberChanged;
        
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (PlayerSeatsUI == null)
        {
            PlayerSeatsUI.InstantiatedEvent += SubscribeToPlayerClickTakeButton;
        }
        else
        {
            SubscribeToPlayerClickTakeButton();
        }
    }

    private void OnDisable()
    {
        Game.GameStageOverEvent -= OnGameStageOver;
        Game.EndDealEvent -= OnEndDeal;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
        OwnerBetUI.BetInputFieldValueChangedEvent -= OnBetInputFieldValueChanged;
        _seatNumber.OnValueChanged -= OnSeatNumberChanged;
        
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        if (PlayerSeatsUI != null)
        {
            PlayerSeatsUI.PlayerClickTakeButtonEvent -= OnPlayerClickTakeSeatsButton;
        }
    }

    private void Start()
    {
        // Set data and UI of non-owner players.
        if (IsOwner == false && _seatNumber.Value != NullSeatNumber)
        {
            TakeSeat(_seatNumber.Value, true); // go to false todo
        }
    }

    private void Update()
    {
        if (IsOwner == false)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) == false)
        {
            return;
        }
        
        if (PlayerSeats.Players.Contains(this) == true || PlayerSeats.WaitingPlayers.Contains(this) == true)
        {
            if (CanPerformSeatAction() == false)
            {
                return;
            }
            
            SetSeatServerRpc(NullSeatNumber);

            LeaveSeat(PlayerSeats.SeatLeaveReason.UserInput);
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

    public override void OnNetworkSpawn()
    {
        if (IsOwner == false)
        {
            return;
        }

        SetIsImageReadyServerRpc(false);
        
        PlayerData playerData = ReadonlySaveLoadSystemFactory.Instance.Get().Load<PlayerData>();
        PlayerAvatarData avatarData = ReadonlySaveLoadSystemFactory.Instance.Get().Load<PlayerAvatarData>();
        
        try
        {
            SetPlayerDataServerRpc(playerData);
            StartCoroutine(SetAvatar(avatarData.CodedValue));
        }
        catch
        {
            playerData.SetDefaultValues();
            avatarData.SetDefaultValues();
            SetPlayerDataServerRpc(playerData);
            StartCoroutine(SetAvatar(avatarData.CodedValue));
        }
    }
    
    public void SetBetAction(BetAction betAction)
    {
        if (IsOwner == false)
        {
            return;
        }
        
        SetSelectedBetActionServerRpc(betAction);
    }

    public bool TryBet(uint value)
    {
        if (IsServer == false)
        {
            return false;
        }

        if (value > _stack.Value)
        {
            LeaveSeat(PlayerSeats.SeatLeaveReason.Kick);
            return false;
        }
        
        _stack.Value -= value;
        _betAmount.Value += value;
        return true;
    }

    public void SetLocalPocketCards(CardObject card1, CardObject card2, ulong playerId)
    {
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = new[] { playerId };
        SetLocalPocketCardsClientRpc(card1, card2, rpcParams);
    }

    private void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        SceneLoader.Instance.LoadScene(Constants.SceneNames.Menu, false);
    }

    private IEnumerator HostShutdown()
    {
        ShutdownClientRpc();

        yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count <= 1);

        Shutdown();
    }
    
    private void SubscribeToPlayerClickTakeButton()
    {
        PlayerSeatsUI.PlayerClickTakeButtonEvent += OnPlayerClickTakeSeatsButton;
    }

    private void OnClientDisconnected(ulong id)
    {
        if (id != 0 || OwnerClientId == 0)
        {
            return;
        }
        
        Shutdown();
    }
    
    private void OnBetInputFieldValueChanged(uint value)
    {
        if (IsOwner == false)
        {
            return;
        }
        
        SetBetInputFieldValueServerRpc(value);
    }
    
    // Set data to owner.
    private void OnPlayerClickTakeSeatsButton(int seatNumber)
    {
        if (IsOwner == false)
        {
            return;
        }
     
        if (CanPerformSeatAction() == false)
        {
            return;
        }
        
        SetSeatServerRpc(seatNumber);

        TakeSeat(seatNumber);
    }

    // Set data to NON owner players.
    private void OnSeatNumberChanged(int oldValue, int newValue)
    {
        if (IsOwner == true)
        {
            return;
        }

        if (newValue != NullSeatNumber)
        {
            TakeSeat(newValue);
        }
        else
        {
            LeaveSeat(PlayerSeats.SeatLeaveReason.UserInput);
        }
    }    
    
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        if (IsOwner == false)
        {
            return;
        }
        
        if (betActionInfo.BetAction != BetAction.Empty || betActionInfo.Player != this)
        {
            return;
        }
        
        SetSeatServerRpc(NullSeatNumber);

        LeaveSeat(PlayerSeats.SeatLeaveReason.Kick);
    }

    private void OnGameStageOver(GameStage gameStage)
    {
        if (IsOwner == true)
        {
            if (gameStage == GameStage.River || Betting.IsAllIn == true)
            {
                SetGlobalPocketCardsServerRpc(LocalPocketCard1, LocalPocketCard2);
            }
        }
        
        if (IsServer == false)
        {
            return;
        }

        _betAmount.Value = 0;
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        if (IsOwner == true)
        {
            SetSelectedBetActionServerRpc(BetAction.Empty);
        }
        
        if (IsServer == false)
        {
            return;
        }
        
        if (winnerInfo.Select(x => x.WinnerId).Contains(OwnerClientId) == true)
        {        
            WinnerInfo info = winnerInfo.FirstOrDefault(x => x.WinnerId == OwnerClientId);
            _stack.Value += info.Chips;
        }
        
        _betAmount.Value = 0;
    } 
    
    private void TakeSeat(int seatNumber, bool forceToSeat = false)
    {
        _lastSeatActionTime = Time.realtimeSinceStartup;
        PlayerSeats.TryTake(this, seatNumber, forceToSeat);
    }

    private void LeaveSeat(PlayerSeats.SeatLeaveReason leaveReason)
    {
        _lastSeatActionTime = Time.realtimeSinceStartup;
        PlayerSeats.TryLeave(this, leaveReason);
    }

    private IEnumerator SetAvatar(byte[] allBytes)
    {
        if (allBytes == null)
        {
            yield break;
        }
        
        ClearAvatarDataServerRpc();
        
        yield return new WaitUntil(() => _avatarData.Value.CodedValue.Length == 0);
        
        const int maxBytesPerRpc = PlayerAvatarData.MaxBytesPerRpc;
        int packageAmount = Mathf.CeilToInt((float)allBytes.Length / maxBytesPerRpc);

        List<byte[]> packages = new();
        for (var i = 0; i < packageAmount; i++)
        {
            int startIndex = i * maxBytesPerRpc;
            int length = Mathf.Min(maxBytesPerRpc, allBytes.Length - startIndex);
            
            var package = new byte[length];
            Array.Copy(allBytes, startIndex, package, 0, length);
            packages.Add(package);
        }
        
        for (var i = 0; i < packageAmount - 1; i++)
        {
            AppendAvatarDataServerRpc(packages[i]);
            yield return new WaitUntil(() => _avatarData.Value.CodedValue.Length == (i+1) * maxBytesPerRpc); // Wait for RPC to apply.
            Logger.Log($"Loading player avatar. {i+1}/{packageAmount} done.");
        }
        
        AppendAvatarDataServerRpc(packages[packageAmount - 1]);
        yield return new WaitUntil(() => _avatarData.Value.CodedValue.Length == allBytes.Length); // Wait for RPC to apply.
        
        Logger.Log("Player avatar loaded.");

        SetIsImageReadyServerRpc(true);
    }

    private bool CanPerformSeatAction()
    {
        if (Game.CanPerformSeatAction == true)
        {
            return false;
        }

        if (_lastSeatActionTime + SeatActionCooldownSeconds > Time.realtimeSinceStartup)
        {
            return false;
        }

        return true;
    }
    
    public override string ToString()
    {
        return $"Nick: '{_nickName.Value}', ID: '{OwnerClientId}'";
    }

    #region RPC

    [ServerRpc]
    private void SetGlobalPocketCardsServerRpc(CardObject card1, CardObject card2)
    {
        _pocketCard1.Value = card1;
        _pocketCard2.Value = card2;
    }
    
    [ServerRpc]
    private void SetSeatServerRpc(int seatNumber)
    {
        _seatNumber.Value = seatNumber;
    }

    [ServerRpc]
    private void SetPlayerDataServerRpc(PlayerData data)
    {            
        _nickName.Value = data.NickName;
        _stack.Value = data.Stack;
    }

    [ServerRpc]
    private void ClearAvatarDataServerRpc()
    {
        _avatarData.Value = new PlayerAvatarData(Array.Empty<byte>());
    }
    
    [ServerRpc]
    private void AppendAvatarDataServerRpc(byte[] data)
    {
        List<byte> allBytes = new(_avatarData.Value.CodedValue);
        allBytes.AddRange(data);

        _avatarData.Value = new PlayerAvatarData(allBytes.ToArray());
    }

    [ServerRpc]
    private void SetBetInputFieldValueServerRpc(uint value)
    {
        _betInputFieldValue.Value = value;
    }

    [ServerRpc]
    private void SetSelectedBetActionServerRpc(BetAction betAction)
    {
        _selectedBetAction.Value = betAction;
    }

    [ServerRpc]
    private void SetIsImageReadyServerRpc(bool value)
    {
        _isAvatarImageReady.Value = value;
    }

    [ClientRpc]
    private void SetLocalPocketCardsClientRpc(CardObject card1, CardObject card2, ClientRpcParams clientRpcParams = default)
    {
        LocalPocketCard1 = card1;
        LocalPocketCard2 = card2;
    }
    
    [ClientRpc]
    private void ShutdownClientRpc()
    {
        Shutdown();
    }
    
    #endregion
}