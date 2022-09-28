using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSeatUI : MonoBehaviour
{
    [SerializeField] private List<SeatUI> _seats;

    private void OnValidate()
    {
        _seats = GetComponentsInChildren<SeatUI>().ToList();
    }
}
