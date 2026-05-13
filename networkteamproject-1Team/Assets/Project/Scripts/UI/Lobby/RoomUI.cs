using Michsky.UI.Dark;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using VFX;

/// <summary>
/// 룸(방) 내부 UI. 플레이어 슬롯 + 레디/나가기 버튼 + 상태 메시지
/// </summary>
public class RoomUI : NetworkBehaviour
{
    [SerializeField] LobbySettings _settings;
    [SerializeField] TMP_Text _roomNameText;
    [SerializeField] TMP_Text[] _lobbyCodeTexts;
    [SerializeField] List<RoomPlayerSlotUI> _playerSlots = new List<RoomPlayerSlotUI>();
    [SerializeField] Button _readyButton;
    [SerializeField] TMP_Text[] _readyButtonLabels;
    [SerializeField] TMP_Text _statusText;

    [SerializeField] Sprite[] _sourceImages;
    [SerializeField] MainPanelManager _darkUIPanelMulti;
    [SerializeField] GameObject _panelToHideAtGame;

    bool _isLocalPlayerReady;
    bool _isProcessingReady;
    string _sessionCode;

    private void OnEnable()
    {
        AssignSlotBackgrounds();
        BindEvents();
        ResetInteractables();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void ResetInteractables()
    {
        // 이전 진입에서 OnLeaveClicked / OnGameStarting에 의해 false로 남아있을 수 있어 재진입 시 복구
        _readyButton.interactable = true;
        _isProcessingReady = false;
        _isLocalPlayerReady = false;
    }

    public override void OnNetworkSpawn()
    {
        LobbyManager.Instance.OnGameStarting += OnGameStarting;
    }
    public override void OnNetworkDespawn()
    {
        LobbyManager.Instance.OnGameStarting -= OnGameStarting;
    }


    private void BindEvents()
    {
        LobbyManager.Instance.OnSessionUpdated += Refresh;
        LobbyManager.Instance.OnRestartCooldownEnded += RefreshReadyButton;
    }

    private void UnbindEvents()
    {
        LobbyManager.Instance.OnSessionUpdated -= Refresh;
        LobbyManager.Instance.OnRestartCooldownEnded -= RefreshReadyButton;
    }

    private void AssignSlotBackgrounds()
    {
        for (int i = 0; i < _playerSlots.Count; i++)
        {
            Image img = _playerSlots[i].GetComponent<Image>();
            img.sprite = _sourceImages[i % _sourceImages.Length];
        }
    }

    private void Refresh(ISession session)
    {
        if (session == null) return;
        _roomNameText.text = session.Name;
        for (int i = 0; i < _lobbyCodeTexts.Length; i++)
        {
            _lobbyCodeTexts[i].text = $"Join Code: {session.Code}";
        }
        _sessionCode = session.Code;
        RefreshPlayerSlots(session);
        RefreshReadyButton();
        RefreshStatusText(session);
    }

    private void RefreshPlayerSlots(ISession session)
    {
        bool isLocalHost = session.CurrentPlayer != null && session.CurrentPlayer.Id == session.Host;
        if (!isLocalHost && session.CurrentPlayer != null)
        {
            string readyValue = LobbyManager.GetPlayerProperty(session.CurrentPlayer, LobbyConstants.KEY_PLAYER_READY);
            _isLocalPlayerReady = readyValue == LobbyConstants.VALUE_TRUE;
        }
        else
        {
            _isLocalPlayerReady = false;
        }

        for (int i = 0; i < _playerSlots.Count; i++)
        {
            if (i < session.Players.Count)
            {
                ApplyPlayerToSlot(session, i);
            }
            else
            {
                _playerSlots[i].SetEmpty();
            }
        }
    }
    private void ApplyPlayerToSlot(ISession session, int index)
    {
        IReadOnlyPlayer player = session.Players[index];
        string playerName = LobbyManager.GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_NAME) ?? "Player";
        string readyValue = LobbyManager.GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_READY);
        bool isReady = readyValue == LobbyConstants.VALUE_TRUE;
        bool isHost = player.Id == session.Host;
        _playerSlots[index].gameObject.SetActive(true);
        _playerSlots[index].SetPlayer(playerName, isReady, isHost);
    }

    private void RefreshReadyButton()
    {
        bool isHost = LobbyManager.Instance.IsHost;

        for (int i = 0; i < _readyButtonLabels.Length; i++)
        {
            _readyButtonLabels[i].text = isHost
                ? "START" : (_isLocalPlayerReady ? "CANCEL" : "READY");
        }

        if (!_isProcessingReady)
        {
            _readyButton.interactable = !isHost || LobbyManager.Instance.CanHostStartGame;
        }
    }

    private void RefreshStatusText(ISession session)
    {
        int total = session.PlayerCount;
        int nonHostTotal = GetNonHostPlayerCount(session);
        int nonHostReady = GetNonHostReadyCount(session);

        if (total < _settings.MinPlayersToStart)
        {
            _statusText.text = $"최소 {_settings.MinPlayersToStart}명 필요 ({total}/{_settings.MaxPlayers})";
        }
        else if (nonHostReady < nonHostTotal)
        {
            _statusText.text = $"다른 플레이어 레디 대기 ({nonHostReady}/{nonHostTotal})";
        }
        else
        {
            _statusText.text = "호스트가 게임을 시작할 수 있습니다";
        }
    }

    private int GetNonHostPlayerCount(ISession session)
    {
        int count = 0;
        for (int i = 0; i < session.Players.Count; i++)
        {
            if (session.Players[i].Id != session.Host) count++;
        }
        return count;
    }

    private int GetNonHostReadyCount(ISession session)
    {
        int count = 0;
        for (int i = 0; i < session.Players.Count; i++)
        {
            IReadOnlyPlayer player = session.Players[i];
            if (player.Id == session.Host) continue;
            string ready = LobbyManager.GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_READY);
            if (ready == LobbyConstants.VALUE_TRUE) count++;
        }
        return count;
    }

    public async void OnReadyClicked()
    {
        if (_isProcessingReady) return;
        _isProcessingReady = true;
        _readyButton.interactable = false;
        try
        {
            if (LobbyManager.Instance.IsHost)
            {
                await LobbyManager.Instance.TryStartGameAsHostAsync();
            }
            else
            {
                await LobbyManager.Instance.SetReadyAsync(!_isLocalPlayerReady);
            }
        }
        finally
        {
            _isProcessingReady = false;
            RefreshReadyButton();
        }
    }

    public async void OnLeaveClicked()
    {
        for (int i = 0; i < _playerSlots.Count; i++)
        {
            _playerSlots[i].SetEmpty();
        }
        await LobbyManager.Instance.LeaveSessionAsync();
    }

    void OnGameStarting()
    {
        GameStartingClientRpc();
    }

    [ClientRpc]
    void GameStartingClientRpc()
    {
        _statusText.text = "게임에 입장합니다...";
        _readyButton.interactable = false;
        _panelToHideAtGame.SetActive(false);
        _darkUIPanelMulti.OpenPanel("LOADING");
        TransitionControl.Instance.PlayIn();
        TransitionControl.Instance.PlaySound();
    }


    public void CopyLobbyCodeToClipboard()
    {
        if (!string.IsNullOrEmpty(_sessionCode))
        {
            GUIUtility.systemCopyBuffer = _sessionCode;
            _statusText.text = "코드가 클립보드에 복사되었습니다";
        }
    }
}
