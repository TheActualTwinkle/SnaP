using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SeatUI : MonoBehaviour
{
    public Image Image;

    private void Start()
    {
        Image = GetComponent<Image>();
    }
}
