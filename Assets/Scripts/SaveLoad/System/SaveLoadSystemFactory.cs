using System;
using UnityEngine;

// TODO: Has to be zenjected
public class SaveLoadSystemFactory : MonoBehaviour
{
    public static SaveLoadSystemFactory Instance { get; private set; }

    [SerializeField] private SaveLoadSystemType _chosenSaveLoadSystemType;

    private ISaveLoadSystem _saveLoadSystem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        SetupSystemFromInspector();
    }

    public ISaveLoadSystem Get()
    {
        return _saveLoadSystem;
    }

    private void SetupSystemFromInspector()
    {
        _saveLoadSystem = _chosenSaveLoadSystemType switch
        {
            SaveLoadSystemType.Binary => new BinarySaveLoadSystem(),
            _ => throw new ArgumentOutOfRangeException(nameof(_chosenSaveLoadSystemType))
        };
    }
}
