using System;
using Unity.Netcode;
using UnityEngine;

namespace Monster
{
    public class Prison : NetworkBehaviour, IInteractable
    {
        [SerializeField] private GameObject _monsterPrefab;
        [SerializeField] private Transform _monsterSpawnPoint;
        
        private PressAction _pressAction;
        public bool Unlocked { get; private set; }

        public override void OnNetworkSpawn()
        {
            _pressAction = GetComponent<PressAction>();
            Unlocked = false;
            
            if (_monsterPrefab == null || _monsterSpawnPoint == null) return;
            MonsterSpawn();
        }

        private void MonsterSpawn()
        {
            GameObject monster = Instantiate(_monsterPrefab, _monsterSpawnPoint.position, _monsterSpawnPoint.rotation);
            monster.GetComponent<NetworkObject>().Spawn();
            monster.GetComponent<MonsterController>().PrisonSet(this);
        }

        public void InteractStart()
        {
            _pressAction.StartInteraction();
        }

        public void InteractStop()
        {
            _pressAction.StopInteraction();
        }
    }
}
