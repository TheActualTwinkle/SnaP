using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotUI : MonoBehaviour
{
    [SerializeField] private Image _chipsImage;
    [SerializeField] private TextMeshProUGUI _valueText;
    
    private static Game Game => Game.Instance;
    private static Pot Pot => Pot.Instance;
    
    private void OnEnable()
    {
        Game.EndDealEvent += OnEndDeal;
        Game.GameStageOverEvent += OnGameStageOver;
    }

    private void OnDisable()
    {
        Game.EndDealEvent -= OnEndDeal;
        Game.GameStageOverEvent -= OnGameStageOver;
    }

    private void OnGameStageOver(GameStage gameStage)
    {
        if (_valueText.text != Pot.Value.ToString())
        {
            SfxAudio.Instance.Play(2);
        }
        
        _chipsImage.enabled = true;
        _valueText.text = Pot.Value.ToString();
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        _valueText.text = string.Empty;
        _chipsImage.enabled = false;
    }
}