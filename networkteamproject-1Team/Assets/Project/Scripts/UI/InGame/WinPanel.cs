using Battle;
using Cysharp.Threading.Tasks;
using Player;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinPanel : MonoBehaviour
{
    #region 두근두근 의존성 주입하려고 만든 Instance...
    public static WinPanel Instance;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init() => Instance = null;
    private void Awake() => Instance = this;
    private void OnDestroy() => Instance = null;

    private PlayerInputHandler _playerInputHandler;
    private const InputCategory PauseBlock = InputCategory.All;
    // 두근두근 의존성 주입 추가
    public void Inject(PlayerInputHandler inputHandler)
    {
        _playerInputHandler = inputHandler;
    }
    #endregion
    public CanvasGroup citizenWinPanel;     // 시민팀 승리 패널
    public TMP_Text citizenWinText;
    public CanvasGroup mafiaWinPanel;       // 마피아 승리 패널
    public TMP_Text mafiaWinText;
    public float fadeDuration = 2.0f;       // 페이드인 되는시간
    [SerializeField] Button _restartButtonA;
    [SerializeField] Button _restartButtonB; // 호스트만 활성화
    private void Start()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnGameEnd += HandleGameEnd;
        }
    }

    private void OnDisable()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnGameEnd -= HandleGameEnd;
        }
    }

    private void HandleGameEnd(TeamType winner)
    {
        if (winner == TeamType.A)
        {
            StartCoroutine(FadeInRoutine(citizenWinPanel));
            if (LocalManager.Instance.IamB)
            {
                citizenWinText.text = $"<color=red>당신은 패배 하였습니다.</color>";
            }
            else
                citizenWinText.text = $"<color=green>당신은 승리 하였습니다!</color>";
        }
        else if (winner == TeamType.B)
        {
            StartCoroutine(FadeInRoutine(mafiaWinPanel));
            if (LocalManager.Instance.IamB)
            {
                mafiaWinText.text = $"<color=green>당신은 승리 하였습니다!</color>";
            }
            else
                mafiaWinText.text = $"<color=red>당신은 패배 하였습니다.</color>";
        }
    }

    private IEnumerator FadeInRoutine(CanvasGroup target)
    {
        target.gameObject.SetActive(true);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            target.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        target.alpha = 1f;

        // 호스트만 재시작 버튼 활성화
        if (_restartButtonA != null) _restartButtonA.interactable = NetworkManager.Singleton.IsServer;
        if (_restartButtonB != null) _restartButtonB.interactable = NetworkManager.Singleton.IsServer;

        // 결과창 나오면 커서락 풀기
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 플레이어 인풋 막기
        _playerInputHandler.DisableInput(PauseBlock);
    }

    public void GoToLobby()
    {
        LobbyManager.Instance.LeaveSessionAsync().Forget();
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
    }

    // 호스트만 호출. 버튼에 연결
    public void RestartGame()
    {
        BattleManager.Instance.RestartGame();
    }
}
