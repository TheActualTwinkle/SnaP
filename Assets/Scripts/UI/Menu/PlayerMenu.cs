using System;
using System.Collections;
using TMPro;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nickNameInputField;
    [SerializeField] private Image _image;

    private ISaveLoadSystem _saveLoadSystem;

    private void OnEnable()
    {
        _nickNameInputField.onEndEdit.AddListener(OnInputFiledEndEdit);
    }

    private void OnDisable()
    {
        _nickNameInputField.onEndEdit.RemoveListener(OnInputFiledEndEdit);
    }

    private void Start()
    {
        _saveLoadSystem = SaveLoadSystemFactory.Instance.Get();
        SetupUI();
    }

    private void SetupUI()
    {
        PlayerData playerData = _saveLoadSystem.Load<PlayerData>();
        _nickNameInputField.text = playerData.NickName;

        byte[] rawTexture = Convert.FromBase64String(playerData.AvatarBase64String);
        _image.sprite = TextureConverter.GetSprite(rawTexture);
    }

    private void OnInputFiledEndEdit(string value)
    {
        SavePlayerData();
    }
   
    // Button.
    private void OnChangeImageButtonClick()
    {
        StartCoroutine(ChangeImage());
    }

    private IEnumerator ChangeImage()
    {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select avatar", "", "jpg,png");
        yield return StartCoroutine(SetImageFromLocalFIle(filePath));

        SavePlayerData();
#endif
        yield return null;
    }

    private IEnumerator SetImageFromLocalFIle(string filePath)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(filePath))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Log.WriteLine(webRequest.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                _image.sprite = TextureConverter.GetSprite(texture);
            }
        }
    }

    private void SavePlayerData()
    {
        byte[] rawTexture = TextureConverter.GetRawTexture(_image.sprite.texture);
        PlayerData playerData = new(_nickNameInputField.text, Convert.ToBase64String(rawTexture));
        _saveLoadSystem.Save(playerData);
    }
}