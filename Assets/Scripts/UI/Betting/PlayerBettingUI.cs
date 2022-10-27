using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBettingUI : MonoBehaviour
{
    public static PlayerBettingUI Instance { get; private set; }

    public BetAction ChoosedBetAction => _choosedBetAction;
    [ReadOnly]
    [SerializeField] private BetAction _choosedBetAction;

    private List<IToggle<BetAction>> _toggles;

    private Game _game => Game.Instance; 

    private void OnEnable()
    {
        _game.GameStageChangedEvent += OnGameStageChanged;

        foreach (var toggle in _toggles)
        {
            toggle.ToggleOnEvent += OnBetActionToggleOn;
        }
    }

    private void OnDisable()
    {
        _game.GameStageChangedEvent -= OnGameStageChanged;

        foreach (var toggle in _toggles)
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
        _choosedBetAction = 0;
        SetupButtons();
    }

    private void SetupButtons()
    {
        // Setup buttons.
    }

    private void OnBetActionToggleOn(BetAction betAction)
    {
        _choosedBetAction = betAction;
    }
}
