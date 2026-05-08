using System;
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
    
    [Header("전체화면")]
    [SerializeField] private Toggle fullscreenToggle;
    
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
        bgmSlider?.onValueChanged.AddListener(OnBGMChanged);
        
    }
}
