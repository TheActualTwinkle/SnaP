using TMPro;
using UnityEngine;

public class PopupTextSpawner : MonoBehaviour
{
    [SerializeField] private PopupObject _prefab;
    [SerializeField] private string _text;

    private Camera _camera;
    
    private void Awake()
    {
        _camera = Camera.main;
    }

    public void SetText(string text)
    {
        _text = text;
    }
    
    // Button.
    public void SpawnOnMousePosition()
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
