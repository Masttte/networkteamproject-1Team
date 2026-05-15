using Battle;
using System;
using Player;
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

        public int cnt = 0;
        private float _timer;
        public float unlockTime;
        public bool IsSecondMonster;

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

            if (!IsServer) return;
            isUnlock.Value = false;

            if (_monsterPrefab == null || _monsterSpawnPoint == null) return;
            MonsterSpawn();

            _pressAction.IsPressAction += UnlockPrison;

        }

        private void OnEnable()
        {
            BattleManager.Instance.OnGameStart += OnGameStart;
        }
        private void OnDisable()
        {
            BattleManager.Instance.OnGameStart -= OnGameStart;
        }

        void OnGameStart()
        {
            cnt = 1; // 시작 시점에 설정
        }
        private void Update()
        {
            if (!IsSecondMonster || cnt != 1) return;

            _timer += Time.deltaTime;

            if (_timer >= unlockTime)
            {
                UnlockPrison();
                cnt++;
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

            UnlockRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void UnlockRpc()
        {
            if (IsServer) isUnlock.Value = true;
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
            
            var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            var input = playerObj.GetComponent<PlayerInputHandler>();

            input.DisableInput(InputCategory.Movement); // 발전기 상호작용 중에는 움직이지 못하게
            AudioManager.Instance.PlayUnlockLoop().Forget();
        }

        public void InteractStop()
        {
            _pressAction.StopInteraction();
            
            var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            var input = playerObj.GetComponent<PlayerInputHandler>();
        
            input.EnableInput(InputCategory.Movement); // 이동 복구
            AudioManager.Instance.StopUnlockLoop();
        }
    }
}
