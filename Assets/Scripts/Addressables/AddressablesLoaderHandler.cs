using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AddressablesLoaderHandler : MonoBehaviour
{
    public static AddressablesLoaderHandler Instance { get; private set; }

    public static string LoadTarget => GetLoadTarget();

    public uint AssetsCount => GetAssetsCount();
    public uint LoadedAssetsCount => GetLoadedAssetsCount();
    
    private readonly List<IAddressablesLoader> _contentUsers = new();
    
    private void Awake()
    {
#if UNITY_SERVER
        Destroy(gameObject);
        return;
#endif

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        // Take all objects with IAddressableContentUser component
        List<IAddressablesLoader> addressableContentUsers = FindObjectsOfType<MonoBehaviour>().OfType<IAddressablesLoader>().ToList();
        foreach (IAddressablesLoader user in addressableContentUsers)
        {
            AddContentUser(user);
        }
        
        float startTime = Time.realtimeSinceStartup;
        
        // Load all content
        foreach (IAddressablesLoader user in _contentUsers)
        {
            try
            {
                await user.LoadContent();
            }
            catch(Exception e)
            {
                Logger.Log($"Error while loading content: {e.Message}", Logger.LogLevel.Error, Logger.LogSource.Addressables);
            }
        }
        
        float endTime = Time.realtimeSinceStartup;
        
        Logger.Log($"All {AssetsCount} assets loaded for {endTime - startTime} seconds", Logger.LogSource.Addressables);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void AddContentUser(IAddressablesLoader contentUser)
    {
        _contentUsers.Add(contentUser);
    } 
    
    private uint GetAssetsCount()
    {
        uint assetsCount = 0;
        
        foreach (IAddressablesLoader contentUser in _contentUsers)
        {
            assetsCount += contentUser.AssetsCount;
        }

        return assetsCount;
    }

    private uint GetLoadedAssetsCount()
    {
        return (uint)_contentUsers.Sum(x => x.LoadedCount);
    }
    
    private static string GetLoadTarget()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
                return "StandaloneWindows64";
            case RuntimePlatform.LinuxPlayer:
                return "StandaloneLinux64";;
            case RuntimePlatform.OSXPlayer:
                return "StandaloneOSX";
            case RuntimePlatform.WindowsEditor:
                return "StandaloneWindows64";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
