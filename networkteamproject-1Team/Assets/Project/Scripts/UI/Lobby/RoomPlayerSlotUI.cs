using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 룸 내 플레이어 1명을 표시하는 슬롯 UI
/// </summary>
public class RoomPlayerSlotUI : MonoBehaviour
{
    [SerializeField] Image _ReadyIcon;
    [SerializeField] TMP_Text _playerNameText;

    /// <summary>
    /// 빈 슬롯 상태로 표시
    /// </summary>
    public void SetEmpty()
    {
        _playerNameText.text = "-";
        if (_ReadyIcon != null) _ReadyIcon.enabled = false;
        gameObject.SetActive(false); // 비어있는 슬롯은 화면에서 숨김
    }

    /// <summary>
    /// 실제 플레이어 정보로 슬롯 채움
    /// </summary>
    /// <param name="playerName">플레이어 이름</param>
    /// <param name="isReady">레디 상태</param>
    /// <param name="isHost">호스트 여부</param>
    public void SetPlayer(string playerName, bool isReady, bool isHost)
    {
        _playerNameText.text = playerName;
        if (!isHost) _ReadyIcon.enabled = isReady ? true : false;
    }
}
