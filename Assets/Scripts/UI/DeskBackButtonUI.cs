using System;
using UnityEngine;
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
            if (PlayerSeats.Instance.IsLocalPlayerSitting == true)
            {
                localPlayer.HandleEscapeButton();
            }
            else
            {
                // Fuck off from the scene anyway.
                localPlayer.LeaveGame();
            }
        }
        catch (Exception e)
        {
            Logger.Log($"Can`t handle desk back button: {e.Message}", Logger.LogLevel.Error);
            SceneLoader.Instance.LoadScene(Constants.SceneNames.Menu, false);
        }
    }
}
