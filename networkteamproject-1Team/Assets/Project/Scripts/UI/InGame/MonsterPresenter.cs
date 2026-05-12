using System;
using Battle;
using Monster;
using Unity.Netcode;
using UnityEngine;

public class MonsterPresenter : MonoBehaviour
{
    private Prison _prison;
    [SerializeField] private MonsterView _view;

    private void Awake()
    {
        Prison.OnPrisonSpawned += HandlePrisonSpawned;
    }
    
    private void OnDestroy()
    {
        Prison.OnPrisonSpawned -= HandlePrisonSpawned;

        if (_prison != null)
        {
            _prison.isUnlock.OnValueChanged -= OnMonsterUnlocked;
        }
    }

    private void HandlePrisonSpawned(Prison prisonSpawned)
    {
        Debug.Log(prisonSpawned.isUnlock.Value);
        _prison = prisonSpawned;
        _prison.isUnlock.OnValueChanged += OnMonsterUnlocked;
    }

    private void OnMonsterUnlocked(bool previousValue, bool newValue)
    {
        _view.NotifyMonster(newValue);
    }
}
