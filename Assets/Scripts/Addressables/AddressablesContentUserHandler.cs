using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AddressablesContentUserHandler : MonoBehaviour
{
    public static AddressablesContentUserHandler Instance { get; private set; }

    public uint AssetsCount => GetAssetsCount();
    public uint LoadedAssetsCount => GetLoadedAssetsCount();
    
    private readonly List<IAddressableContentUser> _contentUsers = new();
    
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

    private void Start()
    {
        // Take all objects with IAddressableContentUser component
        List<IAddressableContentUser> addressableContentUsers = FindObjectsOfType<MonoBehaviour>().OfType<IAddressableContentUser>().ToList();
        foreach (IAddressableContentUser user in addressableContentUsers)
        {
            AddContentUser(user);
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void AddContentUser(IAddressableContentUser contentUser)
    {
        _contentUsers.Add(contentUser);
    } 
    
    private uint GetAssetsCount()
    {
        uint assetsCount = 0;
        
        foreach (IAddressableContentUser contentUser in _contentUsers)
        {
            assetsCount += contentUser.AssetsCount;
        }

        return assetsCount;
    }

    private uint GetLoadedAssetsCount()
    {
        return (uint)_contentUsers.Sum(x => x.LoadedCount);
    }
}
