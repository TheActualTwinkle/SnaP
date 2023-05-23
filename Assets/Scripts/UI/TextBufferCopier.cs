using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextBufferCopier : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    // Button.
    public void CopyTextFromTextObject()
    {
        GUIUtility.systemCopyBuffer = _text.text;
    }
}
