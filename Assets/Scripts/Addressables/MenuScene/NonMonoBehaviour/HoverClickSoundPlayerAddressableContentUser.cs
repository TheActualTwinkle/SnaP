using System.Threading.Tasks;
using UnityEngine;

public class HoverClickSoundPlayerAddressableContentUser : IAddressableContentUser
{
    private readonly HoverClickSoundPlayer _soundPlayer;
    
    private AudioClip _hoverAudioClip;
    private AudioClip _clickAudioClip;
    
    public HoverClickSoundPlayerAddressableContentUser(HoverClickSoundPlayer soundPlayer)
    {
        _soundPlayer = soundPlayer;
    }
    
    public async Task LoadContent()
    {
        _hoverAudioClip = await AddressablesLoader.LoadAsync<AudioClip>(Constants.Sound.Sfx.Paths[Constants.Sound.Sfx.Type.ButtonHover]);
        _clickAudioClip = await AddressablesLoader.LoadAsync<AudioClip>(Constants.Sound.Sfx.Paths[Constants.Sound.Sfx.Type.ButtonClick]);
        
        _soundPlayer.SetClips(_hoverAudioClip, _clickAudioClip);
    }

    public void UnloadContent()
    {
        AddressablesLoader.Unload(_hoverAudioClip);
        AddressablesLoader.Unload(_clickAudioClip);
    }
}
