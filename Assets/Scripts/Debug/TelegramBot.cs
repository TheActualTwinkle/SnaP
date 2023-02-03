using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TelegramCallbacks : MonoBehaviour
{
    public static TelegramCallbacks Instance { get; private set; }
    
    [SerializeField] private List<string> _chatIDs;
    [SerializeField] private string _token;
    
    private string ApiUrl => $"https://api.telegram.org/bot{_token}/";

    private readonly Queue<UnityWebRequest> _sendRequests = new();

    private IEnumerator _sendRequestCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        SendFile(File.ReadAllBytes(Log.LogFilePath), "Logs.txt");
    }

    public void GetMe()
    {
        WWWForm form = new();
        UnityWebRequest www = UnityWebRequest.Post(ApiUrl + "getMe", form);
        
        EnqueueRequest(www);
    }

    public new void SendMessage(string text)
    {
        foreach (string chatID in _chatIDs)
        {
            WWWForm form = new();
            form.AddField("chat_id", chatID);
            form.AddField("text", text);
            UnityWebRequest www = UnityWebRequest.Post(ApiUrl + "sendMessage?", form);

            EnqueueRequest(www);
        }
    }
    
    public void SendFile(byte[] bytes, string filename = "test.txt", string caption = "")
    {
        foreach (string chatID in _chatIDs)
        {
            WWWForm form = new();
            form.AddField("chat_id", chatID);
            form.AddField("caption", caption);
            form.AddBinaryData("document", bytes, filename, "filename");
            UnityWebRequest www = UnityWebRequest.Post(ApiUrl + "sendDocument?", form);
            
            EnqueueRequest(www);
        }
    }

    public void SendPhoto(byte[] bytes, string filename, string caption = "")
    {
        foreach (string chatID in _chatIDs)
        {        
            WWWForm form = new();
            form.AddField("chat_id", chatID);
            form.AddField("caption", caption);
            form.AddBinaryData("photo", bytes, filename, "filename");
            UnityWebRequest www = UnityWebRequest.Post(ApiUrl + "sendPhoto?", form);
            
            EnqueueRequest(www);
        }
    }

    private void EnqueueRequest(UnityWebRequest request)
    {
        if (_sendRequestCoroutine != null)
        {
            _sendRequests.Enqueue(request);
            return;
        }
            
        StartCoroutine(SendRequest(request));
    }
    
    private IEnumerator SendRequest(UnityWebRequest request)
    {
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            //Debug.Log("Success!\n" + www.downloadHandler.text);
            request.Dispose();
        }

        if (_sendRequests.Count == 0)
        {
            _sendRequestCoroutine = null;
            yield break;    
        }

        UnityWebRequest newRequest = _sendRequests.Dequeue();
        _sendRequestCoroutine = SendRequest(newRequest);
        StartCoroutine(_sendRequestCoroutine);
    }
}