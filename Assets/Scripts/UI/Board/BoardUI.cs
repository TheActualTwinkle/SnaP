using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BoardUI : MonoBehaviour
{
    [SerializeField] private Animator _animator;
}
