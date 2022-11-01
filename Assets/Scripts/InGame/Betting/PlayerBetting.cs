using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerBetting : MonoBehaviour
{
    public static PlayerBetting Instance { get; private set; }

    public event Action<BetAction> BetActionEvent;

    public IEnumerator C_StartBetCountdown { get; private set; }

    [SerializeField] private float _betTime;
    private float _timePaased = 0f;

    private Game _game => Game.Instance;
    private PlayerBettingUI _bettingUI => PlayerBettingUI.Instance;

    private void OnEnable()
    {
        _game.GameStageChangedEvent += OnGameStageChanged;
    }

    private void OnDisable()
    {
        _game.GameStageChangedEvent -= OnGameStageChanged;
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

    private void OnGameStageChanged(GameStage gameStage)
    {
        if (gameStage == GameStage.Showdown)
        {
            return;
        }

        if (C_StartBetCountdown != null)
        {
            Log.WriteLine("Bet coroutine is`t null. This should never happend.");
            StopCoroutine(C_StartBetCountdown);
        }
        C_StartBetCountdown = StartBetCountdown();
        StartCoroutine(C_StartBetCountdown);
    }

    private IEnumerator StartBetCountdown()
    {
        while (_bettingUI.ChoosedBetAction == 0 || _timePaased < _betTime)
        {
            _timePaased += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        BetActionEvent?.Invoke(_bettingUI.ChoosedBetAction);
    }
}