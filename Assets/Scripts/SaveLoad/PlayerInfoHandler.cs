using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nickNameInputField;
    [SerializeField] private Image _image;

    private void Start()
    {
        if (File.Exists(SaveLoadSystem.PlayerSaveFile) == true)
        {
            PlayerData? playerData = SaveLoadSystem.LoadPlayerData();

            _nickNameInputField.text = playerData?.NickName;

            Sprite avatar = Resources.Load<Sprite>($"Sprites/{playerData?.ImageID}"); // Add image from browse window and after save it on persistentPath
            _image.sprite = avatar;
        }
        else
        {
            _nickNameInputField.text = "Player";
            _image.sprite = Resources.Load<Sprite>("Sprites/Clown");

            Save();
        }
    }

    private void Save()
    {
        if (File.Exists(SaveLoadSystem.PlayerSaveFile) == false)
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/saves");
        }

        SaveLoadSystem.SavePlayerData(new PlayerData(_nickNameInputField.text, _image.sprite.name));
    }
}
