using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SocialPlatforms;
using UnityEngine.InputSystem;

public class TutorialKeyGuide : MonoBehaviour
{
    //[Inspector 연결 필요한것들]
    // - CitizenGuide  : A그룹 키 가이드 오브젝트 (Panel)
    // - MafiaGuide  : B그룹 키 가이드 오브젝트 (Panel)
    // - closeButton        : 닫기 버튼 (버튼 클릭 또는 F1 다시누르기)
    
    [Header("그룹별 키 가이드 오브젝트")] 
    [SerializeField] private GameObject citizenGuide;      // 시민 목표 및 키가이드
    [SerializeField] private GameObject mafiaGuide;        // 마피아 목표 및 키가이드
    
    [Header("닫기 버튼")]
    [SerializeField] private Button closeButton;
    
    // TODO: 본인이 속해있는 그룹을 호출해서 호출한 그룹의 가이드 보기
    //private PlayerGroup myGroup = PlayerGroup.None;
    private bool isOpen = false;
    
    private void Start()
    {
        citizenGuide.SetActive(false);
        mafiaGuide.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseGuide);
        }
        
        GameManager.Instance.OnGameStarted += OnGameStarted;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= OnGameStarted;
        }
    }

    private void OnGameStarted()
    {
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying()) return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            ToggleGuide();
        }
    }

    private void ToggleGuide()
    {
        if (isOpen)
        {
            CloseGuide();
        }
        else
        {
            OpenGuide();
        }
    }

    private void OpenGuide()
    {
        if (LocalManager.Instance.IamB == false)
        {
            citizenGuide.SetActive(true);
            mafiaGuide.SetActive(false);
        }
        else
        {
            citizenGuide.SetActive(false);
            mafiaGuide.SetActive(true);
        }
        
        isOpen = true;
    }

    private void CloseGuide()
    {
        citizenGuide.SetActive(false);
        mafiaGuide.SetActive(false);
        isOpen = false;
    }
}
