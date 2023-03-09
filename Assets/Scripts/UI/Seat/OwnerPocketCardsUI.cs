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

    private IEnumerator _showCardsCoroutine;

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

        if (_showCardsCoroutine != null)
        {
            StopCoroutine(_showCardsCoroutine);
        }

        _showCardsCoroutine = ShowCards(player);
        StartCoroutine(_showCardsCoroutine);
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        ResetAllAnimatorTriggers();
        _animator.SetTrigger(ThrowCards);
    }
    
    private void OnPlayerSit(Player player, int index)
    {
        if (player.IsOwner == false || PlayerSeats.Players.Contains(player) == false)
        {
            return;
        }

        if (Game.IsPlaying == false)
        {
            return;
        }
        
        if (_showCardsCoroutine != null)
        {
            StopCoroutine(_showCardsCoroutine);
        }
        
        _showCardsCoroutine = ShowCards(player);
        StartCoroutine(_showCardsCoroutine);
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

        ResetAllAnimatorTriggers();
        _animator.SetTrigger(GetCards);
    }
    
    private void ResetAllAnimatorTriggers()
    {
        foreach (AnimatorControllerParameter controllerParameter in _animator.parameters)
        {
            if (controllerParameter.type == AnimatorControllerParameterType.Trigger)
            {
                _animator.ResetTrigger(controllerParameter.name);
            }
        }
    }
}
