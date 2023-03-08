using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class OwnerBetUI : MonoBehaviour
{
    public static event Action<uint> BetInputFieldValueChangedEvent;

    private BetAction ChosenBetAction => GetChosenBetAction();
    
    private List<BetActionToggle> _toggles;
    
    private static Player LocalPlayer => PlayerSeats.LocalPlayer;

    private static Game Game => Game.Instance; 
    private static Betting Betting => Betting.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;

    [SerializeField] private TMP_InputField _betInputField;
    [SerializeField] private TextMeshProUGUI _callText;
    
    private Animator _animator;
    private static readonly int BetTogglePointerEnter = Animator.StringToHash("BetTogglePointerEnter");
    private static readonly int BetTogglePointerExit = Animator.StringToHash("BetTogglePointerExit");
    private static readonly int CallTogglePointerEnter = Animator.StringToHash("CallTogglePointerEnter");
    private static readonly int CallTogglePointerExit = Animator.StringToHash("CallTogglePointerExit");
    private static readonly int Hide = Animator.StringToHash("Hide");
    private static readonly int Show = Animator.StringToHash("Show");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        
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

        _betInputField.onEndEdit.AddListener(OnBetInputFieldValueChanged);
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
        
        _betInputField.onEndEdit.AddListener(OnBetInputFieldValueChanged);
    }

    #region UnityEvents

    // Uinty Event
    private void OnBetTogglePointerEnter()
    {
        if (LocalPlayer == null)
        {
            return;
        }
        
        if (Betting.CurrentBetter != LocalPlayer)
        {
            return;
        }
        
        ClampBetToggleValue();
        BetInputFieldValueChangedEvent?.Invoke(uint.Parse(_betInputField.text));

        _animator.ResetTrigger(BetTogglePointerExit);
        _animator.SetTrigger(BetTogglePointerEnter);
    }

    // Uinty Event
    private void OnBetTogglePointerExit()
    {
        _animator.ResetTrigger(BetTogglePointerEnter);
        _animator.SetTrigger(BetTogglePointerExit);
    }

    // Uinty Event
    private void OnCallTogglePointerEnter()
    {
        if (PlayerSeats.LocalPlayer == null)
        {
            return;
        }
        
        if (Betting.GetBetSituation(LocalPlayer.BetAmount) != BetSituation.CallOrFold)
        {
            return;
        }

        if (Betting.CallAmount > LocalPlayer.Stack + LocalPlayer.BetAmount)
        {
            _callText.text = $"{LocalPlayer.Stack + LocalPlayer.BetAmount} (+{LocalPlayer.Stack})";
        }
        else
        {
            _callText.text = $"{Betting.CallAmount} (+{Betting.CallAmount - LocalPlayer.BetAmount})";
        }
        
        _animator.ResetTrigger(CallTogglePointerExit);
        _animator.SetTrigger(CallTogglePointerEnter);
    }

    // Uinty Event
    private void OnCallTogglePointerExit()
    {
        _animator.ResetTrigger(CallTogglePointerEnter);
        _animator.SetTrigger(CallTogglePointerExit);
    }

    #endregion
    
    private void OnToggleValueChanged(bool value)
    {
        if (Betting.CurrentBetter != LocalPlayer)
        {
            return;
        }

        if (value == true)
        {
            DisableToggles();
        }

        if (LocalPlayer != null) 
        {
            LocalPlayer.SetBetAction(ChosenBetAction);
        }
    }

    private void OnBetInputFieldValueChanged(string value)
    {
        ClampBetToggleValue();
        BetInputFieldValueChangedEvent?.Invoke(uint.Parse(_betInputField.text));
    }

    private void OnGameStageBegan(GameStage gameStage)
    {
        if (LocalPlayer == null)
        {
            return;
        }
        
        if (PlayerSeats.Players.Contains(LocalPlayer) == false || gameStage != GameStage.Preflop)
        {
            return;
        }
        
        _toggles[2].gameObject.SetActive(true);
    }
    
    private void OnGameStageOver(GameStage gameStage)
    {
        DisableToggles();

        if (Betting.IsAllIn == true)
        {
            HideToggles();
            return;
        }
        
        if (gameStage != GameStage.River)
        {
            PushBackToggles();
        }
    }
    
    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        DisableToggles();
        PushBackToggles();
        HideToggles();
    }
    
    private void OnPlayerStartBetting(Player player)
    {
        if (player.IsOwner == true)
        {
            if (player.Stack == 0)
            {
                return;
            }

            BetAction betAction = ChosenBetAction;
            
            if (betAction is BetAction.Cancel or BetAction.Empty)
            {
                PushBackToggles();
            }
            
            if (LocalPlayer != null)
            {
                LocalPlayer.SetBetAction(betAction);
            }
        }

        if (LocalPlayer == null || PlayerSeats.Players.Contains(LocalPlayer) == false)
        {
            return;
        }
        
        if (LocalPlayer.BetAction is BetAction.AllIn or BetAction.Fold)
        {
            return;
        }
        
        ShowToggles();
        EnableToggles();        
        SetupTogglesUI();
    }
    
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        if (betActionInfo.Player.IsOwner == true)
        {
            _betInputField.text = string.Empty;
            
            _animator.SetTrigger(CallTogglePointerExit);
            _animator.SetTrigger(BetTogglePointerExit);
            
            if (betActionInfo.BetAction is BetAction.Fold or BetAction.AllIn)
            {
                if (LocalPlayer != null && betActionInfo.BetAction == BetAction.AllIn)
                {
                    LocalPlayer.SetBetAction(betActionInfo.BetAction);
                }
                DisableToggles();
                HideToggles();
                return;
            }
            
            EnableToggles();
            PushBackToggles();
            
            if (LocalPlayer != null)
            {
                SetupTogglesUI();
                LocalPlayer.SetBetAction(BetAction.Empty);
            }
        }
        else
        {
            if (betActionInfo.BetAction == BetAction.AllIn)
            {        
                SetupTogglesUI();
                return;
            }
            
            BetAction betAction = ChosenBetAction;

            if (betAction == BetAction.Check && LocalPlayer.BetAmount < betActionInfo.BetAmount)
            {
                PushBackToggles();
            }

            if (betAction is not (BetAction.Empty or BetAction.Cancel))
            {
                DisableToggles();
            }
        }
    }
    
    private void OnPlayerLeave(Player player, int index)
    {
        if (player.IsOwner == false)
        {
            return;
        }
        
        DisableToggles();
        HideToggles();
    }

    private BetAction GetChosenBetAction()
    {
        if (_toggles.Count(x => x.Toggle.isOn == true) == 0)
        {
            return BetAction.Empty;
        }

        BetAction betAction = _toggles.First(x => x.Toggle.isOn == true).BetAction;

        if (betAction == BetAction.CheckFold)
        {
            betAction = Betting.GetBetSituation(LocalPlayer.BetAmount) == BetSituation.CanCheck ? BetAction.Check : BetAction.Fold;
        }

        if (betAction == BetAction.CallAny)
        {
            betAction = Betting.CallAmount <= LocalPlayer.Stack + LocalPlayer.BetAmount ? BetAction.Call : BetAction.Cancel;
        }

        if (betAction == BetAction.Check)
        {
            betAction = Betting.GetBetSituation(LocalPlayer.BetAmount) == BetSituation.CanCheck ? BetAction.Check : BetAction.Cancel;
        }
        
        switch (betAction)
        {
            case BetAction.Raise or BetAction.Bet when _betInputField.text == LocalPlayer.Stack.ToString() || _betInputField.text == Betting.GetAllInBetAmount(LocalPlayer).ToString():
            case BetAction.Call when Betting.CallAmount >= LocalPlayer.Stack + LocalPlayer.BetAmount || Betting.CallAmount == Betting.GetAllInBetAmount(LocalPlayer):
                return BetAction.AllIn;
            default:
                return betAction;
        }
    }

    private void SetupTogglesUI()
    {
        Player player = LocalPlayer;
        if (player == null)
        {
            return;
        }

        if (Betting.IsAllIn == true)
        {
            _toggles[0].SetToggleInfo(BetAction.Fold, "Fold");
            _toggles[1].SetToggleInfo(BetAction.AllIn, "All In");
            _toggles[2].SetToggleInfo(BetAction.Empty, string.Empty);
            _toggles[2].gameObject.SetActive(false);

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

    private void ClampBetToggleValue()
    {
        uint minBetValue = Betting.BigBlind >= Betting.CallAmount ? Betting.BigBlind : Betting.CallAmount;
        uint maxBetValue = LocalPlayer.Stack >= Betting.GetAllInBetAmount(LocalPlayer) ? Betting.GetAllInBetAmount(LocalPlayer) : LocalPlayer.Stack;
     
        if (uint.TryParse(_betInputField.text, out uint value) == false)
        {
            value = minBetValue;
        }
        
        if (value + LocalPlayer.BetAmount < minBetValue) // Set min.
        {
            value = minBetValue - LocalPlayer.BetAmount;
        }
        
        if (value > maxBetValue) // Set max.
        {
            value = maxBetValue;
        }

        _betInputField.text = value.ToString();
    }

    private void ShowToggles()
    {
        _animator.ResetTrigger(Hide);
        _animator.SetTrigger(Show);
    }
    
    private void HideToggles()
    {
        _animator.ResetTrigger(Show);
        _animator.SetTrigger(Hide);
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