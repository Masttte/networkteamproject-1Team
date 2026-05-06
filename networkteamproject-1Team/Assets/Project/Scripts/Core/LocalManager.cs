using Cysharp.Threading.Tasks;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// 자동생성 싱글톤, 로컬정보 관리
public partial class LocalManager : MonoBehaviour
{
    public static LocalManager Instance;

    public event Action OnIamBSet;
    bool _iamB;
    public bool IamB
    {
        get => _iamB;
        set { _iamB = value; if (value) OnIamBSet?.Invoke(); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] // 씬 시작 전에 만들기
    private static void CreateInstance()
    {
        GameObject go = new GameObject("LocalManager");
        Instance = go.AddComponent<LocalManager>();
        DontDestroyOnLoad(go);
    }

    public virtual void Start()
    {
        if (NetworkManager.Singleton == null) return; // 테스트 씬 오류 방지

        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {

        // 자기 자신이 해제된 경우 = 서버와의 연결이 끊김 = Host 이탈
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[LocalManager] 누군가 연결이 해제되었습니다."); //서버만 로그 뜨는 중
            return;
        }
        else
        {
            Debug.Log("[LocalManager] 서버와의 연결이 끊겼습니다."); // 자신 또는 서버 연결 해제 시
            if (LobbyManager.Instance.darkUIPanelMain != null) LobbyManager.Instance.CloseDarkUI();

            LobbyManager.Instance.LeaveSessionAsync().Forget();
        }

        // TODO: 연결해제 UI 처리

        //NetworkManager.Singleton.Shutdown();
        //SceneManager.LoadScene(0);
    }
}
