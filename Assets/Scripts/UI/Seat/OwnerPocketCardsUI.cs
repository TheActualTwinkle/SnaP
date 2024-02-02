using System.Collections;
using System.Threading.Tasks;
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

    private async void GameStageBeganEvent(GameStage gameStage)
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

        await ShowCards(player);
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        _animator.ResetAllTriggers();
        _animator.SetTrigger(ThrowCards);
    }

    private async void OnPlayerSit(Player player, int index)
    {
        if (player.IsOwner == false || PlayerSeats.Players.Contains(player) == false)
        {
            return;
        }

        if (Game.IsPlaying == false)
        {
            return;
        }
        
        await ShowCards(player);
    }

    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        if (betActionInfo.BetAction != BetAction.Fold || betActionInfo.Player.IsOwner == false)
        {
            return;
        }

        _animator.SetTrigger(Fold);
    }

    private async Task ShowCards(Player player)
    {
        while (ReferenceEquals(player.LocalPocketCard1, null) == true || ReferenceEquals(player.LocalPocketCard2, null) == true)
        {
            await Task.Yield();
        }

        await LoadSprites(player);
        
        _animator.ResetAllTriggers();
        _animator.SetTrigger(GetCards);
    }

    private async Task LoadSprites(Player player)
    {
        _cardImage1.sprite = await AddressablesLoader.LoadAsync<Sprite>($"{(int)player.LocalPocketCard1.Value}_{player.LocalPocketCard1.Suit.ToString()}");
        _cardImage2.sprite = await AddressablesLoader.LoadAsync<Sprite>($"{(int)player.LocalPocketCard2.Value}_{player.LocalPocketCard2.Suit.ToString()}");
    }
}
