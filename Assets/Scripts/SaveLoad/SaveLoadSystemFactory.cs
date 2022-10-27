using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadSystemFactory : MonoBehaviour
{
    public static SaveLoadSystemFactory Instance { get; private set; }

    [SerializeField] private SaveLoadSystemType _saveLoadSystemType;

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
    }

    public ISaveLoadSystem Get()
    {
        if (_saveLoadSystem != null)
        {
            return _saveLoadSystem;
        }

        switch (_saveLoadSystemType)
        {
            case SaveLoadSystemType.Binary:
                return new BinarySaveLoadSystem();
            case SaveLoadSystemType.SQL:
                return new MySQLSaveLoadSystem();
            default:
                return null;
        }
    }
}
