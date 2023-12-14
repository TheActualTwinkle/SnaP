using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class BoardUI : MonoBehaviour
{
    [SerializeField] private CombinationHighlightingUI _combinationHighlighting;
    [SerializeField] private Animator _animator;
    [SerializeField] private List<Image> _cardImages;
    [SerializeField] private Sprite _backSprite;
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

    private void Start()
    {
        if (Game.IsPlaying == false)
        {
            return;
        }

        for (var i = 1; i <= (int)Game.CurrentGameStage; i++)
        {
            OnGameStageBegan((GameStage)i);
        }
    }

    private void OnGameStageBegan(GameStage gameStage)
    {
        switch (gameStage)
        {
            case GameStage.Preflop:
                _animator.SetTrigger(StartPreflop);
                break;
            case GameStage.Flop:
                StartCoroutine(LoadCardsFrontSprites(0, 3));
                _animator.SetTrigger(StartFlop);
                break;
            case GameStage.Turn:
                StartCoroutine(LoadCardsFrontSprites(3, 1));
                _animator.SetTrigger(StartTurn);
                break;
            case GameStage.River:
                StartCoroutine(LoadCardsFrontSprites(4, 1));
                _animator.SetTrigger(StartRiver);
                break;
            case GameStage.Showdown:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameStage), gameStage, null);
        }
    }
    
    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        _animator.ResetAllTriggers();
        _animator.SetTrigger(EndDeal);
        
        foreach (Image image in _cardImages)
        {
            image.sprite = _backSprite;
        }
    }

    // Animator
    private void PlaySound(Constants.Sound.Sfx.Type type)
    {
        SfxAudio.Instance.Play(type);
    }
    
    // Animator
    private void HighlightCards(GameStage gameStage)
    {
        _combinationHighlighting.Highlight(gameStage);
    }

    private IEnumerator LoadCardsFrontSprites(int startIndex, int count)
    {
        if (startIndex == 0)
        {
            _cardSprites.Clear();
        }
        
        yield return new WaitWhile(() => Game.BoardCards.Count < startIndex + count);
        
        List<CardObject> cards = Game.BoardCards.ToList();
        
        int endIndex = startIndex + count;
        for (int i = startIndex; i < endIndex; i++)
        {
            CardObject card = cards[i];
            
            var id = $"{Constants.ResourcesPaths.Cards}/{(int)card.Value}_{card.Suit}";

            Sprite sprite = Resources.Load<Sprite>(id);
            _cardSprites.Add(sprite);
        }
    }
    
    // Animator
    private void OpenCard(int index)
    {
        _cardImages[index].sprite = _cardSprites[index];
    }
}
