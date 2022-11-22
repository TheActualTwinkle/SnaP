using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Button : NetworkBehaviour
{
    private const int NullIndex = -1;
    
    public int Index => _index.Value;
    private NetworkVariable<int> _index = new(NullIndex);

    private void OnEnable()
    {
        throw new NotImplementedException();
    }

    private void OnDisable()
    {
        throw new NotImplementedException();
    }

    private void OnGameStageChanged(GameStage gameStage)
    {
        if (gameStage == GameStage.Preflop)
        {
            if (_index.Value == PlayerSeats.MaxSeats - 1)
            {
                _index.Value = 0;
            }
            
            
        }
    }
    
    []
}
