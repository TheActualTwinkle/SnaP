using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private const int NullSeatNumber = -1;

    [SerializeField] private NetworkVariable<int> _seatNumber = new(NullSeatNumber);
    
    public string NickName => _nickName.Value.ToString();
    private readonly NetworkVariable<FixedString32Bytes> _nickName = new();

    public string AvatarBase64String => _avatarBase64String.Value.ToString();
    private readonly NetworkVariable<FixedString4096Bytes> _avatarBase64String = new();

    public BetAction ChoosenBetAction => _choosenBetAction.Value;
    private readonly NetworkVariable<BetAction> _choosenBetAction = new();

    public uint BetAmount => _betAmount.Value;
    private readonly NetworkVariable<uint> _betAmount = new();
    
    public uint Stack => _stack;
    [ReadOnly] [SerializeField] private uint _stack;

    public CardObject PocketCard1 => _pocketCard1;
    [ReadOnly] [SerializeField] private CardObject _pocketCard1;

    public CardObject PocketCard2 => _pocketCard2;
    [ReadOnly] [SerializeField] private CardObject _pocketCard2;
    
    private static Game Game => Game.Instance;
    private static Betting Betting => Betting.Instance;
    private static OwnerBetUI OwnerBetUI => OwnerBetUI.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static PlayerSeatsUI PlayerSeatUI => PlayerSeatsUI.Instance;

    private void OnEnable()
    {
        Game.GameStageOverEvent += OnGameStageOver;
        Game.EndDealEvent += OnEndDeal;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
        OwnerBetUI.OnBetActionChangedEvent += OnBetActionChanged;
        PlayerSeatUI.PlayerClickTakeButton += OnPlayerClickTakeSeatButton;
        _seatNumber.OnValueChanged += OnSeatNumberChanged;
    }

    private void OnDisable()
    {
        Game.GameStageOverEvent -= OnGameStageOver;
        Game.EndDealEvent -= OnEndDeal;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
        OwnerBetUI.OnBetActionChangedEvent -= OnBetActionChanged;
        PlayerSeatUI.PlayerClickTakeButton -= OnPlayerClickTakeSeatButton; 
        _seatNumber.OnValueChanged -= OnSeatNumberChanged;
    }

    private void Start()
    {
        // Set data and UI to non owner players.
        if (IsOwner == false && _seatNumber.Value != NullSeatNumber)
        {
            TakeSeat(_seatNumber.Value);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true && IsOwner == true)
        {
            if (PlayerSeats.Players.Contains(this) == true || PlayerSeats.WaitingPlayers.Contains(this) == true)
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

        PlayerData playerData = SaveLoadSystemFactory.Instance.Get().Load<PlayerData>();
        CangePlayerDataServerRpc(playerData.NickName, playerData.AvatarBase64String);
    }

    public bool TryBet(uint value)
    {
        if (IsOwner == false)
        {
            return false;
        }

        if (value > _stack)
        {            
            LeaveSeat();
            return false;
        }
        
        ChangeBetAmountServerRpc(value);
        return true;
    }
    
    public void SetPocketCards(CardObject card1, CardObject card2)
    {
        _pocketCard1 = card1;
        _pocketCard2 = card2;
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

        ChangeSeatServerRpc(seatNumber);

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
        
        ChangeSeatServerRpc(NullSeatNumber);

        LeaveSeat();
    }

    private void OnBetActionChanged(BetAction betAction)
    {
        if (IsOwner == false)
        {
            return;
        }
        
        ChangeChoosenBetActionServerRpc(betAction);
    }

    private void OnGameStageOver(GameStage gameStage)
    {
        if (IsOwner == false)
        {
            return;
        }
        
        ChangeChoosenBetActionServerRpc(BetAction.Empty);
        ChangeBetAmountServerRpc(0);
    }

    private void OnEndDeal(WinnerInfo winnerInfo)
    {
        if (winnerInfo.WinnerId != OwnerClientId)
        {
            return;
        }

        _stack += winnerInfo.Chips;
    } 
    
    private void TakeSeat(int seatNumber)
    {
        PlayerSeats.TryTake(this, seatNumber);
    }

    private void LeaveSeat()
    {
        PlayerSeats.TryLeave(this);
    }

    #region RPC
    
    [ServerRpc]
    private void ChangeSeatServerRpc(int seatNumber)
    {
        _seatNumber.Value = seatNumber;
    }

    [ServerRpc]
    private void CangePlayerDataServerRpc(string nickName, string avatarBase64String)
    {
        _nickName.Value = nickName;
        _avatarBase64String.Value = avatarBase64String;
    }

    [ServerRpc]
    private void ChangeBetAmountServerRpc(uint value)
    {
        _betAmount.Value = value;
    }

    [ServerRpc]
    private void ChangeChoosenBetActionServerRpc(BetAction betAction)
    {
        _choosenBetAction.Value = betAction;
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