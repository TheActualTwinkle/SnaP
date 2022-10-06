using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BoardUI : MonoBehaviour
{
    [SerializeField] private Game _game;
    [SerializeField] private Animator _animator;

    private void OnEnable()
    {
        if (_game == null)
        {
            _game = FindObjectOfType<Game>();
        }
    }

    private void OnDisable()
    {
        if (_game == null)
        {
            return;
        }
    }
}
