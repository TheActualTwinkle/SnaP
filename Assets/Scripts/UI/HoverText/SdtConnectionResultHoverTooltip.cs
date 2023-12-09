using System;
using System.Collections;
using System.Collections.Generic;
using SDT;
using UnityEngine;

public class SdtConnectionResultHoverTooltip : HoverTooltip
{
    private StandaloneClient SdtStandaloneClient => StandaloneClient.Instance;

    public override void SetupText()
    {
        switch (SdtStandaloneClient.ConnectionState)
        {
            case ConnectionState.Connecting:
                Text.text = "Connecting to SnaP Data Transfer server...";
                break;
            case ConnectionState.Successful:
                Text.text = "You are connected to SnaP Data Transfer server!";
                break;
            case ConnectionState.Failed:
                Text.text = "Connection to SnaP Data Transfer server failed.\nClick to reconnect.";
                break;
            case ConnectionState.Disconnected:
                Text.text = "You are currently disconnected from SnaP Data Transfer server.\nClick to reconnect.";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
