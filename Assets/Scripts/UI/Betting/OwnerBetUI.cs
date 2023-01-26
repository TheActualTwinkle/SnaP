using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OwnerBetUI : MonoBehaviour
{
    public static OwnerBetUI Instance { get; private set; }

    public event Action<BetAction> OnBetActionChangedEvent;

    [ReadOnly] [SerializeField] private BetAction _choosenBetAction;

    private List<BetActionToggle> _toggles;

    private static Game Game => Game.Instance; 
    private static Betting Betting => Betting.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _toggles = GetComponentsInChildren<BetActionToggle>().ToList();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            SetupToggles(); // todo
        }
    }

    private void OnEnable()
    {
        Game.GameStageBeganEvent += OnGameStageBegan;
        Game.EndDealEvent += OnEndDeal;
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
        Betting.PlayerStartBettingEvent += OnPlayerStartBetting;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;

        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.ToggleOnEvent += OnBetActionToggleOn;
        }
    }

    private void OnDisable()
    {
        Game.GameStageBeganEvent -= OnGameStageBegan;
        Game.EndDealEvent -= OnEndDeal;
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;
        Betting.PlayerStartBettingEvent -= OnPlayerStartBetting;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;

        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.ToggleOnEvent -= OnBetActionToggleOn;
        }
    }
    
    private void OnGameStageBegan(GameStage gameStage)
    {
        _choosenBetAction = BetAction.Empty;
        OnBetActionChangedEvent?.Invoke(_choosenBetAction);
    }

    private void OnEndDeal(WinnerInfo winnerInfo)
    {
        _choosenBetAction = BetAction.Empty;
        OnBetActionChangedEvent?.Invoke(_choosenBetAction);
    }
    
    private void OnBetActionToggleOn(BetAction betAction)
    {
        _choosenBetAction = betAction;
        OnBetActionChangedEvent?.Invoke(betAction);
    }

    private void OnPlayerStartBetting(Player player)
    {
        SetupToggles();
    }
    
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        SetupToggles();
    }
    
    private void OnPlayerLeave(Player player, int seatIndex)
    {
        ClearToggles();
    }
    
    private void SetupToggles() // todo НЕ обновлять тоглы когда выбран режим префаером 
    {
        Player player = PlayerSeats.Players.FirstOrDefault(x => x != null && x.IsOwner == true);
        if (player == null)
        {
            return;
        }
        
        BetSituation betSituation = Betting.GetBetSituation(player.BetAmount);

        if (Betting.CurrentBetter == player)
        {
            if (betSituation == BetSituation.CanCheck)
            {
                _toggles[0].SetToggleInfo(BetAction.Fold, "Fold");
                _toggles[1].SetToggleInfo(BetAction.Check, "Check");
                _toggles[2].SetToggleInfo(BetAction.Bet, "Bet");
            }
            else
            {
                _toggles[0].SetToggleInfo(BetAction.Fold, "Fold");
                _toggles[1].SetToggleInfo(BetAction.Call, "Call");
                _toggles[2].SetToggleInfo(BetAction.Raise, "Raise");
            }
        }
        else
        {
            if (betSituation == BetSituation.CanCheck)
            {
                _toggles[0].SetToggleInfo(BetAction.CheckFold, "Check/Fold");
                _toggles[1].SetToggleInfo(BetAction.Check, "Check");
                _toggles[2].SetToggleInfo(BetAction.CallAny, "Call Any");
            }
            else
            {
                _toggles[0].SetToggleInfo(BetAction.CheckFold, "Check/Fold");
                _toggles[1].SetToggleInfo(BetAction.Call, "Call");
                _toggles[2].SetToggleInfo(BetAction.CallAny, "Call Any");
            }
        }
    }

    private void ClearToggles()
    {
        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.SetToggleInfo(BetAction.Empty, string.Empty);
        }
    }
}