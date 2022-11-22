using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class BettingUI : MonoBehaviour
{
    public static BettingUI Instance { get; private set; }

    public BetAction ChoosenBetAction => _choosenBetAction;
    [ReadOnly]
    [SerializeField] private BetAction _choosenBetAction;

    private List<BetActionToggle> _toggles;

    private static Game Game => Game.Instance; 
    private static Betting Betting => Betting.Instance;
    
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

    private void OnGameStageChanged(GameStage gameStage)
    {
        _choosenBetAction = 0;
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (Betting.CurrentBetter == NetworkManager.Singleton.LocalClient.PlayerObject)
        {
            if (Betting.BetSituation == BetSituation.CallEqualsOrLessCheck)
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
            if (Betting.BetSituation == BetSituation.CallEqualsOrLessCheck)
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
    }
}