using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BetTimerUI : MonoBehaviour
{
    [SerializeField] private int _position;
    [SerializeField] private Image _image;

    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Betting Betting => Betting.Instance;

    private void OnEnable()
    {
        Betting.PlayerStartBettingEvent += OnPlayerStartBetting;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
    }

    private void OnDisable()
    {
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
        StartCoroutine(StartTimer());
    }

    private void OnPlayerEndBetting(Player player, BetAction betAction)
    {
        if (_position != PlayerSeats.Players.IndexOf(player))
        {
            return;
        }
        
        _image.enabled = false;
    }
    
    private IEnumerator StartTimer()
    {
        _image.fillAmount = 1f;
        while (_image.fillAmount > 0)
        {
            _image.fillAmount -= Time.deltaTime / Betting.BetTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
