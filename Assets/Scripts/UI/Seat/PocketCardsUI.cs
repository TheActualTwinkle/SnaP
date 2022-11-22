using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PocketCardsUI : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    [ReadOnly] [SerializeField] private Image _cardImageFront1;
    [ReadOnly] [SerializeField] private Image _cardImageFront2;
    
    private static Game Game => Game.Instance;
    private static Betting Betting => Betting.Instance;
    private static PocketCards PocketCards => PocketCards.Instance;

    private void OnEnable()
    {
        //PocketCards.ReceiveCardsEvent += OnReceiveCards;
        Game.EndDealEvent += OnEndDeal;
        Betting.BetActionEvent += OnBetAction;
    }

    private void OnDisable()
    {
        //PocketCards.ReceiveCardsEvent -= OnReceiveCards;
        Game.EndDealEvent -= OnEndDeal;
        Betting.BetActionEvent -= OnBetAction;
    }

    private void OnReceiveCards()
    {
        //Sprite card1Sprite;
        //Sprite card2Sprite;
        // if (true) // pizda
        // {
        //     card1Sprite = PocketCards.Card1FrontSprite;
        //     card2Sprite = PocketCards.Card2FrontSprite;
        // }
        // else
        // {
        //     card1Sprite = PocketCards.Card1BackSprite;
        //     card2Sprite = PocketCards.Card2BackSprite;
        // }
        
        //_cardImageFront1.sprite = card1Sprite;
        //_cardImageFront2.sprite = card2Sprite;
        
        _animator.SetTrigger("GetCards");
    }

    private void OnEndDeal(WinnerData winnerData)
    {
        _animator.SetTrigger("ThrowCards");
    }

    private void OnBetAction(BetAction betAction)
    {
        if (betAction != BetAction.Fold)
        {
            return;
        }

        _animator.SetTrigger("Fold");
    }
}