using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OwnerBetUI : MonoBehaviour
{
    private BetAction ChoosenBetAction => GetChoosenBetAction();
    
    private List<BetActionToggle> _toggles;
    
    private static Player LocalPlayer => PlayerSeats.LocalPlayer;

    private static Game Game => Game.Instance; 
    private static Betting Betting => Betting.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;

    private void Awake()
    {
        _toggles = GetComponentsInChildren<BetActionToggle>().ToList();
        DisableToggles();
    }

    private void OnEnable()
    {
        Game.GameStageBeganEvent += OnGameStageBegan;
        Game.GameStageOverEvent += OnGameStageOver;
        Game.EndDealEvent += OnEndDeal;
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
        Betting.PlayerStartBettingEvent += OnPlayerStartBetting;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;

        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnDisable()
    {
        Game.GameStageBeganEvent -= OnGameStageBegan;
        Game.GameStageOverEvent -= OnGameStageOver;
        Game.EndDealEvent -= OnEndDeal;
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;
        Betting.PlayerStartBettingEvent -= OnPlayerStartBetting;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
        
        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool value)
    {
        if (value == true && Betting.CurrentBetter == LocalPlayer)
        {
            DisableToggles();
        }
        
        LocalPlayer.ChangeBetAction(ChoosenBetAction);
    }
    
    private void OnGameStageBegan(GameStage gameStage)
    { 
        EnableToggles();
        PushBackToggles();
    }

    private void OnGameStageOver(GameStage gameStage)
    {
        DisableToggles();
        PushBackToggles();
    }
    
    private void OnEndDeal(WinnerInfo winnerInfo)
    {
        DisableToggles();
        PushBackToggles();
        ClearToggles();
    }
    
    private void OnPlayerStartBetting(Player player)
    {
        SetupToggles();
    }
    
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        if (betActionInfo.Player.IsOwner == true)
        {
            EnableToggles();
            
            if (betActionInfo.BetAction == BetAction.Fold)
            {
                ClearToggles();
                return;
            }

            PushBackToggles();
        }
        
        SetupToggles();
    }
    
    private void OnPlayerLeave(Player player, int seatIndex)
    {
        if (player.IsOwner == false)
        {
            return;
        }
        
        ClearToggles();
    }

    private BetAction GetChoosenBetAction()
    {
        if (_toggles.Count(x => x.Toggle.isOn == true) == 0)
        {
            return BetAction.Empty;
        }

        return _toggles.First(x => x.Toggle.isOn == true).BetAction;
    }
    
    private void SetupToggles() // todo НЕ обновлять тоглы когда выбран режим префаером 
    {
        Player player = LocalPlayer;
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

    private void PushBackToggles()
    {
        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.Toggle.isOn = false;
        }
    }

    private void EnableToggles()
    {
        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.Toggle.enabled = true;
        }
    }

    private void DisableToggles()
    {
        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.Toggle.enabled = false;
        }
    }
}