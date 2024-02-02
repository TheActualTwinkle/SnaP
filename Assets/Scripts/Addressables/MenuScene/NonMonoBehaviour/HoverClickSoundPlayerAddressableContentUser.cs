using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class HoverClickSoundPlayerAddressableContentUser : IAddressableContentUser
{
    public uint LoadedCount { get; private set; }

    public uint AssetsCount => 2;

    private readonly HoverClickSoundPlayer _soundPlayer;
    
    private AudioClip _hoverAudioClip;
    private AudioClip _clickAudioClip;

    private static bool _isLoading;
    
    public HoverClickSoundPlayerAddressableContentUser(HoverClickSoundPlayer soundPlayer)
    {
        AddressablesContentUserHandler.Instance.AddContentUser(this);
        _soundPlayer = soundPlayer;
    }

    public async Task LoadContent()
    {
        if (_isLoading == true)
        {
            // Wait until loading for first asset is finished.
            // Then loading will be done for ~0 seconds for next instances.
            await WaitUntilLoadingIsFinished();
        }
        else
        {
            _isLoading = true;
        }
        
        _hoverAudioClip = await AddressablesLoader.LoadAsync<AudioClip>(Constants.Sound.Sfx.Paths[Constants.Sound.Sfx.Type.ButtonHover]);
        LoadedCount++;
        _clickAudioClip = await AddressablesLoader.LoadAsync<AudioClip>(Constants.Sound.Sfx.Paths[Constants.Sound.Sfx.Type.ButtonClick]);
        LoadedCount++;

        _isLoading = false;
        
        _soundPlayer.SetClips(_hoverAudioClip, _clickAudioClip);
    }

    public void UnloadContent()
    {
        AddressablesLoader.Unload(_hoverAudioClip);
        LoadedCount--;
        AddressablesLoader.Unload(_clickAudioClip);
        LoadedCount--;
    }
    
    private async Task WaitUntilLoadingIsFinished()
    {
        while (_isLoading == true)
        {
            await Task.Yield();
        }
    }
}
