using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class BoardUI : MonoBehaviour, ICardsAssetUser
{
    [SerializeField] private CombinationHighlightingUI _combinationHighlighting;
    [SerializeField] private Animator _animator;
    [SerializeField] private List<Image> _cardImages;
    
    private Sprite _backSprite;
    private readonly List<Sprite> _cardSprites = new();
    
    private static readonly int StartPreflop = Animator.StringToHash("StartPreflop");
    private static readonly int StartFlop = Animator.StringToHash("StartFlop");
    private static readonly int StartTurn = Animator.StringToHash("StartTurn");
    private static readonly int StartRiver = Animator.StringToHash("StartRiver");
    private static readonly int EndDeal = Animator.StringToHash("EndDeal");

    private static Game Game => Game.Instance;
    
    // All cards sprites.
    private List<Sprite> _preloadedCardSprites = new();

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

    private async void Start()
    {
        _backSprite = await AddressablesAssetLoader.LoadAsync<Sprite>("CardBack2");

        foreach (Image cardImage in _cardImages)
        {
            cardImage.sprite = _backSprite;
        }

        if (Game.IsPlaying == false)
        {
            return;
        }

        for (var i = 1; i <= (int)Game.CurrentGameStage; i++)
        {
            OnGameStageBegan((GameStage)i);
        }
    }
    

    public void SetCardsSprites(IEnumerable<Sprite> sprites)
    {
        _preloadedCardSprites = sprites.ToList();
    }

    private async void OnGameStageBegan(GameStage gameStage)
    {
        switch (gameStage)
        {
            case GameStage.Preflop:
                _animator.SetTrigger(StartPreflop);
                break;
            case GameStage.Flop:
                await LoadCardsFrontSprites(0, 3);
                _animator.SetTrigger(StartFlop);
                break;
            case GameStage.Turn:
                await LoadCardsFrontSprites(3, 1);
                _animator.SetTrigger(StartTurn);
                break;
            case GameStage.River:
                await LoadCardsFrontSprites(4, 1);
                _animator.SetTrigger(StartRiver);
                break;
            case GameStage.Showdown:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameStage), gameStage, null);
        }
    }
    
    private void OnEndDeal(WinnerDto[] winnerInfo)
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
        SfxAudioPlayer.Instance.Play(type);
    }
    
    // Animator
    private void HighlightCards(GameStage gameStage)
    {
        _combinationHighlighting.Highlight(gameStage);
    }

    private async Task LoadCardsFrontSprites(int startIndex, int count)
    {
        if (startIndex == 0)
        {
            _cardSprites.Clear();
        }

        while (Game.BoardCards.Count < startIndex + count)
        {
            await Task.Yield();
        }
        
        List<CardObject> cards = Game.BoardCards.ToList();
        
        int endIndex = startIndex + count;
        for (int i = startIndex; i < endIndex; i++)
        {
            CardObject card = cards[i];
            
            var id = $"{(int)card.Value}_{card.Suit}";

            Sprite sprite = _preloadedCardSprites.Find(x => x.name == id);
            _cardSprites.Add(sprite);
        }
    }
    
    // Animator
    private void OpenCard(int index)
    {
        _cardImages[index].sprite = _cardSprites[index];
    }
}
