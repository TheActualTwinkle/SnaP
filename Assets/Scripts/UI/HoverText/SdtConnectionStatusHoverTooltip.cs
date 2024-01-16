using System;
using System.Collections;
using System.Collections.Generic;
using SDT;
using UnityEngine;

public class SdtConnectionStatusHoverTooltip : HoverTooltip
{
    private Client SdtClient => Client.Instance;
    private Server SdtServer => Server.Instance;

    private SdtType _sdtType;

    public void SetSdtType(SdtType sdtType)
    {
        _sdtType = sdtType;
    }
    
    public override void SetupText()
    {
        ConnectionState state = _sdtType switch
        {
            SdtType.Server => SdtServer.ConnectionState,
            SdtType.Client => SdtClient.ConnectionState,
            _ => throw new ArgumentOutOfRangeException()
        };

        Text.text = state switch
        {
            ConnectionState.Connecting => "Connecting to SnaP Data Transfer server...",
            ConnectionState.Successful => "You are connected to SnaP Data Transfer server!",
            ConnectionState.Failed => "Connection to SnaP Data Transfer server failed.\nClick to reconnect.",
            ConnectionState.Disconnected => "Disconnected.\nYour lobby not visible outside your LAN.\nClick to reconnect.",
            ConnectionState.Abandoned => "Abandoned.\nConnection to SnaP Data Transfer corrupted.\nPlease restart the game.",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
