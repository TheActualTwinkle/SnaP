using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class OwnerBetUI : MonoBehaviour
{
    public static event Action<uint> BetInputFieldValueChangedEvent;

    private BetAction ChoosenBetAction => GetChoosenBetAction();
    
    private List<BetActionToggle> _toggles;
    
    private static Player LocalPlayer => PlayerSeats.LocalPlayer;

    private static Game Game => Game.Instance; 
    private static Betting Betting => Betting.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;

    [SerializeField] private TMP_InputField _betInputField;
    
    private Animator _animator;
    private static readonly int BetTogglePointerEnter = Animator.StringToHash("BetTogglePointerEnter");
    private static readonly int BetTogglePointerExit = Animator.StringToHash("BetTogglePointerExit");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        
        _toggles = GetComponentsInChildren<BetActionToggle>().ToList();
        DisableToggles();
    }

    private void OnEnable()
    {
        Game.GameStageOverEvent += OnGameStageOver;
        Game.EndDealEvent += OnEndDeal;
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
        Betting.PlayerStartBettingEvent += OnPlayerStartBetting;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;

        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        _betInputField.onValueChanged.AddListener(OnBetInputFieldValueChanged);
    }

    private void OnDisable()
    {
        Game.GameStageOverEvent -= OnGameStageOver;
        Game.EndDealEvent -= OnEndDeal;
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;
        Betting.PlayerStartBettingEvent -= OnPlayerStartBetting;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
        
        foreach (BetActionToggle toggle in _toggles)
        {
            toggle.Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
        
        _betInputField.onValueChanged.AddListener(OnBetInputFieldValueChanged);
    }

    private void OnToggleValueChanged(bool value)
    {
        if (value == true && Betting.CurrentBetter == LocalPlayer)
        {
            DisableToggles();
        }

        LocalPlayer.SetBetAction(ChoosenBetAction);
    }

    // Uinty Event
    private void OnBetTogglePointerEnter()
    {
        if (PlayerSeats.LocalPlayer == null)
        {
            return;
        }
        
        if (Betting.CurrentBetter != PlayerSeats.LocalPlayer)
        {
            return;
        }
        
        _animator.ResetTrigger(BetTogglePointerExit);
        _animator.SetTrigger(BetTogglePointerEnter);
    }

    // Uinty Event
    private void OnBetTogglePointerExit()
    {
        _animator.ResetTrigger(BetTogglePointerEnter);
        _animator.SetTrigger(BetTogglePointerExit);
    }

    private void OnBetInputFieldValueChanged(string value)
    {
        uint betValue = uint.Parse(value);
        if (betValue > PlayerSeats.LocalPlayer.Stack)
        {
            betValue = PlayerSeats.LocalPlayer.Stack;
        }
        
        _betInputField.text = betValue.ToString();
        BetInputFieldValueChangedEvent?.Invoke(betValue);
    }

    private void OnGameStageOver(GameStage gameStage)
    {
        DisableToggles();

        if (gameStage == GameStage.River)
        {
            return;
        }
        
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
        if (player.IsOwner == true)
        {
            PushBackToggles();
        }

        EnableToggles();
        SetupToggles();
    }
    
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        if (betActionInfo.Player.IsOwner == true)
        {
            if (betActionInfo.BetAction == BetAction.Fold)
            {
                ClearToggles();
                return;
            }
            
            EnableToggles();
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