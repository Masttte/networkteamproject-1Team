using System;
using Player;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        if (Input.GetKeyDown(KeyCode.Escape))
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
        pauseMenu.SetActive(true);
        confirmPanel.SetActive(false);
        isPaused = true;
    }


}
