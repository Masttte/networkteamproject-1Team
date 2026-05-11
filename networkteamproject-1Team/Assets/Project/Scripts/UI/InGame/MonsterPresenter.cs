using System;
using Battle;
using Monster;
using Unity.Netcode;
using UnityEngine;

public class MonsterPresenter : NetworkBehaviour
{
    private Prison _prison;
    [SerializeField] private MonsterView _view;

    public override void OnNetworkSpawn()
    {
        Debug.Log("1");
        Prison.OnPrisonSpawned += HandlePrisonSpawned;
    }
    
    public override void OnNetworkDespawn()
    {
        Prison.OnPrisonSpawned -= HandlePrisonSpawned;

        if (_prison != null)
        {
            _prison.isUnlocked.OnValueChanged -= OnMonsterUnlocked;
        }
    }

    private void HandlePrisonSpawned(Prison prisonSpawned)
    {
        _prison = prisonSpawned;
        _prison.isUnlocked.OnValueChanged += OnMonsterUnlocked;
    }

    private void OnMonsterUnlocked(bool previousValue, bool newValue)
    {
        if (previousValue == false && newValue == true)
        {
            _view.NotifyMonster(newValue);
        }
    }
}
