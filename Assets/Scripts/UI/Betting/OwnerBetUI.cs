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

    private void OnEnable()
    {
        Game.GameStageChangedEvent += OnGameStageChanged;

        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.ToggleOnEvent += OnBetActionToggleOn;
        }
    }

    private void OnDisable()
    {
        Game.GameStageChangedEvent -= OnGameStageChanged;

        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.ToggleOnEvent -= OnBetActionToggleOn;
        }
    }
    
    private void OnGameStageChanged(GameStage gameStage)
    {
        _choosenBetAction = BetAction.Empty;
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        Player player = PlayerSeats.Players.First(x => x != null && x.IsOwner == true);
        BetSituation betSituation = Betting.GetBetSituation(player.BetAmount);

        if (Betting.CurrentBetter == player)
        {
            if (betSituation == BetSituation.CallEqualsOrLessCheck)
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
            if (betSituation == BetSituation.CallEqualsOrLessCheck)
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

    private void OnBetActionToggleOn(BetAction betAction)
    {
        _choosenBetAction = betAction;
        OnBetActionChangedEvent?.Invoke(betAction);
    }
}