using Battle;
using System;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace Monster
{
    public class Prison : NetworkBehaviour, IInteractable
    {
        [SerializeField] private GameObject _monsterPrefab;
        [SerializeField] private Transform _monsterSpawnPoint;

        private PressAction _pressAction;
        private MeshRenderer _monsterRenderer;

        [HideInInspector] public int cnt = 0;
        private float _timer;
        public float unlockTime;
        public bool IsSecondMonster;
        bool _isInteracting; // 로컬 플레이어가 현재 상호작용 중인지 여부

        public static event Action<Prison> OnPrisonSpawned;

        public NetworkVariable<bool> isUnlock = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        [SerializeField] AudioResource _openCage;
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

            isUnlock.OnValueChanged += OnIsUnlockChanged;

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
            isUnlock.OnValueChanged -= OnIsUnlockChanged;

            if (IsServer && _pressAction != null)
            {
                _pressAction.IsPressAction -= UnlockPrison;
            }
        }

        private void OnIsUnlockChanged(bool prev, bool next)
        {
            if (!next) return;

            // 상호작용 중이던 로컬 플레이어만 이동 복구 + 오디오 정지
            if (_isInteracting)
                StopLocalInteract();
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
            gameObject.layer = 2;
            _monsterRenderer.enabled = false;
            AudioManager.Instance.PlaySfxWet(_openCage, transform.position);
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
            _isInteracting = true;

            var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            var input = playerObj.GetComponent<PlayerInputHandler>();

            input.DisableInput(InputCategory.Movement); // 발전기 상호작용 중에는 움직이지 못하게
            AudioManager.Instance.PlayUnlockLoop().Forget();
        }

        public void InteractStop()
        {
            if (_isInteracting)
                _pressAction.StopInteraction();

            StopLocalInteract();
        }

        void StopLocalInteract()
        {
            if (!_isInteracting) return;
            _isInteracting = false;

            var playerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
            var input = playerObj.GetComponent<PlayerInputHandler>();
            input.EnableInput(InputCategory.Movement);

            AudioManager.Instance.StopUnlockLoop();
        }
    }
}
