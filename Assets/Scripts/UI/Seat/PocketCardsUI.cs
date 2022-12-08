using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PocketCardsUI : MonoBehaviour
{
    [SerializeField] private int _position;
    
    [SerializeField] private Animator _animator;

    [SerializeField] private Image _cardImage1;
    [SerializeField] private Image _cardImage2;
    
    private static Game Game => Game.Instance;
    private static Betting Betting => Betting.Instance;

    private void OnEnable()
    {
        Game.GameStageChangedEvent += GameStageChangedEvent;
        Game.EndDealEvent += OnEndDeal;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
    }

    private void OnDisable()
    {
        Game.GameStageChangedEvent -= GameStageChangedEvent;
        Game.EndDealEvent -= OnEndDeal;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
    }

    private void GameStageChangedEvent(GameStage gameStage)
    {
        if (gameStage != GameStage.Preflop)
        {
            return;
        }

        Player player = PlayerSeats.Instance.Players[_position];
        if (player == null || player.IsOwner == true)
        {
            return;
        }
        
        _cardImage1.sprite = Resources.Load<Sprite>("Sprites/BlueCardBack");
        _cardImage2.sprite = Resources.Load<Sprite>("Sprites/BlueCardBack");
        
        _animator.ResetTrigger("ThrowCards");
        _animator.SetTrigger("GetCards");
    }

    private void OnEndDeal(WinnerData winnerData)
    {
        _animator.ResetTrigger("GetCards");
        _animator.SetTrigger("ThrowCards");
    }

    private void OnPlayerEndBetting(Player player, BetAction betAction)
    {
        if (betAction != BetAction.Fold || player.IsOwner == true)
        {
            return;
        }

        _animator.SetTrigger("Fold");
    }
}