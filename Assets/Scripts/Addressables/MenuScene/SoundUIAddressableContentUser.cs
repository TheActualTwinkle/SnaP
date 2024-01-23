using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SoundUI))]
public class SoundUIAddressableContentUser : MonoBehaviour, IAddressableContentUser
{
    [SerializeField] private Image _musicImage; 
    [SerializeField] private Image _musicCrossImage;
    [SerializeField] private Image _sfxCrossImage;

    private void Start()
    {
        LoadContent();
    }

    public async void LoadContent()
    {
        _musicImage.sprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Music);
        
        Sprite crossSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Cross);
        _musicCrossImage.sprite = crossSprite;
        _sfxCrossImage.sprite = crossSprite;
    }
}
