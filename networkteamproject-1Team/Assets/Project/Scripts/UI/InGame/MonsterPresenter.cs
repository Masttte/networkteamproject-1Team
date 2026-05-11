using System;
using Monster;
using Unity.Netcode;
using UnityEngine;

public class MonsterPresenter : NetworkBehaviour
{
    [SerializeField] private Prison _monster;
    [SerializeField] private MonsterView _view;

    public override void OnNetworkSpawn()
    {
        _monster.isUnlocked.OnValueChanged += OnMonsterUnlocked;
    }
    
    public override void OnNetworkDespawn()
    {
        _monster.isUnlocked.OnValueChanged -= OnMonsterUnlocked;
    }

    private void OnMonsterUnlocked(bool previousValue, bool newValue)
    {
        if (previousValue == false && newValue == true)
        {
            _view.NotifyMonster(newValue);
        }
    }
}
