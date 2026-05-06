using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using TMPro;
using Michsky.UI.Dark;

/// <summary>
/// 로비 씬의 세션 목록 UI + 방 생성/빠른참여/새로고침 컨트롤
/// </summary>
public class LobbyListUI : MonoBehaviour
{
    [SerializeField] MainPanelManager _darkUIPanelMain;
    [SerializeField] MainPanelManager _darkUIPanelMulti;
    [SerializeField] Transform _entryContainer;
    [SerializeField] LobbyEntryUI _entryPrefab;
    //[SerializeField] Button _quickJoinButton;
    //[SerializeField] Button _joinByCodeButton;
    [SerializeField] TMP_Text _statusText;
    //[SerializeField] TMP_Text _emptyListText;
    //[SerializeField] JoinByCodeDialogUI _joinByCodeDialog;
    [SerializeField] GameObject _tabButtonToHide;

    readonly List<LobbyEntryUI> _spawnedEntries = new List<LobbyEntryUI>();
    bool _isBusy;

    private void Awake()
    {
        BindEvents();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    private void Start()
    {
        RefreshLobbyList();
    }

    private void BindEvents()
    {
        //BindButtonEvents();
        BindLobbyManagerEvents();
    }

    private void UnbindEvents()
    {
        //UnbindButtonEvents();
        UnbindLobbyManagerEvents();
    }

    //private void BindButtonEvents()
    //{
    //_quickJoinButton.onClick.AddListener(OnQuickJoinClicked);
    //_joinByCodeButton.onClick.AddListener(OnJoinByCodeClicked);
    //}

    //private void UnbindButtonEvents()
    //{
    //_quickJoinButton.onClick.RemoveListener(OnQuickJoinClicked);
    //_joinByCodeButton.onClick.RemoveListener(OnJoinByCodeClicked);
    //}

    private void BindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated += OnSessionUpdated;
    }

    private void UnbindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated -= OnSessionUpdated;
    }

    public async void RefreshLobbyList()
    {
        if (_isBusy) return;
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            SetStatus("로그인 상태가 아닙니다.");
            return;
        }

        SetBusy(true);
        SetStatus("방 목록 조회 중...");
        try
        {
            IList<ISessionInfo> sessions = await LobbyManager.Instance.QuerySessionsAsync();
            PopulateEntries(sessions);
            SetStatus($"방 {sessions.Count}개 조회됨");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void PopulateEntries(IList<ISessionInfo> sessions)
    {
        ClearEntries();
        for (int i = 0; i < sessions.Count; i++)
        {
            LobbyEntryUI entry = Instantiate(_entryPrefab, _entryContainer);
            entry.Setup(sessions[i], OnEntryJoinClicked);
            _spawnedEntries.Add(entry);
        }
    }

    private void ClearEntries()
    {
        for (int i = 0; i < _spawnedEntries.Count; i++)
        {
            if (_spawnedEntries[i] != null) Destroy(_spawnedEntries[i].gameObject);
        }
        _spawnedEntries.Clear();
    }

    //private void OnJoinByCodeClicked()
    //{
    //    if (_isBusy) return;
    //    _joinByCodeDialog.Open();
    //}

    private async void OnQuickJoinClicked()
    {
        if (_isBusy) return;
        SetBusy(true);
        SetStatus("빠른 참여 중...");
        try
        {
            bool success = await LobbyManager.Instance.QuickJoinAsync();
            if (!success) SetStatus("참여할 방을 찾지 못했습니다.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnEntryJoinClicked(ISessionInfo sessionInfo)
    {
        if (_isBusy) return;
        SetBusy(true);
        LobbyManager.Instance.SetPlayerName(LobbyManager.Instance.GetPlayerName());

        SetStatus($"'{sessionInfo.Name}' 참여 중...");
        try
        {
            bool success = await LobbyManager.Instance.JoinSessionByIdAsync(sessionInfo.Id);
            if (!success) SetStatus("방 참여 실패");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        //_quickJoinButton.interactable = !busy;
        //_joinByCodeButton.interactable = !busy;
        for (int i = 0; i < _spawnedEntries.Count; i++)
        {
            if (_spawnedEntries[i] != null) _spawnedEntries[i].SetInteractable(!busy);
        }
    }

    private void OnSessionUpdated(ISession session)
    {
        if (session != null) ShowLobbyListPanel(false);
    }

    private void ShowLobbyListPanel(bool show)
    {
        if (show)
        {
            _darkUIPanelMain.OpenPanel("Multiplayer");
            TabButtonToShow();
        }
        else
        {
            _darkUIPanelMulti.OpenPanel("ROOM");
            _tabButtonToHide.SetActive(false);
            SetStatus(string.Empty);
        }
        //_lobbyListPanel.SetActive(show);
        //_roomPanel.SetActive(!show);
    }

    public void TabButtonToShow() => _tabButtonToHide.SetActive(true);

    private void SetStatus(string message)
    {
        _statusText.text = message;
    }
}
