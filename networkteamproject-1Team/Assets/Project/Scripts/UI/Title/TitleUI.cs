using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 타이틀 씬 UI. 플레이어 이름 입력 후 UGS 인증 후 로비 씬으로 전환
/// </summary>
public class TitleUI : MonoBehaviour
{
    [SerializeField] TMP_Text _statusText;

    private async void Start()
    {
        SetStatus("로그인 중...");
        try
        {
            await AuthService.InitializeAsync();
            SetStatus("로그인 완료");
        }
        catch (Exception e)
        {
            SetStatus("로그인 실패");
            Debug.LogError($"TitleUI: 로그인 실패: {e.Message}");
        }
    }

    private void SetStatus(string message)
    {
        _statusText.text = message;
    }
}
