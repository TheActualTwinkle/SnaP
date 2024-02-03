using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(Image))]
public class BetChipsUI : MonoBehaviour
{
    [SerializeField] private int _index;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _betValueText;
    [SerializeField] private Animator _animator;

    [SerializeField] private float _delayToPotAnimation;

    private static readonly int Bet = Animator.StringToHash("Bet");
    private static readonly int ToPot = Animator.StringToHash("ToPot");

    private static Game Game => Game.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Betting Betting => Betting.Instance;
    
    private void OnEnable()
    {
        Game.EndDealEvent += OnEndDeal;
        Game.GameStageOverEvent += OnGameStageOver;
        PlayerSeats.PlayerSitEvent += OnPlayerSit;
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
    }

    private void OnDisable()
    {
        Game.EndDealEvent -= OnEndDeal;
        Game.GameStageOverEvent -= OnGameStageOver;
        PlayerSeats.PlayerSitEvent -= OnPlayerSit;
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;
    }

    private void OnGameStageOver(GameStage gameStage)
    {
        StartCoroutine(DelayToPotAnimation(_delayToPotAnimation));
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        StartCoroutine(DelayToPotAnimation(_delayToPotAnimation));
    }
    
    private void OnPlayerSit(Player player, int index)
    {
        if (index != _index)
        {
            return;
        }

        player.BetNetworkVariable.OnValueChanged += OnBetValueChanged;

        if (player.BetAmount <= 0)
        {
            return;
        }

        _betValueText.text = player.BetAmount.ToString();
        SetImage(player.BetAmount);
        
        _animator.ResetAllTriggers();
        _animator.SetTrigger(Bet);

        SfxAudio.Instance.Play(Constants.Sound.Sfx.Type.Bet);
    }

    private void OnPlayerLeave(Player player, int index)
    {
        if (index != _index)
        {
            return;
        }
        
        player.BetNetworkVariable.OnValueChanged -= OnBetValueChanged;
    }
    
    private void OnBetValueChanged(uint oldValue, uint newValue)
    {
        if (newValue <= 0)
        {
            return;
        }

        _betValueText.text = newValue.ToString();
        SetImage(newValue);
        
        _animator.ResetAllTriggers();
        _animator.SetTrigger(Bet);

        try
        {
            SfxAudio.Instance.Play(Constants.Sound.Sfx.Type.Bet);
        }
        catch (Exception)
        {
            Logger.Log("SfxAudio.Instance.Play(Constants.Sound.Sfx.Type.Bet) failed.", Logger.LogLevel.Error);
        }
    }
    
    private IEnumerator DelayToPotAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        _animator.ResetAllTriggers();
        _animator.SetTrigger(ToPot);
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

        _image.sprite = Resources.Load<Sprite>($"{Constants.ResourcesPaths.Chips}/ChipsStack_" + imageId);
        //_image.SetNativeSize(); todo: Fix chips stack size.
    }
}
