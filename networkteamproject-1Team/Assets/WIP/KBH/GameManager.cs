using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using WIP.KYB.Scripts;

// [게임 상태]
public enum GameState
{
    Lobby,          // 플레이어 접속 대기
    GroupAssign,    // A/B 그룹 랜덤 배정
    Playing,        // 게임 진행 중
    GameOver        // 게임 종료 (게임 결과 화면)
}

// [플레이어 그룹 구분]
public enum PlayerGroup
{
    None,       // 미배정(초기 상태)
    Killer,     // 킬러 (1명)
    Survivor    // 생존자 (3~4명)
}

// [플레이어 정보 컨테이너]
[System.Serializable]
public class PlayerInfo
{
    public string playerId;             // 네트워크 플레이어 ID
    public string playerName;           // 표시 이름
    public PlayerGroup playerGroup;     // 배정된 그룹
    public int hitStack;                // 현재 피격 스택 (최대 2)
    public bool isDead;                 // 사망 여부
    public bool isEscaped;              // 탈출 성공 여부

    public PlayerInfo(string id, string name)
    {
        playerId = id;
        playerName = name;
        playerGroup = PlayerGroup.None;
        hitStack = 0;
        isDead = false;
        isEscaped = false;
    }
}

public class GameManager : NetworkBehaviour
{
    // ---------------------------------
    // [싱글톤 패턴]
    // 게임 어디서든 접근 가능하게 Instance로 만들기
    // ---------------------------------

    public static GameManager Instance { get ; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // 씬 변경에도 파괴 안되게 하기
    }


    // [현재 게임 상태]
    public GameState CurrentState { get; private set; } = GameState.Lobby;

    // [이벤트 시스템]
    // 다른 매니저들이 구독해서 상태 변화 알게 해주기
    // 예) UIManager가 OnGameStarted에 구독 -> 게임 시작 시 HUD 표시
    public event System.Action OnLobbyUpdate;               // 로비 플레이어 목록 변경
    public event System.Action OnGroupAssigned;             // 팀 A/B 배정 완료
    public event System.Action OnGameStarted;               // 게임 시작   
    public event System.Action<float> OnTimerUpdated;       // 타이머 갱신 (남은 초)
    public event System.Action OnTimeExpired;               // 시간 초과 -> 강력 몬스터
    // public event System.Action<PlayerInfo> OnPlayerHit;     // 플레이어 피격
    // public event System.Action<PlayerInfo> OnPlayerDead;    // 플레이어 사망
    public event System.Action<int, int> OnMissionUpdated;  // 미션 진행 (완료수, 전체수)
    public event System.Action<bool> OnGameOver;            // bool: true = 생존자 승, false = 킬러 승
    
    // [게임 설정값]
    // 인스펙터에서 확인 가능하게 하기
    [Header("게임 설정")] 
    [SerializeField] private int totalSurvivors = 3;      // 생존자 수
    [SerializeField] private int generatorsRequired = 5;  // 필요한 발전기 수
    [SerializeField] private float gameStartDelay = 3f;   // 게임 시작 전 카운트 다운
    
    // [내부 상태 추적 변수]
    private int aliveSurvivors;         // 현재 생존 중인 생존자 수
    private int repairedGenerators;     // 수리된 발전기 수
    private bool doorsOpened;           // 탈출구 문이 열렸는가
    private int escapedSurvivors;       // 탈출에 성공한 생존자 수
    
    // [게임 시작 진입점]
    // 로비 씬에서 "게임 시작" 버튼을 누르면 해당 함수 호출
    public void StartGame()
    {
        if (CurrentState != GameState.Lobby) return; // 중복 호출 방지

        StartCoroutine(GameStartSequence());
    }

    private IEnumerator GameStartSequence()
    {
        Debug.Log("게임 시작 카운트 다운...");
        
        // 카운트 다운
        yield return new WaitForSeconds(gameStartDelay);
        
        // 상태 초기화
        aliveSurvivors = totalSurvivors;
        repairedGenerators = 0;
        doorsOpened = false;
        escapedSurvivors = 0;
        
        // 상태를 Playing으로 전환
        ChangeState(GameState.Lobby);
        
        // 등록된 모든 시스템에게 "게임 시작!" 알림
        // PlayerManager, UIManager 등이 이 신호를 받아서 각자 초기화 함
        OnGameStarted?.Invoke();
        
        Debug.Log("게임 시작!");
    }

