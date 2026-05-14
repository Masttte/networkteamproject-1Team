using UnityEngine;
using TMPro;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

/// <summary>
/// 게임 씬 진입 후 다른 플레이어 합류를 기다리는 동안 표시되는 오버레이 UI
/// 서버(TeamManager)가 보내는 ClientRpc를 받아 UI를 통제
/// </summary>
public class GameWaitingUI : NetworkBehaviour
{
    public static GameWaitingUI Instance;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init() => Instance = null;

    [SerializeField] TMP_Text _statusText;

    private void Awake()
    {
        Instance = this;
    }

    protected override void OnNetworkPostSpawn()
    {
        _statusText.text = "플레이어 합류 대기 중...";
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Instance = null;
    }

    public void UpdateWaitingText(int current, int expected)
    {
        _statusText.text = $"플레이어 합류 대기 중... ({current}/{expected})";
    }

    public void ShowTimeoutError(int timeoutCount)
    {
        _statusText.color = Color.red;
        _statusText.text = $"씬 로드 Timeout 발생! ({timeoutCount}명)";
    }

    public async UniTaskVoid HideWaitingPanel()
    {
        _statusText.text = "<color=green>TAB</color>키를 눌러 역할을 확인하세요\n 곧 게임을 시작합니다...";
        await UniTask.Delay(3000);
        gameObject.SetActive(false);
    }
}
