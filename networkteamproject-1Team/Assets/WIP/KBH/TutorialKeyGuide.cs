using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SocialPlatforms;

public class TutorialKeyGuide : MonoBehaviour
{
    //[Inspector 연결 필요한것들]
    // - guidePanel         : 키 가이드 전체 패널
    // - groupTitleText     : "A 그룹 목표, B 그룹 목표"
    // - objectiveText      : 목표 설명 텍스트
    // - KeyGuideContent_A  : A그룹 키 가이드 오브젝트 (Panel)
    // - KeyGuideContent_B  : B그룹 키 가이드 오브젝트 (Panel)
    // - closeButton        : 닫기 버튼 (버튼 클릭 또는 F1 다시누르기)
    
    [Header("패널")]
    [SerializeField] private GameObject guidePanel;

    [Header("그룹별 텍스트")] 
    [SerializeField] private TextMeshProUGUI groupTitleText;
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("그룹별 키 가이드 패널 (Inspector에서 각각 연결)")] 
    [SerializeField] private GameObject KeyGuideContent_A;      // A그룹 키 목록 패널
    [SerializeField] private GameObject KeyGuideContent_B;      // B그룹 키 목록 패널
    
    [Header("닫기 버튼")]
    [SerializeField] private Button closeButton;
    
    // TODO: 본인이 속해있는 그룹을 호출해서 호출한 그룹의 가이드 보기
    // private PlayerGroup myGroup = PlayerGroup.None;
    private bool isOpen = false;
    
    // ----------------------
    private const string TITLE_A = "A그룹 목표";
    private const string OBJECTIVE_A =
        "맵에 숨겨진 발전기를 모두 수리하세요.\n" +
        "발전기를 수리해 술래로부터 생존하세요!";

    private const string TITLE_B = "B그룹 목표";
    private const string OBJECTIVE_B =
        "감옥에 갇힌 몬스터를 해방시키세요.\n" +
        "감옥에 가까이 다가가 3초간 상호작용 키를 누르면 해방됩니다.\n" +
        "발전기를 고치고 탈출하려는 생존자들을 방해하고 사살하십시오";

    private void Start()
    {
        guidePanel.SetActive(false);
        closeButton.onClick.AddListener(CloseGuide);
        
        // 게임 시작 이벤트 구독 -> 본인 팀 받아오기
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

        if (Input.GetKeyDown(KeyCode.F1))
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
            groupTitleText.text = TITLE_A;
            objectiveText.text = OBJECTIVE_A;
            KeyGuideContent_A.SetActive(true);
            KeyGuideContent_B.SetActive(false);
        }
        else
        {
            groupTitleText.text = TITLE_B;
            objectiveText.text = OBJECTIVE_B;
            KeyGuideContent_B.SetActive(true);
            KeyGuideContent_A.SetActive(false);
        }
        
        guidePanel.SetActive(true);
        isOpen = true;
    }

    private void CloseGuide()
    {
        guidePanel.SetActive(false);
        isOpen = false;
    }
}
