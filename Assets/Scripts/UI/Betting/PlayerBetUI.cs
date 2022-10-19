using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerBetUI : MonoBehaviour
{
    [SerializeField] private Game _game;

    public float BetTime => _betTime;
    [SerializeField] private float _betTime;

    [ReadOnly]
    [SerializeField] private List<BetButton> _buttons;

    public IEnumerator C_WaitForPlayerBet =>  c_WaitForPlayerBet;
    private IEnumerator c_WaitForPlayerBet;

    private void OnValidate()
    {
        _buttons = GetComponentsInChildren<BetButton>(true).ToList();
    }

    private void OnEnable()
    {
        _game.PlayerTurnBegunEvent += OnPlayerTurnBegun;
        foreach (var button in _buttons)
        {
            button.OnClickEvent += OnPlayerTurnOver;
        }
    }

    private void OnDisable()
    {
        _game.PlayerTurnBegunEvent -= OnPlayerTurnBegun;
        foreach (var button in _buttons)
        {
            button.OnClickEvent -= OnPlayerTurnOver;
        }
    }

    private void OnPlayerTurnBegun(Player player)
    {
        SetupButtons();
        if (c_WaitForPlayerBet != null)
        {
            StopCoroutine(c_WaitForPlayerBet);
        }
        c_WaitForPlayerBet = WaitForPlayerBetAction();
        StartCoroutine(c_WaitForPlayerBet);
    }

    private void OnPlayerTurnOver(BetButtonActions action)
    {
        if (c_WaitForPlayerBet != null)
        {
            StopCoroutine(c_WaitForPlayerBet);
        }
    }

    private void SetupButtons()
    {

    }

    private IEnumerator WaitForPlayerBetAction()
    {
        yield return new WaitForSecondsRealtime(_betTime);   
    }
}
