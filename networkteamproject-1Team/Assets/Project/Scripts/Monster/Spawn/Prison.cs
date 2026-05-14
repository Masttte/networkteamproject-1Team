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
        private MeshRenderer _monsterRenderer;

        private int _cnt;
        private float _timer;
        public float unlockTime;
        public bool IsSecondMonster { get; set; }

        public static event Action<Prison> OnPrisonSpawned;

        public NetworkVariable<bool> isUnlock = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            OnPrisonSpawned?.Invoke(this); // 추가
            _pressAction = GetComponent<PressAction>();
            _monsterRenderer = GetComponentInChildren<MeshRenderer>();
            IsSecondMonster = false;
            _cnt = 0;

            if (!IsServer) return;
            isUnlock.Value = false;

            if (_monsterPrefab == null || _monsterSpawnPoint == null) return;
            MonsterSpawn();

            _pressAction.IsPressAction += UnlockPrison;

        }

        private void Update()
        {
            if (!IsSecondMonster || _cnt == 1) return;
            
            _timer += Time.deltaTime;

            if (_timer >= unlockTime)
            {
                UnlockPrison();
                _cnt++;
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
            if (isUnlock.Value)
            {
                Debug.Log("이미 몬스터가 풀려났다");
                return;
            }

            if (IsServer)
            {
                SyncUnlock();
                isUnlock.Value = true;
            }
            else
            {
                UnlockServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UnlockServerRpc()
        {
            SyncUnlock();
        }

        private void SyncUnlock()
        {
            Debug.Log("<color=red> 괴물이 풀려났다..! </color>");
            _monsterRenderer.enabled = false;
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
