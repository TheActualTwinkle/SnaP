using UnityEngine;

public class UI : MonoBehaviour
{
    private static Camera _mainCamera;

    private Canvas _canvas;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _canvas = GetComponent<Canvas>();
        
        SetCamera();
    }

    private void SetCamera()
    {
        _canvas.worldCamera = _mainCamera;
    }
}