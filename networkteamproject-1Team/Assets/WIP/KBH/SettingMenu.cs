using System;
using Player;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;


public class SettingMenu : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private TMP_Dropdown displayDropdown;

    [Header("사운드 설정")] 
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    
    [Header("마우스 감도")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityValueText;
    private const float DEFAULT_SENSITIVITY = 600f;
    private const float MIN_SENSITIVITY = 100f;
    private const float MAX_SENSITIVITY = 1500f;
    
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private GameObject settings;
    
    private const string KEY_DISPLAY = "display";
    private const string KEY_MASTER = "vol_master";
    private const string KEY_BGM = "vol_bgm";
    private const string KEY_SFX = "vol_sfx";
    private const string KEY_SENS = "sensitivity";
    
    private void OnEnable()
    {
        LoadSettings();
    }

    private void Start()
    {
        if (masterSlider != null)
        {
            masterSlider.minValue = 0f;
            masterSlider.maxValue = 1f;
        }

        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = MIN_SENSITIVITY;
            sensitivitySlider.maxValue = MAX_SENSITIVITY;
        }
        
        displayDropdown?.onValueChanged.AddListener(OnDisplayChanged);
        masterSlider?.onValueChanged.AddListener(OnMasterChanged);
        bgmSlider?.onValueChanged.AddListener(OnBgmChanged);
        sfxSlider?.onValueChanged.AddListener(OnSfxChanged);
        sensitivitySlider?.onValueChanged.AddListener(OnSensitivityChanged);
        
        LoadSettings();
    }

    private void OnDisplayChanged(int index)
    {
        bool ifFullscreen = (index == 1);
        Screen.fullScreen = ifFullscreen;
        PlayerPrefs.SetInt(KEY_DISPLAY, index);
        PlayerPrefs.Save();
    }

    private void OnMasterChanged(float value)
    {
        // TODO: AudioManager 연결 필요 AudioManager.Instance.SetMasterVolume(value);
        PlayerPrefs.SetFloat(KEY_MASTER, value);
    }
    
    private void OnBgmChanged(float value)
    {
        // TODO: AudioManager.Instance.SetBGMVolume(value);
        PlayerPrefs.SetFloat(KEY_BGM, value);
    }

    private void OnSfxChanged(float value)
    {
        // TODO: AudioManager.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat(KEY_SFX, value);
    }

    private void OnSensitivityChanged(float value)
    {
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = Mathf.RoundToInt(value).ToString();
        }
        
        // TODO: 카메라 연결
        PlayerPrefs.SetFloat(KEY_SENS, value);
    }

    private void LoadSettings()
    {
        // UI
        int displayIndex = PlayerPrefs.GetInt(KEY_DISPLAY, 0);
        if (displayDropdown != null)
        {
            displayDropdown.SetValueWithoutNotify(displayIndex);
            Screen.fullScreen = (displayIndex == 1);
        }
        
        // Sound
        float master = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
        float bgm = PlayerPrefs.GetFloat(KEY_BGM, 1f);
        float sfx = PlayerPrefs.GetFloat(KEY_SFX, 1f);
        masterSlider?.SetValueWithoutNotify(master);
        bgmSlider?.SetValueWithoutNotify(bgm);
        sfxSlider?.SetValueWithoutNotify(sfx);
        
        // Sensitivity
        float sensitivity = PlayerPrefs.GetFloat(KEY_SENS, DEFAULT_SENSITIVITY);
        
        if (sensitivitySlider != null)
        {
            sensitivitySlider.SetValueWithoutNotify(sensitivity);
        }

        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = Mathf.RoundToInt(sensitivity).ToString();
        }
    }

    private void CloseSetting()
    { 
        settings.SetActive(false);

        if (pauseMenu != null)
        {
            pauseMenu.OpenPausePanel();
        }
    }





}
