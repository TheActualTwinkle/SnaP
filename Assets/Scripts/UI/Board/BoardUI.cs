using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class BoardUI : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Sprite _backSprite;
    [SerializeField] private List<Image> _cardImages;
    private readonly List<Sprite> _cardSprites = new();
    
    private static readonly int StartPreflop = Animator.StringToHash("StartPreflop");
    private static readonly int StartFlop = Animator.StringToHash("StartFlop");
    private static readonly int StartTurn = Animator.StringToHash("StartTurn");
    private static readonly int StartRiver = Animator.StringToHash("StartRiver");
    private static readonly int EndDeal = Animator.StringToHash("EndDeal");

    private static Game Game => Game.Instance;

    private void OnEnable()
    {
        Game.GameStageBeganEvent += OnGameStageBegan;
        Game.EndDealEvent += OnEndDeal;
    }

    private void OnDisable()
    {
        Game.GameStageBeganEvent -= OnGameStageBegan;
        Game.EndDealEvent -= OnEndDeal;
    }
    
    private void OnGameStageBegan(GameStage gameStage)
    {
        ResetAllTriggers();
        
        switch (gameStage)
        {
            case GameStage.Preflop:
            {
                LoadFrontSpriteForCards();
        
                _animator.SetTrigger(StartPreflop);
                break;
            } 
            case GameStage.Flop:
                _animator.SetTrigger(StartFlop);
                break;
            case GameStage.Turn:
                _animator.SetTrigger(StartTurn);
                break;
            case GameStage.River:
                _animator.SetTrigger(StartRiver);
                break;
            case GameStage.Showdown:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameStage), gameStage, null);
        }
    }
    
    private void OnEndDeal(WinnerInfo winnerInfo)
    {
        ResetAllTriggers();
        _animator.SetTrigger(EndDeal);

        foreach (Image image in _cardImages)
        {
            image.sprite = _backSprite;
        }
    }

    private void ResetAllTriggers()
    {
        _animator.ResetTrigger(EndDeal);
        _animator.ResetTrigger(StartPreflop);
        _animator.ResetTrigger(StartFlop);
        _animator.ResetTrigger(StartTurn);
        _animator.ResetTrigger(StartRiver);
    }

    private void LoadFrontSpriteForCards()
    {
        _cardSprites.Clear();
        for (var i = 0; i < _cardImages.Count; i++)
        {
            var id = $"Sprites/{(int)Game.BoardCards[i].Value}_{Game.BoardCards[i].Suit}";

            Sprite sprite = Resources.Load<Sprite>(id);
            _cardSprites.Add(sprite);
        }
    }
    
    // Animator
    private void OpenCard(int idnex)
    {
        _cardImages[idnex].sprite = _cardSprites[idnex];
    }
}
