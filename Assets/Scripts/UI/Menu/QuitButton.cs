using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuitButton : MonoBehaviour
{
    // Button.
    private void Quit()
    {
        Application.Quit();
    }
}
