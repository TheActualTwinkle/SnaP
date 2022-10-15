using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Image _image;

    private void Start()
    {
        if (File.Exists(SaveLoadSystem.PlayerSaveFile) == true)
        {
            PlayerData playerData = SaveLoadSystem.LoadPlayerData();

            _inputField.text = playerData.NickName;

            Sprite avatar = Resources.Load<Sprite>($"Sprites/{playerData.ImageID}");
            if (avatar == null)
            {
                avatar = Resources.Load<Sprite>("Sprites/Clown");
            }

            _image.sprite = avatar;
        }
    }

    public void Save()
    {
        if (File.Exists(SaveLoadSystem.PlayerSaveFile) == false)
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/saves");
        }

        SaveLoadSystem.SavePlayerData(new PlayerData(_inputField.text, _image.sprite.name));
    }
}
