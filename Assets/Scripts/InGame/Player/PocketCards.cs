using System;
using UnityEngine;

public class PocketCards : MonoBehaviour
{
    public static PocketCards Instance { get; private set; }

    [ReadOnly] [SerializeField] private CardObject _card1;
    [ReadOnly] [SerializeField] private CardObject _card2;
    
    private static Game Game => Game.Instance;
    
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
    }

    private void OnEnable()
    {
        Game.GameStageChangedEvent += OnGameStageChanged;
    }

    private void OnDisable()
    {
        Game.GameStageChangedEvent -= OnGameStageChanged;
    }
    
    private void OnGameStageChanged(GameStage gameStage)
    {
        if (gameStage != GameStage.Preflop)
        {
            return;
        }
        
        SetPocketCards(null, null);
    }

    private void SetPocketCards(CardObject card1, CardObject card2)
    {
        _card1 = card1;
        _card2 = card2;
    }
}