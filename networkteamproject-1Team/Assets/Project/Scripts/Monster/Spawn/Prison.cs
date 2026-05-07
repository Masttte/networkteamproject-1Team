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
            
            // ======추가======
            if (IsServer)
            {
                _pressAction.IsPressAction += UnlockPrison;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && _pressAction != null)
            {
                _pressAction.IsPressAction -= UnlockPrison;
            }
        }

        private void UnlockPrison()
        {
            if (Unlocked) return;
            
            Unlocked = true;
            SyncUnlockClientRpc();
        }

        // 클라이언트들도 true로 바꿔주는 메서드
        [ClientRpc]
        private void SyncUnlockClientRpc()
        {
            Unlocked = true;
        }
        // ======추가======

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
