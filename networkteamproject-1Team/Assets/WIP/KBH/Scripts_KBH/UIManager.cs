using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 싱글톤 (GameManager와 동일한 패턴)
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        Instance = this;
    }
    
    // [UI 오브젝트 연결]
    // 인스펙터에서 각 UI 오브젝트를 드래그로 연결
    
    [Header("HUD (게임 중 화면)")]
    [SerializeField] private GameObject hudPanel;                // 발전기, 생존자 표시 패널
    [SerializeField] private TextMeshProUGUI generatorCountText; // "발전기: 3/5"
    [SerializeField] private TextMeshProUGUI survivorCountText;  // "생존자: 3명"

    [Header("결과 화면")]
    [SerializeField] private GameObject resultPanel;             // 결과창 패널 
    [SerializeField] private TextMeshProUGUI resultTitleText;    // "생존자 승리!" or "킬러 승리!"
    [SerializeField] private Button restartButton;               // 다시하기 버튼

    [Header("대기 화면")]
    [SerializeField] private GameObject waitingPanel;            // "플레이어 대기중..."

    // [이벤트 구독]
    private void Start()
    {
        // GameManager 이벤트 에 내 함수들을 구독
        GameManager.Instance.OnGameStarted += HandleGameStarted;
        GameManager.Instance.OnGameOver += HandleGameOver;
        
        // 초기 상태: 대기 화면만 표시
        ShowWaitingScreen();
    }
    
    // [오브젝트가 파괴될 때 구독 해제]
    private void OnDestroy()
    {
        if  (Instance == this) Instance = null;
        GameManager.Instance.OnGameStarted -= HandleGameStarted;
        GameManager.Instance.OnGameOver -= HandleGameOver;
    }

    // GameManager.OnGameStarted 이벤트가 오면 자동 호출
    private void HandleGameStarted()
    {
        waitingPanel.SetActive(false);  // 대기 화면 숨김
        resultPanel.SetActive(false);   // 결과 화면 숨김
        hudPanel.SetActive(true);       // HUD 표시

        UpdateGeneratorUI(0, 5);        // 초기값 표시 "발전기 0/5"
        UpdateSurvivorUI(4);            // 초기값 표시 "생존자 4명"

        Debug.Log("[UIManager] HUD 표시 완료");
    }
    
    // GameManager.OnGameOver 이벤트에 자동 호출
    // survivorsWin: true=생존자 승, false=킬러 승
    private void HandleGameOver(bool survivorsWin)
    {
        hudPanel.SetActive(false); // HUD 숨김
        resultPanel.SetActive(true); // 결과창 표시

        if (survivorsWin)
        {
            resultTitleText.text = "생존자 승리!";
            resultTitleText.color = Color.cyan;
        }
        else
        {
            resultTitleText.text = "킬러 승리!";
            resultTitleText.color =  Color.red;
        }

        Debug.Log("[UIManager] 결과 화면 표시 완료");
    }
    
    // [HUD 업데이트 함수]
    
    // 발전기 현황 - GeneratorSystem 
    public void UpdateGeneratorUI(int repaired, int total)
    {
        if (generatorCountText != null)
        {
            generatorCountText.text = $"발전기: {repaired}/{total}";
        }
    }
    
    // 생존자 현황 - PlayerManager
    public void UpdateSurvivorUI(int aliveCount)
    {
        if (survivorCountText != null)
        {
            survivorCountText.text = $"생존자: {aliveCount}명";
        }
    }
    
    // [버튼 이벤트]
    // 결과창의 "다시하기" 버튼에 연결
    public void OnRestartButton()
    {
        GameManager.Instance.RestartGame();
    }

    private void ShowWaitingScreen()
    {
        waitingPanel.SetActive(true);
        hudPanel.SetActive(false);
        resultPanel.SetActive(false);
    }
}
