using Cysharp.Threading.Tasks;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    #region 두근두근 의존성 주입하려고 만든 Instance...
    public static PauseMenu Instance;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init() => Instance = null;
    private void Awake() => Instance = this;
    private void OnDestroy() => Instance = null;

    // 두근두근 의존성 주입 추가
    public void Inject(PlayerInputHandler inputHandler)
    {
        _playerInputHandler = inputHandler;
    }
    #endregion
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

    TutorialKeyGuide _TKG; //추가


    private void Start()
    {
        resumeButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        yesButton.onClick.AddListener(OnConfirmYes);
        noButton.onClick.AddListener(OnConfirmNo);

        pauseMenu.SetActive(false);
        confirmPanel.SetActive(false);

        _TKG = GetComponent<TutorialKeyGuide>(); //추가
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            _TKG.CloseGuide(); //추가

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
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
    }

    private void OnConfirmNo()
    {
        confirmPanel.SetActive(false);
    }
}
