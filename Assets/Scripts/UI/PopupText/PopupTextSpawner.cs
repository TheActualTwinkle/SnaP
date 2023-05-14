using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class PopupTextSpawner : MonoBehaviour
{
    [SerializeField] private PopupObject _prefab;
    [SerializeField] private string _text;

    private Camera _camera;
    
    private void Awake()
    {
        _camera = Camera.main;
    }

    // Button.
    private void SpawnOnMousePosition()
    {
        Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Spawn(mousePosition);
    }
    
    private void Spawn(Vector3 position)
    {
        PopupObject popupObject = Instantiate(_prefab, position, Quaternion.identity, transform);
        popupObject.SetText(_text);
        
        Destroy(popupObject.gameObject, popupObject.TimeOfLife);
    }
}
