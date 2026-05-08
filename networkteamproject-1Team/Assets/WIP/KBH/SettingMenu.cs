using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SettingMenu : MonoBehaviour
{
    // Pause 메뉴 패널
    [Header("Pause 메뉴")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    
    // 설정 패널
    [Header("설정")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button backButton;

    [Header("사운드 설정")] 
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI bgmValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    
    [Header("마우스 감도")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityValueText;
    
    [Header("전체화면")]
    [SerializeField] private Toggle fullscreenToggle;
    
    
    private const string KEY_BGM = "vol_bgm";
    private const string KEY_SFX = "vol_sfx";
    private const string KEY_SENS = "sensitivity";
    
    private bool isPaused = false;

    private void Start()
    {
        
    }

}
