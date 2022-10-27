using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
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

        Sprite avatar = Resources.Load<Sprite>($"Sprites/{playerData.ImageID}");
        _image.sprite = avatar;
    }

    private void OnInputFiledEndEdit(string value)
    {
        PlayerData playerData = new PlayerData(value, ""); // When create SetImage(): find path to img and paste it as 2nd parameter.
        _saveLoadSystem.Save(playerData);
    }

    private void SetImage() // Add image from browse window and after save it on persistentPath
    {
        
    }
}