    // [게임 중 호출되는 함수들]
    
    // 생존자가 사망했을 때
    public void OnSurvivorDowned()
    {
        if (CurrentState != GameState.Playing) return;

        aliveSurvivors--;
        Debug.Log($"생존자 사망, 남은 생존자: {aliveSurvivors}");

        CheckWinCondition();
    }

    // 발전기 수리 완료됐을 때 -> GeneratorSystem이 호출
    public void OnGeneratorRepaired()
    {
        if (CurrentState != GameState.Playing) return;
        
        repairedGenerators++;
        Debug.Log($"발전기 수리 완료:  {repairedGenerators}/{generatorsRequired}");
        
        // 발전기를 모두 수리하면 문 열기 가능 상태로 전환
        if (repairedGenerators >= generatorsRequired)
        {
            OpenExitDoors();
        }
    }
    
    // 생존자가 탈출에 성공했을 때 -> DoorSystem 또는 EscapeZone이 호출
    public void OnSurvivorEscaped()
    {
        if (CurrentState != GameState.Playing) return;
        
        escapedSurvivors++;
        aliveSurvivors--;
        Debug.Log($"생존자 탈출 성공! 탈출 인원: {escapedSurvivors}");

        CheckWinCondition();
    }
    
    // [내부 로직 함수들]
    private void OpenExitDoors()
    {
        doorsOpened = true;
        Debug.Log("탈출구 문 활성화!");
        // TODO: DoorManager.Instance.ActivateDoors(); 등으로 연결
    }
    
    // 승패 조건 체크 -> 매 이벤트마다 호출됨
    private void CheckWinCondition()
    {
        // 킬러 승리 조건: 생존자가 한 명도 안 남음 (탈출 포함 전원 제거)
        if (aliveSurvivors <= 0)
        {
            bool survivorsWin = escapedSurvivors > 0;       // 탈출자가 1명이라도 있으면 생존자 부분 승리
            
            EndGame(survivorsWin);
            return;
        }
        
        // 생존자 완전 승리 조건: 모든 생존자가 탈출
        if (escapedSurvivors >= totalSurvivors)
        {
            EndGame(true);
        }
    }

    private void EndGame(bool survivorsWin)
    {
        if (CurrentState == GameState.GameOver) return;     // 중복 방지
        
        ChangeState(GameState.GameOver);

        string result = survivorsWin ? "생존자 승리!" : "킬러 승리";
        Debug.Log($"게임 종료 - {result}");
        
        // 등록된 모든 시스템에게 "게임 종료!" 알림
        // -> UIManager가 결과 화면 표시
        OnGameOver?.Invoke(survivorsWin);
    }
    
    // 상태 전환을 한 곳에서 관리 (디버그 용이)
    private void ChangeState(GameState newState)
    {
        Debug.Log($"[GameState] {CurrentState} -> {newState}");
        CurrentState = newState;
    }
    
    // [게임 재시작]
    // GameOver 화면에서 "다시하기" 버튼이 이 함수를 호출
    public void RestartGame()
    {
        ChangeState(GameState.Lobby);
        // TODO: SceneManager.LoadScene("GameScene"); 으로 씬 리로드
    }
    
    // 자주 사용될 유틸리티
    
    // 현재 게임이 진행 중인지 확인 (입력, AI등을 막을 때 사용)
    public bool IsGamePlaying() => CurrentState == GameState.Playing;
    
    // 남은 생존자 수 조회
    public int GetAliveSurvivors() => aliveSurvivors;
    
    // 수리된 발전기 수 조회
    public int GetRepairedGenerators() => repairedGenerators;
}
// [상황별 사용] (다들 알고 계실거같은데 제가 잘 모르고 어려워서 일부러 남길게용...)
    //
    // 1. 게임 시작 버튼 연결:
    //    GameManager.Instance.StartGame();
    //
    // 2. 발전기 수리 완료 시:
    //    GameManager.Instance.OnGeneratorRepaired();
    //
    // 3. 생존자 사망 시:
    //    GameManager.Instance.OnSurvivorDowned();
    //
    // 4. 생존자 탈출 시:
    //    GameManager.Instance.OnSurvivorEscaped();
    //
    // 5. 게임 시작 이벤트 구독 (UIManager 에서):
    //    void Start()
    //    {
    //          GameManager.Instance.OnGameStarted += ShowHUD;
    //    }
    //    void ShowHUD() {...}