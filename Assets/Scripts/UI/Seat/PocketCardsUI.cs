using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PocketCardsUI : MonoBehaviour
{
    [SerializeField] Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
}
