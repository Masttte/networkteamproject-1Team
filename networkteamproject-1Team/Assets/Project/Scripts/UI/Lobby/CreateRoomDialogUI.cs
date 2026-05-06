using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

/// <summary>
/// 방 생성 팝업. 방 이름만 입력받아 LobbyManager로 생성 요청
/// </summary>
public class CreateRoomDialogUI : MonoBehaviour
{
    [SerializeField] TMP_InputField _roomNameInput;
    [SerializeField] TMP_Text _warningText;
    [SerializeField] Button _confirmButton;

    private void OnEnable()
    {
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindButtonEvents();
    }

    private void BindButtonEvents()
    {
        _confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    private void UnbindButtonEvents()
    {
        _confirmButton.onClick.RemoveListener(OnConfirmClicked);
    }

    private async void OnConfirmClicked()
    {
        _confirmButton.interactable = false;
        LobbyManager.Instance.SetPlayerName(LobbyManager.Instance.GetPlayerName());

        string roomName = _roomNameInput.text;
        if (string.IsNullOrWhiteSpace(roomName))
        {
            roomName = $"{LobbyManager.Instance.PlayerName}'s Room";
        }

        SetWarning("방 생성 중...");
        bool success = await LobbyManager.Instance.CreateSessionAsync(roomName);

        if (!success)
        {
            SetWarning("방 생성 실패. 다시 시도하세요.");
        }
        await UniTask.Delay(1000);
        _confirmButton.interactable = true;
    }

    private void SetWarning(string message)
    {
        _warningText.text = message;
    }
}
