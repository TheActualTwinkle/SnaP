using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Betting : MonoBehaviour 
{
    public static Betting Instance { get; private set; }
    
    public event Action<BetAction> BetActionEvent;

    public NetworkObject CurrentBetter { get; private set; }
    
    public BetSituation BetSituation => GetBetSituation();

    private IEnumerator _startBetCountdownCoroutine;
        
    [ReadOnly] [SerializeField] private uint _bigBlindValue;
    [SerializeField] private uint _smallBlindValue;
    [SerializeField] private float _betTime;
    
    private float _timePaasedSinceTurn;
    private uint _callAmount;

    private static Game Game => Game.Instance;
    private static BettingUI BettingUI => BettingUI.Instance;
    
    private void OnValidate()
    {
        _bigBlindValue = _smallBlindValue * 2;
    }
    
    private void OnEnable()
    {
        Game.GameStageChangedEvent += OnGameStageChanged;
    }

    private void OnDisable()
    {
        Game.GameStageChangedEvent -= OnGameStageChanged;
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

        if (_startBetCountdownCoroutine != null)
        {
            Log.WriteLine("Bet coroutine is`t null. This should never happens."); // Why?
            StopCoroutine(_startBetCountdownCoroutine);
        }
        _startBetCountdownCoroutine = StartBetCountdown();
        StartCoroutine(_startBetCountdownCoroutine);
    }

    private IEnumerator StartBetCountdown()
    {
        CurrentBetter = NetworkManager.Singleton.LocalClient.PlayerObject;
        while (BettingUI.ChoosenBetAction == BetAction.Empty || _timePaasedSinceTurn < _betTime)
        {
            _timePaasedSinceTurn += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        BetActionEvent?.Invoke(BettingUI.ChoosenBetAction);
    }

    private BetSituation GetBetSituation()
    {
        return Game.CallAmount <= _callAmount ? BetSituation.CallEqualsOrLessCheck : BetSituation.CallGreaterCheck;
    }
}