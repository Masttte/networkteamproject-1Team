using Battle;
using UnityEngine;

public class UIVisibilityManager : MonoBehaviour
{
    [SerializeField] private BattleManager _battleManager;
    [SerializeField] private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        if (_battleManager != null) _battleManager.OnGameStart += ShowUI;
    }

    private void ShowUI()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    private void OnDestroy()
    {
        if (_battleManager != null) _battleManager.OnGameStart -= ShowUI;
    }
}
