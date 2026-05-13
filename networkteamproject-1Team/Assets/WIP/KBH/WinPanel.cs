using Battle;
using Cysharp.Threading.Tasks;
using Player;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public CanvasGroup mafiaWinPanel;       // 마피아 승리 패널
    public float fadeDuration = 2.0f;       // 페이드인 되는시간
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
        }
        else if (winner == TeamType.B)
        {
            StartCoroutine(FadeInRoutine(mafiaWinPanel));
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

}
