using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using SFB;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerMenu : MonoBehaviour, INetworkSerializeByMemcpy
{
    [SerializeField] private TMP_InputField _nickNameInputField;
    [SerializeField] private Image _image;
    [SerializeField] private Slider _stackSlider;
    [SerializeField] private float _sliderIntervalPerScroll;
    
    private int AvatarImageWidth => (int)_image.rectTransform.rect.width;
    private int AvatarImageHeight => (int)_image.rectTransform.rect.height;

    private ISaveLoadSystem _saveLoadSystem;

    private void OnEnable()
    {
        _stackSlider.onValueChanged.AddListener(OnSliderValueChanged);
        _nickNameInputField.onEndEdit.AddListener(OnInputFiledEndEdit);
    }

    private void OnDisable()
    {
        _stackSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        _nickNameInputField.onEndEdit.RemoveListener(OnInputFiledEndEdit);
    }

    private void Start()
    {
        _saveLoadSystem = SaveLoadSystemFactory.Instance.Get();
        SetupUI();
        SetupBetSliderStep();
    }

    private void SetupUI()
    {
        PlayerDto playerDto = _saveLoadSystem.Load<PlayerDto>();
        if (playerDto.Equals(default(PlayerDto)) == true)
        {
            playerDto.SetDefaultValues();
        }
        
        PlayerAvatarDto avatarDto = _saveLoadSystem.Load<PlayerAvatarDto>();
        if (avatarDto.Equals(default(PlayerAvatarDto)) == true)
        {
            avatarDto.SetDefaultValues();
        }
        
        _nickNameInputField.text = playerDto.NickName;
        _stackSlider.value = playerDto.Stack;

        _image.sprite = TextureConverter.GetSprite(avatarDto.CodedValue, AvatarImageWidth, AvatarImageHeight);

        SavePlayerData();
        SavePlayerAvatarData();
    }

    private void SetupBetSliderStep()
    {
        if (_stackSlider.TryGetComponent(out ISliderSetter sliderSetter) == true)
        {
            sliderSetter.IntervalPerScroll = _sliderIntervalPerScroll;
        }
    }
    
    private void OnInputFiledEndEdit(string value)
    {
        if (string.IsNullOrEmpty(value) == true)
        {
            _nickNameInputField.text = "Player";
        }
        
        SavePlayerData();
    }

    private void OnSliderValueChanged(float value)
    {
        SavePlayerData();
    }
   
    private void OnChangeImageButtonClick()
    {
        StartCoroutine(ChangeImage());
    }

    private IEnumerator ChangeImage()
    {
        ExtensionFilter[] extensions = { new("Image Files", "png", "jpg", "jpeg") };
        string[] imagesPath = StandaloneFileBrowser.OpenFilePanel("Chose avatar", "", extensions, false);

        if (imagesPath == null || imagesPath.Length == 0)
        {
            yield break;
        }
        
        string filePath = imagesPath.First();
        yield return StartCoroutine(SetImageFromLocalFIle(filePath));

        SavePlayerData();
    }

    private IEnumerator SetImageFromLocalFIle(string filePath)
    {
        filePath = "file://" + filePath;
        
        using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(filePath);
        
        yield return webRequest.SendWebRequest(); // todo There is an error when trying load .png pics with big size.

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.DataProcessingError:
            case UnityWebRequest.Result.ConnectionError:
                // todo make a error window.
                Logger.Log($"An error occurred while trying to set image. {webRequest.error}.", Logger.LogLevel.Error);
                PlayerAvatarDto avatarDto = new();
                avatarDto.SetDefaultValues();
                _image.sprite = TextureConverter.GetSprite(avatarDto.CodedValue, AvatarImageWidth, AvatarImageHeight);
                break;
            case UnityWebRequest.Result.Success:
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                _image.sprite = TextureConverter.GetSprite(texture, AvatarImageWidth, AvatarImageHeight);
                break;
        }

        SavePlayerAvatarData();
    }

    private void SavePlayerData()
    {
        PlayerDto playerDto = new(_nickNameInputField.text, (uint)_stackSlider.value);
        _saveLoadSystem.Save(playerDto);
    }

    private void SavePlayerAvatarData()
    {
        byte[] rawTexture = TextureConverter.GetRawTexture(_image.sprite.texture);
        PlayerAvatarDto avatarDto = new(rawTexture);
        
        _saveLoadSystem.Save(avatarDto);
    }
}