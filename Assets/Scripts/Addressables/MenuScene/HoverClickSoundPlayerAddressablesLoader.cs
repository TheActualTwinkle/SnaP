using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(HoverClickSoundPlayer))]
public class HoverClickSoundPlayerAddressablesLoader : MonoBehaviour, IAddressablesLoader
{
    public Constants.Sound.Sfx.Type HoverType { get; set; }
    public Constants.Sound.Sfx.Type ClickType { get; set; }
    
    public uint LoadedCount { get; private set; }

    public uint AssetsCount => 2;

    private AudioClip _hoverAudioClip;

    private AudioClip _clickAudioClip;

    private static bool _isLoading;

    private void OnApplicationQuit()
    {
        UnloadContent();
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

        try
        {
            _hoverAudioClip = await AddressablesLoader.LoadAsync<AudioClip>(Constants.Sound.Sfx.Paths[HoverType]);
            LoadedCount++;
            _clickAudioClip = await AddressablesLoader.LoadAsync<AudioClip>(Constants.Sound.Sfx.Paths[ClickType]);
            LoadedCount++;
        }
        catch (Exception e)
        {
            throw new Exception($"Error while loading content: {e.Message} for {gameObject.name}");
        }
        finally
        {
            _isLoading = false;
        }

        HoverClickSoundPlayer.SetClips(_hoverAudioClip, _clickAudioClip);
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
