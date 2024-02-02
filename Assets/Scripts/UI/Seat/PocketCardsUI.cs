using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PocketCardsUI : MonoBehaviour
{
    [SerializeField] private int _position;
    
    [SerializeField] private Animator _animator;

    [SerializeField] private Image _cardImage1;
    [SerializeField] private Image _cardImage2;
    private Sprite _cardSprite1;
    private Sprite _cardSprite2;

    private static readonly int GetCards = Animator.StringToHash("GetCards");
    private static readonly int ThrowCards = Animator.StringToHash("ThrowCards");
    private static readonly int Fold = Animator.StringToHash("Fold");
    private static readonly int OpenCards = Animator.StringToHash("OpenCards");

    private static Game Game => Game.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Betting Betting => Betting.Instance;
    
    private void OnEnable()
    {
        Game.GameStageBeganEvent += GameStageBeganEvent;
        Game.EndDealEvent += OnEndDeal;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
    }

    private void OnDisable()
    {
        Game.GameStageBeganEvent -= GameStageBeganEvent;
        Game.EndDealEvent -= OnEndDeal;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
    }

    private async void GameStageBeganEvent(GameStage gameStage)
    {
        Player player = PlayerSeats.Players[_position];
        if (player == null || player.IsOwner == true)
        {
            return;
        }
        
        switch (gameStage)
        {
            case GameStage.Preflop:
            {
                _cardImage1.sprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Cards.CardBack);
                _cardImage2.sprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Cards.CardBack);
        
                _animator.ResetAllTriggers();
                _animator.SetTrigger(GetCards);
                break;
            }
            case GameStage.Showdown:
            {
                await LoadFrontSpriteForCards(player);

                _animator.ResetAllTriggers();
                _animator.SetTrigger(OpenCards);
                break;
            }
            default:
            {
                if (Betting.IsAllIn == true)
                {
                    await LoadFrontSpriteForCards(player);

                    _animator.ResetAllTriggers();;
                    _animator.SetTrigger(OpenCards);
                }
                break;
            }
        }
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        _animator.ResetAllTriggers();
        _animator.SetTrigger(ThrowCards);
    }

    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        if (betActionInfo.Player.IsOwner == true)
        {
            return;
        }
        
        if (betActionInfo.BetAction != BetAction.Fold || PlayerSeats.Players.IndexOf(betActionInfo.Player) != _position)
        {
            return;
        }
        
        _animator.ResetAllTriggers();
        _animator.SetTrigger(Fold);
    }

    private async Task LoadFrontSpriteForCards(Player player)
    {
        while (ReferenceEquals(player.PocketCard1, null) == true || ReferenceEquals(player.PocketCard2, null) == true)
        {
            await Task.Yield();
        }

        _cardSprite1 = await AddressablesLoader.LoadAsync<Sprite>($"{(int)player.PocketCard1.Value}_{player.PocketCard1.Suit}");
        _cardSprite2 = await AddressablesLoader.LoadAsync<Sprite>($"{(int)player.PocketCard2.Value}_{player.PocketCard2.Suit}");
    }  
    
    // Animator
    private void OpenCard(int index)
    {
        switch (index)
        {
            case 0:
                _cardImage1.sprite = _cardSprite1;
                break;
            case 1:
                _cardImage2.sprite = _cardSprite2;
                break;
        }
    }
}