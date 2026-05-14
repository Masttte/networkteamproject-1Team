using Battle;
using Monster;
using TMPro;
using Unity.Netcode;
using UnityEngine;

// Prison의 UnlockServerTime을 NetworkTime 기준으로 카운트다운하여 표시
public class NetworkTimeUI : NetworkBehaviour, INetworkUpdateSystem
{
    [SerializeField] TMP_Text _timerText;

    Prison _prison;
    float _remaining;

    private void Awake()
    {
        Prison.OnPrisonSpawned += OnPrisonSpawned;
    }

    public override void OnNetworkSpawn()
    {
        BattleManager.Instance.OnGameStart += OnGameStart;
    }

    public override void OnNetworkDespawn()
    {
        BattleManager.Instance.OnGameStart -= OnGameStart;
        this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
    }

    public override void OnDestroy()
    {
        Prison.OnPrisonSpawned -= OnPrisonSpawned;
        base.OnDestroy();
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
        this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
    }

    public void NetworkUpdate(NetworkUpdateStage updateStage)
    {
        if (updateStage != NetworkUpdateStage.Update) return;

        if (_prison.isUnlock.Value)
        {
            _timerText.text = string.Empty;
            return;
        }

        _remaining -= Time.deltaTime;
        if (_remaining <= 0)
        {
            _timerText.text = "00:00";
            return;
        }

        int totalSec = Mathf.CeilToInt(_remaining);
        int min = totalSec / 60;
        int sec = totalSec % 60;
        _timerText.text = $"{min:00}:{sec:00}";
    }
}
