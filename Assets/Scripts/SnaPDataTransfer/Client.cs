using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace SnaPDataTransfer
{
    public class Client : MonoBehaviour
    {
        private static Client Instance { get; set; }

        [SerializeField] private string _pipeName;
        [SerializeField] private int _connectionTimeout;
        [SerializeField] private int _requestNotFoundTimeout;
        
        private NamedPipeClientStream _pipeClient;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async void Start()
        {
            if (await TryConnect(_pipeName) == false)
            {
                Logger.Log($"[SnaPDataTransfer] Can`t connect to {_pipeName} pipe.", Logger.LogLevel.Error);
                return;
            }

            StreamString streamString = new(_pipeClient);
            
            EndlessHandeServerRequest(streamString);
        }

        private async Task<bool> TryConnect(string pipeName)
        {
            _pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

            Logger.Log("[SnaPDataTransfer] Connecting to pipe server...");
            
            try
            {
                await _pipeClient.ConnectAsync(_connectionTimeout);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void EndlessHandeServerRequest(StreamString streamString)
        {
            while (true)
            {
                string request = await GetRequestAsync(streamString);

                if (request == "404")
                {
                    await Task.Delay(_requestNotFoundTimeout);
                    continue;
                }
                
                Logger.Log($"[SnaPDataTransfer] Got request: {request}");

                string response = GetResponse(request);
                Logger.Log($"[SnaPDataTransfer] Figured response: {response}");

                Logger.Log($"[SnaPDataTransfer] Sending response...");
                await SendDataAsync(response, streamString);
            }
            
            // ReSharper disable once FunctionNeverReturns
        }

        private async Task<string> GetRequestAsync(StreamString streamString)
        {
            return await streamString.ReadStringAsync();
        }
        
        private static string GetResponse(string request)
        {
            string response;
            if (Enum.TryParse(typeof(Request), request, true, out object requestAsEnum) == true)
            {
                response = requestAsEnum switch
                {
                    Request.Get => JsonConvert.SerializeObject(new LobbyInfo(
                        PlayerSeats.MaxSeats,
                        PlayerSeats.Instance.PlayersAmount + PlayerSeats.Instance.WaitingPlayersAmount,
                        "TODO: Create a lobby name feature!"
                    )),
                    Request.IsLobbyReady => PlayerSeats.Instance == null ? "N" : "Y",
                    _ => throw new ArgumentException()
                };
            }
            else
            {
                response = "400";
            }

            return response;
        }

        private async Task SendDataAsync(string dataToSend, StreamString streamString)
        {
            await streamString.WriteStringAsync(dataToSend);
        }
    }   
}