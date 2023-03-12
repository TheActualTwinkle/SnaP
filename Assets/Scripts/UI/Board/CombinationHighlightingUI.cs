using System.Collections.Generic;
using UnityEngine;

public class CombinationHighlightingUI : MonoBehaviour
{
    [SerializeField] private List<CombinationCard> _combinationCards;
    private readonly List<CardObject> _cards = new();

    private static Game Game => Game.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Player LocalPlayer => PlayerSeats.Instance.LocalPlayer;
    
    private void OnEnable()
    {
        Game.EndDealEvent += OnEndDeal;
    }

    private void OnDisable()
    {
        Game.EndDealEvent += OnEndDeal;
    }
    
    public void Highlight(GameStage gameStage)
    {
        if (gameStage is not (GameStage.Flop or GameStage.Turn or GameStage.River))
        {
            return;
        }

        if (LocalPlayer == null || PlayerSeats.Players.Contains(LocalPlayer) == false)
        {
            return;
        }

        switch (gameStage)
        {
            case GameStage.Flop:
                _cards.Add(LocalPlayer.PocketCard1);
                _cards.Add(LocalPlayer.PocketCard2);

                _cards.Add(Game.BoardCards[0]);
                _cards.Add(Game.BoardCards[1]);
                _cards.Add(Game.BoardCards[2]);
                break;
            case GameStage.Turn:
                _cards.Add(Game.BoardCards[3]);
                break;
            default:
                _cards.Add(Game.BoardCards[4]);
                break;
        }

        foreach (CombinationCard combinationCard in _combinationCards)
        {
            combinationCard.DisableAnimation();
        }

        List<CardObject> mainCombination = CombinationСalculator.GetBestHand(new Hand(_cards)).GetMainCombination();

        for (var i = 0; i < _cards.Count; i++)
        {
            if (mainCombination.Contains(_cards[i]))
            {
                _combinationCards[i].EnableAnimation();
            }
        }
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        _cards.Clear();
        foreach (CombinationCard combinationCard in _combinationCards)
        {
            combinationCard.DisableAnimation();
        }
    }
}
