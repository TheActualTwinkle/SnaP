using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI), typeof(Animator))]
public class WinnerText : MonoBehaviour
{
    [SerializeField] private int _index;
    [SerializeField] private Animator _animator;

    private static readonly int Show = Animator.StringToHash("Show");

    private static Game Game => Game.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    
    private void OnEnable()
    {
        Game.EndDealEvent += OnEndDeal;
    }

    private void OnDisable()
    {
        Game.EndDealEvent -= OnEndDeal;
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        List<Player> winners = PlayerSeats.Players.FindAll(player => player != null && winnerInfo.Select(info => info.WinnerId).Contains(player.OwnerClientId));

        foreach (Player winner in winners)
        {
            if (PlayerSeats.Players.IndexOf(winner) != _index)
            {
                continue;
            }

            ResetAllAnimatorTriggers();
            _animator.SetTrigger(Show);
            return;
        }
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
