using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DeskBackButtonUI : MonoBehaviour
{
    // Button.
    public void HandleClick()
    {
        try
        {
            Player localPlayer = PlayerSeats.Instance.LocalPlayer;
            if (localPlayer != null)
            {
                localPlayer.HandleEscapeButton();
            }
            else
            {
                // Fuck off from the scene anyway.
                SceneManager.LoadScene(Constants.SceneNames.Menu);
            }
        }
        catch (Exception e)
        {
            Logger.Log($"Can`t handle desk back button: {e.Message}", Logger.LogLevel.Error);
        }
    }
}
