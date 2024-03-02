using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotUI : MonoBehaviour, IChipsAssetUser
{
    [SerializeField] private Image _chipsImage;
    [SerializeField] private TextMeshProUGUI _valueText;

    private static Pot Pot => Pot.Instance;
    
    private List<Sprite> _preloadedChipsSprites;
    
    private void OnEnable()
    {
        Pot.ValueNetworkVariable.OnValueChanged += OnPotNetworkVariableValueChanged;
    }

    private void OnDisable()
    {
        Pot.ValueNetworkVariable.OnValueChanged -= OnPotNetworkVariableValueChanged;
    }

    private void Start()
    {
        uint potValue = Pot.ValueNetworkVariable.Value;
        if (potValue == 0)
        {
            return;
        }
        
        Show(potValue);
    }

    public void SetChipsSprites(IEnumerable<Sprite> sprite)
    {
        _preloadedChipsSprites = sprite.ToList();
        
        const string assetId = Constants.Sprites.Chips.Pot;
        _chipsImage.sprite = _preloadedChipsSprites.Find(x => x.name == assetId);
    }
    
    private void OnPotNetworkVariableValueChanged(uint previousValue, uint newValue)
    {
        if (newValue == 0)
        {
            Hide();
            return;
        }
        
        if (_valueText.text != newValue.ToString())
        {
            SfxAudioPlayer.Instance.Play(Constants.Sound.Sfx.Type.ToPot);
        }
        
        Show(newValue);
    }

    private void Show(uint value)
    {
        _chipsImage.enabled = true;
        _valueText.text = value.ToString();
    }

    private void Hide()
    {
        _valueText.text = string.Empty;
        _chipsImage.enabled = false;
    }
}