using System.Collections;
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

    public string AvatarBase64String => _avatarBase64String.Value.ToString();
    private readonly NetworkVariable<FixedString4096Bytes> _avatarBase64String = new();

    public BetAction BetAction => _choosenBetAction.Value;
    private readonly NetworkVariable<BetAction> _choosenBetAction = new();

    public NetworkVariable<uint> BetNetworkVariable => _betAmount;
    public uint BetAmount => _betAmount.Value;
    private readonly NetworkVariable<uint> _betAmount = new();

    public uint BetInputFieldValue => _betInputFieldValue.Value;
    private readonly NetworkVariable<uint> _betInputFieldValue = new();

    public NetworkVariable<uint> StackNetworkVariable => _stack;
    public uint Stack => _stack.Value;
    private readonly NetworkVariable<uint> _stack = new();

    public CardObject PocketCard1 { get; private set; }
    public CardObject PocketCard2 { get; private set; }

    private static Game Game => Game.Instance;
    private static Betting Betting => Betting.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static PlayerSeatsUI PlayerSeatUI => PlayerSeatsUI.Instance;

    private void OnEnable()
    {
        Game.GameStageOverEvent += OnGameStageOver;
        Game.EndDealEvent += OnEndDeal;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
        OwnerBetUI.BetInputFieldValueChangedEvent += OnBetInputFieldValueChanged;
        PlayerSeatUI.PlayerClickTakeButton += OnPlayerClickTakeSeatButton;
        _seatNumber.OnValueChanged += OnSeatNumberChanged;
    }

    private void OnDisable()
    {
        Game.GameStageOverEvent -= OnGameStageOver;
        Game.EndDealEvent -= OnEndDeal;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
        OwnerBetUI.BetInputFieldValueChangedEvent -= OnBetInputFieldValueChanged;
        PlayerSeatUI.PlayerClickTakeButton -= OnPlayerClickTakeSeatButton; 
        _seatNumber.OnValueChanged -= OnSeatNumberChanged;
    }

    private void Start()
    {
        // Set data and UI to non owner players.
        if (IsOwner == false && _seatNumber.Value != NullSeatNumber)
        {
            TakeSeat(_seatNumber.Value, true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) == true && IsOwner == true)
        {
            print(_betAmount.Value);
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) == true && IsOwner == true)
        {
            if (PlayerSeats.Players.Contains(this) == true || PlayerSeats.WaitingPlayers.Contains(this) == true)
            {
                SetSeatServerRpc(NullSeatNumber);

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

        PlayerData playerData = SaveLoadSystemFactory.Instance.Get().Load<PlayerData>();
        SetPlayerDataServerRpc(playerData);
    }
    
    public void SetBetAction(BetAction betAction)
    {
        if (IsOwner == false)
        {
            return;
        }

        SetChoosenBetActionServerRpc(betAction);
    }

    public bool TryBet(uint value)
    {
        if (IsServer == false)
        {
            return false;
        }

        if (value > _stack.Value)
        {
            LeaveSeat();
            return false;
        }
        
        SetStackAmountClientRpc(_stack.Value - value);
        SetBetAmountClientRpc(_betAmount.Value + value);
        return true;
    }
    
    public void SetPocketCards(CardObject card1, CardObject card2)
    {
        PocketCard1 = card1;
        PocketCard2 = card2;
    }
    
    private void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        SceneLoader.Instance.LoadScene(SceneName.Menu, false);
    }

    private IEnumerator HostShutdown()
    {
        ShutdownClientRpc();

        yield return new WaitForSeconds(0.5f);

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
    
    // Set data to owner players.
    private void OnPlayerClickTakeSeatButton(int seatNumber)
    {
        if (IsOwner == false)
        {
            return;
        }

        if (PlayerSeats.IsFree(seatNumber) == false)
        {
            return;
        }

        if (_stack.Value < Betting.BigBlind)
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
            LeaveSeat();
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

        LeaveSeat();
    }

    private void OnGameStageOver(GameStage gameStage)
    {
        if (IsServer == false)
        {
            return;
        }
        
        SetBetAmountClientRpc(0);
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        if (IsOwner == true)
        {
            SetChoosenBetActionServerRpc(BetAction.Empty);
        }
        
        if (IsServer == false)
        {
            return;
        }

        if (winnerInfo.Select(x => x.WinnerId).Contains(OwnerClientId) == true)
        {        
            WinnerInfo info = winnerInfo.FirstOrDefault(x => x.WinnerId == OwnerClientId);
            SetStackAmountClientRpc(_stack.Value + info.Chips);
        }

        SetBetAmountClientRpc(0);
    } 
    
    private void TakeSeat(int seatNumber, bool forceToSeat = false)
    {
        PlayerSeats.TryTake(this, seatNumber, forceToSeat);
    }

    private void LeaveSeat()
    {
        PlayerSeats.TryLeave(this);
    }

    #region RPC
    
    [ServerRpc]
    private void SetSeatServerRpc(int seatNumber)
    {
        _seatNumber.Value = seatNumber;
    }

    [ServerRpc]
    private void SetPlayerDataServerRpc(PlayerData data)
    {
        _nickName.Value = data.NickName;
        _avatarBase64String.Value = data.AvatarBase64String;
        _stack.Value = data.Stack;
    }

    [ServerRpc]
    private void SetBetInputFieldValueServerRpc(uint value)
    {
        _betInputFieldValue.Value = value;
    }

    [ServerRpc]
    private void SetChoosenBetActionServerRpc(BetAction betAction)
    {
        _choosenBetAction.Value = betAction;
    }
    
    [ClientRpc]
    private void SetStackAmountClientRpc(uint value)
    {
        if (IsServer == false)
        {
            return;
        }
        
        _stack.Value = value;
    }
    
    [ClientRpc]
    private void SetBetAmountClientRpc(uint value)
    {
        if (IsServer == false)
        {
            return;
        }   
        
        _betAmount.Value = value;
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