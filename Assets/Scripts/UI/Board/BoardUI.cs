using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BoardUI : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    
    private static Game Game => Game.Instance;

    private void OnEnable()
    {
        Game.GameStageChangedEvent += OnGameStageChanged;
        Game.EndDealEvent += OnEndDeal;
    }

    private void OnDisable()
    {
        Game.GameStageChangedEvent -= OnGameStageChanged;
        Game.EndDealEvent -= OnEndDeal;
    }
    
    private void OnGameStageChanged(GameStage gameStage)
    {
        if (gameStage != GameStage.Preflop)
        {
            return;
        }
        
        _animator.ResetTrigger("EndDeal");
        _animator.SetTrigger("StartPreflop");
    }
    
    private void OnEndDeal(WinnerData winnerData)
    {        
        _animator.ResetTrigger("StartPreflop");
        _animator.SetTrigger("EndDeal");
    }
}
