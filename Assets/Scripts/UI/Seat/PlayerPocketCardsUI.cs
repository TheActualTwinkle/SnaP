using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerPocketCardsUI : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private Game _game => Game.Instance;
    private PlayerBetting _playerBetting => PlayerBetting.Instance;

    private void OnEnable()
    {
        _game.GameStageChangedEvent += OnGameStageChanged;
        _game.EndDealEvent += OnEndDeal;
        _playerBetting.BetActionEvent += OnBetAction;
    }

    private void OnDisable()
    {
        _game.GameStageChangedEvent -= OnGameStageChanged;
        _game.EndDealEvent -= OnEndDeal;
        _playerBetting.BetActionEvent -= OnBetAction;
    }

    private void OnGameStageChanged(GameStage stage)
    {
        if (stage == GameStage.Preflop)
        {
            _animator.SetTrigger("GetCards");
        }
    }

    private void OnEndDeal(WinnerData winnerData)
    {
        _animator.SetTrigger("ThrowCards");
    }

    private void OnBetAction(BetAction betAction)
    {
        if (betAction == BetAction.Fold)
        {
            _animator.SetTrigger("Fold");
        }
    }
}