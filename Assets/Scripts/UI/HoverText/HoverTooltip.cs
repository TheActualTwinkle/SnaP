using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public abstract class HoverTooltip : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI Text;

    public void Show()
    {
        Text.gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        Text.gameObject.SetActive(false);
    }
    
    public abstract void SetupText();
}
