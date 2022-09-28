using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PocketCardsUI : MonoBehaviour
{
    [SerializeField] private List<Image> _cardIamges;

    private void OnValidate()
    {
        _cardIamges = GetComponentsInChildren<Image>(true).ToList();

        if (_cardIamges.Count != 2)
        {
            Debug.LogError($"There is not 2 cards in pocket");
        }
    }
}
