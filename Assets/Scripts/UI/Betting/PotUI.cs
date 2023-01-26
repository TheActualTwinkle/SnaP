using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotUI : MonoBehaviour
{
    [SerializeField] private Image _chipsImage;
    [SerializeField] private TextMeshProUGUI _valueText;
    
    private static Game Game => Game.Instance;
    private static Pot Pot => Pot.Instance;
    
    private void OnEnable()
    {
        Game.EndDealEvent += OnEndDeal;
        Game.GameStageOverEvent += OnGameStageOver;
    }

    private void OnDisable()
    {
        Game.EndDealEvent -= OnEndDeal;
        Game.GameStageOverEvent -= OnGameStageOver;
    }

    private void OnGameStageOver(GameStage gameStage)
    {
        List<int> values = Pot.Bets.Values.Select(x => (int)x).ToList();     
        
        _chipsImage.enabled = true;
        _valueText.text = values.Sum().ToString();
    }

    private void OnEndDeal(WinnerInfo winnerInfo)
    {
        _valueText.text = string.Empty;
        _chipsImage.enabled = false;
    }
}