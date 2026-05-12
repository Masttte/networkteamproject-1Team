using System;
using System.Collections;
using Battle;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WinPanel : MonoBehaviour
{
    public CanvasGroup citizenWinPanel;     // 시민팀 승리 패널
    public CanvasGroup mafiaWinPanel;       // 마피아 승리 패널
    public float fadeDuration = 2.0f;       // 페이드인 되는시간

    private void OnEnable()
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
    }

    private void GoToLobby()
    {
        LobbyManager.Instance.LeaveSessionAsync().Forget();
        SceneLoader.LoadLocal(0);
    }

}
