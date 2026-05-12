using System;
using Cysharp.Threading.Tasks;
using Player;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    
    [Header("메뉴버튼")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("종료 확인 팝업")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    
    [SerializeField] private GameObject settingsPanel;
    
    private PlayerInputHandler _playerInputHandler;
    private const InputCategory PauseBlock = InputCategory.All;
    private bool isPaused = false;

    private void Start()
    {
        resumeButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        yesButton.onClick.AddListener(OnConfirmYes);
        noButton.onClick.AddListener(OnConfirmNo);
        
        pauseMenu.SetActive(false);
        confirmPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (confirmPanel.activeSelf)
            {
                OnConfirmNo();
                return;
            }

            if (isPaused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        if (_playerInputHandler != null)
        {
            _playerInputHandler.DisableInput(PauseBlock);
        }

        pauseMenu.SetActive(true);
        confirmPanel.SetActive(false);
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Resume()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            pauseMenu.SetActive(true);
            return;
        }

        if (_playerInputHandler != null)
        {
            _playerInputHandler.EnableInput(PauseBlock);
        }
        
        pauseMenu.SetActive(false);
        confirmPanel.SetActive(false);
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnSettingsClicked()
    {
        pauseMenu.SetActive(false);
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void OpenPausePanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        pauseMenu.SetActive(true);
    }

    private void OnQuitClicked()
    {
        confirmPanel.SetActive(true);
    }

    private void OnConfirmYes()
    {
        if (_playerInputHandler != null)
        {
            _playerInputHandler.EnableInput(PauseBlock);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // yes 눌렀을때 메인 화면(씬)으로 바꿔주기
        LobbyManager.Instance.LeaveSessionAsync().Forget();
        SceneLoader.LoadLocal(0);
    }

    private void OnConfirmNo()
    {
        confirmPanel.SetActive(false);
    }
}
