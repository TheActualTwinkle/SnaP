using System;
using UnityEngine;

public class ReadonlySaveLoadSystemFactory : MonoBehaviour
{
    public static ReadonlySaveLoadSystemFactory Instance { get; private set; }

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

        SetChosen();
    }

    public ISaveLoadSystem Get()
    {
        return _saveLoadSystem;
    }

    private void SetChosen()
    {
        _saveLoadSystem = _chosenSaveLoadSystemType switch
        {
            SaveLoadSystemType.Binary => new BinarySaveLoadSystem(),
            SaveLoadSystemType.MySql => new MySqlSaveLoadSystem(),
            _ => throw new ArgumentOutOfRangeException(nameof(_chosenSaveLoadSystemType))
        };
    }
}
