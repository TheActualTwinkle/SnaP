using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(Image), typeof(TextMeshProUGUI))]
public class BetChipsUI : MonoBehaviour
{
    [SerializeField] private int _index;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _betValueText;
    [SerializeField] private Animator _animator;
    
    private static readonly int Bet = Animator.StringToHash("Bet");
    private static readonly int ToPot = Animator.StringToHash("ToPot");

    private static Game Game => Game.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Betting Betting => Betting.Instance;
    
    private void OnEnable()
    {
        Game.GameStageBeganEvent += OnGameStageBegan;
        Game.GameStageOverEvent += OnGameStageOver;
        Game.EndDealEvent += OnEndDeal;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
    }

    private void OnDisable()
    {
        Game.GameStageBeganEvent -= OnGameStageBegan;
        Game.GameStageOverEvent -= OnGameStageOver;
        Game.EndDealEvent -= OnEndDeal;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
    }

    private void OnEndDeal(WinnerInfo winnerInfo)
    {
        ResetAllAnimatorTriggers();
        _animator.SetTrigger(ToPot);
    }

    private void OnGameStageBegan(GameStage gameStage)
    {
        if (gameStage != GameStage.Preflop)
        {
            return;
        }

        List<int> turnSequence = BoardButton.TurnSequensce;

        if (_index != turnSequence[0] && _index != turnSequence[1])
        {
            return;
        }

        uint betValue = _index == turnSequence[0] ? Betting.SmallBlind : Betting.BigBlind;
            
        _betValueText.text = betValue.ToString();
        SetImage(betValue);
        
        ResetAllAnimatorTriggers();
        _animator.SetTrigger(Bet);
    }

    private void OnGameStageOver(GameStage gameStage)
    {
        ResetAllAnimatorTriggers();
        _animator.SetTrigger(ToPot);
    }
    
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        if (PlayerSeats.Players.IndexOf(betActionInfo.Player) != _index)
        {
            return;
        }
        
        if (betActionInfo.BetAction is BetAction.Empty or BetAction.Fold or BetAction.Check or BetAction.CheckFold or (BetAction)(-1))
        {
            return;
        }

        if (betActionInfo.BetAmount <= 0)
        {
            return;
        }

        _betValueText.text = betActionInfo.BetAmount.ToString();
        SetImage(betActionInfo.BetAmount);
        
        ResetAllAnimatorTriggers();
        _animator.SetTrigger(Bet);
    }

    private void SetImage(uint betValue)
    {
        uint smallBlindValue = Betting.SmallBlind;

        uint imageId;
        
        if (betValue >= 100 * smallBlindValue)
        {
            imageId = 7;
        }
        else if (betValue >= 50 * smallBlindValue)
        {
            imageId = 6;
        }
        else if (betValue >= 25 * smallBlindValue)
        {
            imageId = 6;
        }
        else if (betValue >= 10 * smallBlindValue)
        {
            imageId = 4;
        }
        else if (betValue >= 5 * smallBlindValue)
        {
            imageId = 3;
        }
        else if (betValue >= 2 * smallBlindValue)
        {
            imageId = 2;
        }
        else
        {
            imageId = 1;
        }
        
        _image.sprite = Resources.Load<Sprite>("Sprites/ChipsStack_" + imageId);
        _image.SetNativeSize();
    }

    private void ResetAllAnimatorTriggers()
    {
        _animator.ResetTrigger(Bet);
        _animator.ResetTrigger(ToPot);
    }
}
