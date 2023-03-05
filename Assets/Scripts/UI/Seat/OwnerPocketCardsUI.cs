using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class OwnerPocketCardsUI : MonoBehaviour
{
    
    [SerializeField] private Animator _animator;

    [SerializeField] private Image _cardImage1;
    [SerializeField] private Image _cardImage2;
    
    private static readonly int ThrowCards = Animator.StringToHash("ThrowCards");
    private static readonly int GetCards = Animator.StringToHash("GetCards");
    private static readonly int Fold = Animator.StringToHash("Fold");

    private static Game Game => Game.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Betting Betting => Betting.Instance;

    private void OnEnable()
    {
        Game.GameStageBeganEvent += GameStageBeganEvent;
        Game.EndDealEvent += OnEndDeal;
        PlayerSeats.PlayerSitEvent += OnPlayerSit;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
    }

    private void OnDisable()
    {
        Game.GameStageBeganEvent -= GameStageBeganEvent;
        Game.EndDealEvent -= OnEndDeal; 
        PlayerSeats.PlayerSitEvent -= OnPlayerSit;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
    }

    private void GameStageBeganEvent(GameStage gameStage)
    {
        if (gameStage != GameStage.Preflop)
        {
            return;
        }

        Player player = PlayerSeats.LocalPlayer;
        if (player == null || PlayerSeats.Players.Contains(player) == false)
        {
            return;
        }

        StartCoroutine(ShowCards(player));
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        ResetAllTriggers();
        _animator.SetTrigger(ThrowCards);
    }
    
    private void OnPlayerSit(Player player, int index)
    {
        if (player.IsOwner == false || PlayerSeats.Players.Contains(player) == false)
        {
            return;
        }
    
        StartCoroutine(ShowCards(player));
    }
    
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        if (betActionInfo.BetAction != BetAction.Fold || betActionInfo.Player.IsOwner == false)
        {
            return;
        }

        _animator.SetTrigger(Fold);
    }

    private IEnumerator ShowCards(Player player)
    {
        yield return new WaitUntil(() => ReferenceEquals(player.PocketCard1, null) == false && ReferenceEquals(player.PocketCard2, null) == false);
        
        _cardImage1.sprite = Resources.Load<Sprite>($"Sprites/{(int)player.PocketCard1.Value}_{player.PocketCard1.Suit.ToString()}");
        _cardImage2.sprite = Resources.Load<Sprite>($"Sprites/{(int)player.PocketCard2.Value}_{player.PocketCard2.Suit.ToString()}");

        ResetAllTriggers();
        _animator.SetTrigger(GetCards);
    }
    
    private void ResetAllTriggers()
    {
        _animator.ResetTrigger(GetCards);
        _animator.ResetTrigger(ThrowCards);
        _animator.ResetTrigger(Fold);
    }
}
