using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBetUI : MonoBehaviour
{
    public static PlayerBetUI Instance { get; private set; }

    public event Action<BetAction> BetActionEvent;

    public float BetTime => _betTime;
    [SerializeField] private float _betTime;

    [ReadOnly]
    [SerializeField] private List<Button> _buttons;

    public IEnumerator C_WaitForPlayerBet => c_WaitForPlayerBet;
    private IEnumerator c_WaitForPlayerBet;

    private Game _game => Game.Instance;

    private void OnEnable()
    {
        _game.PlayerTurnBegunEvent += OnPlayerTurnBegun;
    }

    private void OnDisable()
    {
        _game.PlayerTurnBegunEvent -= OnPlayerTurnBegun;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnPlayerTurnBegun(Player player)
    {
        SetupButtons();
        
        if (c_WaitForPlayerBet != null)
        {
            StopCoroutine(c_WaitForPlayerBet);
        }
        c_WaitForPlayerBet = MakeBet();
        StartCoroutine(c_WaitForPlayerBet);
    }

    private void SetupButtons() // ToDo. mb create class to set the actions?
    {

    }

    // Button.
    private void MakeBetAction(BetAction action)
    {
        if (c_WaitForPlayerBet != null)
        {
            StopCoroutine(c_WaitForPlayerBet);
        }
        BetActionEvent?.Invoke(action);
    }

    private IEnumerator MakeBet()
    {
        yield return new WaitForSecondsRealtime(_betTime);
        // exit with no action
    }
}
