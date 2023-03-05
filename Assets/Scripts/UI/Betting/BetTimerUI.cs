using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BetTimerUI : MonoBehaviour
{
    [SerializeField] private int _position;
    [SerializeField] private Image _image;

    private IEnumerator _startTimerCoroutine;
    
    private static Game Game => Game.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Betting Betting => Betting.Instance;

    private void OnEnable()
    {
        Game.EndDealEvent += OnEndDeal;
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
        Betting.PlayerStartBettingEvent += OnPlayerStartBetting;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
    }

    private void OnDisable()
    {
        Game.EndDealEvent -= OnEndDeal;
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;
        Betting.PlayerStartBettingEvent -= OnPlayerStartBetting;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
    }

    private void OnPlayerStartBetting(Player player)
    {
        if (_position != PlayerSeats.Players.IndexOf(player))
        {
            return;
        }

        _image.enabled = true;

        if (_startTimerCoroutine != null)
        {
            StopCoroutine(_startTimerCoroutine);
        }
        _startTimerCoroutine = StartTimer();
        StartCoroutine(_startTimerCoroutine);
    }
    
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        int playerIndex = PlayerSeats.Players.IndexOf(betActionInfo.Player);
        if (_position != playerIndex || playerIndex == -1)
        {
            return;
        }

        if (_startTimerCoroutine != null)
        {
            StopCoroutine(_startTimerCoroutine);
        }
        
        _image.enabled = false;
    }

    private void OnPlayerLeave(Player player, int index)
    {
        if (_position != index || index == -1)
        {
            return;
        }
        
        if (_startTimerCoroutine != null)
        {
            StopCoroutine(_startTimerCoroutine);
        }
        
        _image.enabled = false;
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        _image.enabled = false;
    }
    
    private IEnumerator StartTimer()
    {
        _image.fillAmount = 1 - (Betting.TimePassedSinceBetStart / Betting.BetTime);
        while (Betting.TimePassedSinceBetStart < Betting.BetTime)
        {
            _image.fillAmount -= Time.deltaTime / Betting.BetTime;
            yield return new WaitForEndOfFrame();
        }
    }
}