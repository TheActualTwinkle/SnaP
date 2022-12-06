using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class OwnerPocketCardsUI : MonoBehaviour
{
    
    [SerializeField] private Animator _animator;

    [SerializeField] private Image _cardImage1;
    [SerializeField] private Image _cardImage2;
    
    private static Game Game => Game.Instance;
    private static Betting Betting => Betting.Instance;

    private void OnEnable()
    {
        Game.GameStageChangedEvent += GameStageChangedEvent;
        Game.EndDealEvent += OnEndDeal;
        Betting.BetActionEvent += OnBetAction;
    }

    private void OnDisable()
    {
        Game.GameStageChangedEvent -= GameStageChangedEvent;
        Game.EndDealEvent -= OnEndDeal; 
        Betting.BetActionEvent -= OnBetAction;
    }

    private void GameStageChangedEvent(GameStage gameStage)
    {
        if (gameStage != GameStage.Preflop)
        {
            return;
        }

        Player player = PlayerSeats.Instance.Players.FirstOrDefault(x => x != null && x.IsOwner == true);
        if (player == null)
        {
            return;
        }
        
        _cardImage1.sprite = Resources.Load<Sprite>($"Sprites/{(int)player.PocketCard1.Value + 2}_{player.PocketCard1.Suit.ToString()}");
        _cardImage2.sprite = Resources.Load<Sprite>($"Sprites/{(int)player.PocketCard2.Value + 2}_{player.PocketCard2.Suit.ToString()}");

        _animator.ResetTrigger("ThrowCards");
        _animator.SetTrigger("GetCards");
    }

    private void OnEndDeal(WinnerData winnerData)
    {
        _animator.ResetTrigger("GetCards");
        _animator.SetTrigger("ThrowCards");
    }

    private void OnBetAction(Player player, BetAction betAction)
    {
        if (betAction != BetAction.Fold || player.IsOwner == false)
        {
            return;
        }

        _animator.SetTrigger("Fold");
    }
}
