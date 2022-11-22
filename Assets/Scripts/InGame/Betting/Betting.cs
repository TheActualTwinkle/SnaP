using System;
using System.Collections;
using UnityEngine;

public class PlayerBetting : MonoBehaviour
{
    public static PlayerBetting Instance { get; private set; }

    public event Action<BetAction> BetActionEvent;

    public IEnumerator StartBetCountdownCoroutine { get; private set; }

    [SerializeField] private float _betTime;
    private float _timePaased;

    private static Game Game => Game.Instance;
    private static PlayerBettingUI BettingUI => PlayerBettingUI.Instance;

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

        if (StartBetCountdownCoroutine != null)
        {
            Log.WriteLine("Bet coroutine is`t null. This should never happens.");
            StopCoroutine(StartBetCountdownCoroutine);
        }
        StartBetCountdownCoroutine = StartBetCountdown();
        StartCoroutine(StartBetCountdownCoroutine);
    }

    private IEnumerator StartBetCountdown()
    {
        while (BettingUI.ChoosenBetAction == 0 || _timePaased < _betTime)
        {
            _timePaased += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        BetActionEvent?.Invoke(BettingUI.ChoosenBetAction);
    }
}