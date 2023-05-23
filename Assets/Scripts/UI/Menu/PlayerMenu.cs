using System.Collections;
using TMPro;
using Unity.Netcode;
#if UNITY_EDITOR
    using UnityEditor;
#endif
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
        _saveLoadSystem = ReadonlySaveLoadSystemFactory.Instance.Get();
        SetupUI();
        SetupBetSliderStep();
    }

    private void SetupUI()
    {
        PlayerData playerData = _saveLoadSystem.Load<PlayerData>();

        if (playerData.Equals(default(PlayerData)) == true)
        {
            playerData.SetDefaultValues();
            SavePlayerData();
        }
        
        _nickNameInputField.text = playerData.NickName;
        _stackSlider.value = playerData.Stack;
        
        PlayerAvatarData avatarData = _saveLoadSystem.Load<PlayerAvatarData>();
        if (avatarData.Equals(default(PlayerAvatarData)) == true)
        {
            avatarData.SetDefaultValues();
            SavePlayerAvatar();
        }
        
        _image.sprite = TextureConverter.GetSprite(avatarData.CodedValue, (int)AvatarImageWidth, (int)AvatarImageHeight);
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
#if UNITY_EDITOR // todo
        string filePath = EditorUtility.OpenFilePanel("Select avatar", "", "jpg,png");
        yield return StartCoroutine(SetImageFromLocalFIle(filePath));

        SavePlayerData();
#endif

        yield return null;
    }

    private IEnumerator SetImageFromLocalFIle(string filePath)
    {
        using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(filePath);
        
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Log.WriteToFile($"An error occurred while trying to set image. {webRequest.error}");
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
        _image.sprite = TextureConverter.GetSprite(texture, AvatarImageWidth, AvatarImageHeight);

        SavePlayerAvatar();
    }

    private void SavePlayerData()
    {
        PlayerData playerData = new(_nickNameInputField.text, (uint)_stackSlider.value);
        _saveLoadSystem.Save(playerData);
    }

    private void SavePlayerAvatar()
    {
        byte[] rawTexture = TextureConverter.GetRawTexture(_image.sprite.texture);
        PlayerAvatarData avatarData = new(rawTexture);
        
        _saveLoadSystem.Save(avatarData);
    }
}