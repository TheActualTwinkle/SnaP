using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerBettingUI : MonoBehaviour
{
    public static PlayerBettingUI Instance { get; private set; }

    public BetAction ChoosenBetAction => _choosenBetAction;
    [ReadOnly]
    [SerializeField] private BetAction _choosenBetAction;

    private List<IToggle<BetAction>> _toggles;

    private static Game Game => Game.Instance; 

    private void OnEnable()
    {
        Game.GameStageChangedEvent += OnGameStageChanged;

        foreach (IToggle<BetAction> toggle in _toggles)
        {
            toggle.ToggleOnEvent += OnBetActionToggleOn;
        }
    }

    private void OnDisable()
    {
        Game.GameStageChangedEvent -= OnGameStageChanged;

        foreach (IToggle<BetAction> toggle in _toggles)
        {
            toggle.ToggleOnEvent -= OnBetActionToggleOn;
        }
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

        _toggles = GetComponentsInChildren<IToggle<BetAction>>().ToList();
    }

    private void OnGameStageChanged(GameStage gameStage)
    {
        _choosenBetAction = 0;
        SetupButtons();
    }

    private void SetupButtons()
    {
        
    }

    private void OnBetActionToggleOn(BetAction betAction)
    {
        _choosenBetAction = betAction;
    }
}