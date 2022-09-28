using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConnectButtons : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 100, 1000));
        if (NetworkManager.Singleton.IsClient == false && NetworkManager.Singleton.IsServer == false)
        {
            if (GUILayout.Button("Host") == true)
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUILayout.Button("Server") == true)
            {
                NetworkManager.Singleton.StartServer();
            }

            if (GUILayout.Button("Client") == true)
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        GUILayout.EndArea();
    }
}
