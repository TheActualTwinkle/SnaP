using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Betting : MonoBehaviour 
{
    public static Betting Instance { get; private set; }
    
    public event Action<Player, BetAction> BetActionEvent;

    public Player CurrentBetter { get; private set; }

    private IEnumerator _startBetCountdownCoroutine;

    private static uint CallAmount => PlayerSeats.Instance.Players.Where(x => x != null).Select(x => x.BetAmount).Max();

    public uint BigBlind => _bigBlind;
    [ReadOnly] [SerializeField] private uint _bigBlind;
    
    public uint SmallBlind => _smallBlind;
    [SerializeField] private uint _smallBlind;
    
    
    [SerializeField] private float _betTime;
    [SerializeField] [ReadOnly] private float _timePaasedSinceTurn;
    
    private const float BetInterval = 1f;

    private void OnValidate()
    {
        _bigBlind = _smallBlind * 2;    
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
    
    public static BetSituation GetBetSituation(uint betAmount)
    {
        return betAmount <= CallAmount ? BetSituation.CallEqualsOrLessCheck : BetSituation.CallGreaterCheck;
    }
    
    public IEnumerator StartBetCountdown(Player player)
    {
        CurrentBetter = player;

        yield return new WaitForSeconds(BetInterval);
        
        while (player.ChoosenBetAction == BetAction.Empty && _timePaasedSinceTurn < _betTime)
        {
            _timePaasedSinceTurn += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _timePaasedSinceTurn = 0f;
        BetActionEvent?.Invoke(player, player.ChoosenBetAction);
    }
}