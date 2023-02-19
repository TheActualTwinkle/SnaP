using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StackUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stackText;
    
    [SerializeField] private Image _stackBackgroundImage;

    [SerializeField] private int _index;
    
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    
    private void OnEnable()
    {
        PlayerSeats.PlayerSitEvent += OnPlayerSit;
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
    }

    private void OnDisable()
    {
        PlayerSeats.PlayerSitEvent -= OnPlayerSit;
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;
    }
    
    private void OnStackValueChanged(uint oldValue, uint newValue)
    {
        _stackText.text = newValue.ToString();
    }

    private void OnPlayerSit(Player player, int index)
    {
        if (index != _index)
        {
            return;
        }

        player.StackNetworkVariable.OnValueChanged += OnStackValueChanged;
        
        _stackText.text = player.Stack.ToString();
        _stackBackgroundImage.enabled = true;
    }

    private void OnPlayerLeave(Player player, int index)
    {
        if (index != _index)
        {
            return;
        }
        
        player.StackNetworkVariable.OnValueChanged -= OnStackValueChanged;

        _stackText.text = string.Empty;
        _stackBackgroundImage.enabled = false;
    }
}
