using Battle;
using Monster;
using TMPro;
using UnityEngine;

// Prison의 unlockTime을 기준으로 카운트다운하여 표시
public class NetworkTimeUI : MonoBehaviour
{
    [SerializeField] TMP_Text _timerText;

    Prison _prison;
    float _remaining;
    public bool running;

    private void Awake()
    {
        Prison.OnPrisonSpawned += OnPrisonSpawned;
    }

    private void OnEnable()
    {
        BattleManager.Instance.OnGameStart += OnGameStart;
    }
    private void OnDisable()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnGameStart -= OnGameStart;
    }

    public void OnDestroy()
    {
        Prison.OnPrisonSpawned -= OnPrisonSpawned;
    }

    private void OnPrisonSpawned(Prison prison)
    {
        if (prison.IsSecondMonster)
        {
            _prison = prison;
            _remaining = _prison.unlockTime;
        }
    }

    void OnGameStart()
    {
        running = true;
    }


    private void Update()
    {
        if (!running) return;

        _remaining -= Time.deltaTime;
        if (_remaining <= 0 || _prison.isUnlock.Value)
        {
            _timerText.text = string.Empty;
            running = false;
            return;
        }

        int totalSec = Mathf.CeilToInt(_remaining);
        int min = totalSec / 60;
        int sec = totalSec % 60;
        _timerText.text = $"{min:00}:{sec:00}";
    }
}
