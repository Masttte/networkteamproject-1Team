using System.Threading;
using Battle;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace Monster
{
    // 몬스터 연출 및 사운드 담당
    public class MonsterCinema : NetworkBehaviour
    {
        [Header("Avatars")]
        [SerializeField] GameObject _normalModel;
        [SerializeField] GameObject _monsterModel;
        [SerializeField] Avatar _normalAvatar;
        [SerializeField] Avatar _monsterAvatar;
        Animator _rootAnimator;

        [Header("Audio")]
        [SerializeField] bool _isHelpLoop = true; // locked 대사 루프 여부
        [SerializeField] AudioResource _locked;
        [SerializeField] AudioResource _talk;
        [SerializeField] AudioResource _threat;
        [SerializeField] int _threatAudioMs = 23000; // 위협 대사 주기
        AudioSource _source;

        [Header("Audio Timing")]
        [SerializeField] float _lockedIntervalMin = 7f;
        [SerializeField] float _lockedIntervalMax = 14f;
        [SerializeField] float _threatRange = 5f;
        public float talkRange = 2.49f;

        NetworkVariable<bool> _isTalked = new NetworkVariable<bool>(false);
        MonsterController _monsterController;


        bool _wasBInRange;
        bool _wasAInRange;

        static readonly int BLayer = 1 << 3;
        static readonly int ALayer = 1 << 6;

        private void Awake()
        {
            _rootAnimator = GetComponent<Animator>();
            _source = GetComponent<AudioSource>();
            _monsterController = GetComponent<MonsterController>();
            _monsterController.OnPrisonSet += OnPrisonSet;
        }

        #region 아바타 설정

        protected override void OnNetworkPostSpawn()
        {
            BattleManager.Instance.OnGameStart += OnGameSetup;
        }

        public override void OnNetworkDespawn()
        {
            BattleManager.Instance.OnGameStart -= OnGameSetup;
            _monsterController.OnPrisonSet -= OnPrisonSet;
            _monsterController.Prison.isUnlock.OnValueChanged -= OnPrisonUnlocked;
        }

        private void OnGameSetup()
        {
            if (LocalManager.Instance.IamB)
                ApplyNormalAvatar();
            else
                ApplyMonsterAvatar();
        }

        void ApplyNormalAvatar()
        {
            _normalModel.SetActive(true);
            _monsterModel.SetActive(false);
            _rootAnimator.avatar = _normalAvatar;
        }

        void ApplyMonsterAvatar()
        {
            _normalModel.gameObject.SetActive(false);
            _monsterModel.gameObject.SetActive(true);
            _rootAnimator.avatar = _monsterAvatar;
        }
        #endregion

        #region 오디오
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                if (_isHelpLoop) LockedLoopAsync().Forget();
            }
        }

        void OnPrisonSet(Prison prison)
        {
            prison.isUnlock.OnValueChanged += OnPrisonUnlocked;
        }

        void OnPrisonUnlocked(bool prev, bool next)
        {
            // 해제되는 순간 _wasBInRange를 막아 재진입 방지
            if (next) _wasBInRange = true;
        }
        async UniTaskVoid LockedLoopAsync()
        {
            while (true)
            {
                float delay = UnityEngine.Random.Range(_lockedIntervalMin, _lockedIntervalMax);
                await UniTask.Delay((int)(delay * 1000), cancellationToken: destroyCancellationToken);

                if (_isTalked.Value || _monsterController.Prison.isUnlock.Value) break;

                PlayHelpRpc();
            }
        }

        private void Update()
        {
            if (!IsServer) return;

            // locked 상태일 때만 B팀 근접 대사 체크
            if (!_monsterController.Prison.isUnlock.Value)
            {
                bool isBInRange = Physics.CheckSphere(transform.position, talkRange, BLayer);
                if (isBInRange && !_wasBInRange)
                {
                    PlayTalkRpc();
                    _wasBInRange = true;
                }
            }

            // 항상: A팀(위협 대상) 근접 체크
            bool isAInRange = Physics.CheckSphere(transform.position, _threatRange, ALayer);
            if (isAInRange && !_wasAInRange)
            {
                PlayThreatRpc();
                _wasAInRange = true;
            }
        }

        [Rpc(SendTo.Everyone)]
        void PlayHelpRpc()
        {
            if (!LocalManager.Instance.IamB) return;
            AudioManager.Instance.PlaySfxWet(_locked, transform.position);
        }

        [Rpc(SendTo.Everyone)]
        void PlayTalkRpc()
        {
            if (IsServer) _isTalked.Value = true;
            if (!LocalManager.Instance.IamB) return;

            _source.resource = _talk;
            _source.Play();
        }

        [Rpc(SendTo.Everyone)]
        void PlayThreatRpc()
        {
            if (LocalManager.Instance.IamB) return; // A팀에게만 재생
            _source.resource = _threat;
            _source.Play();
            WaitAndPlayLockedAsync().Forget();
        }
        // 단순 기다리기
        async UniTaskVoid WaitAndPlayLockedAsync()
        {
            await UniTask.Delay(_threatAudioMs, cancellationToken: destroyCancellationToken);
            _wasAInRange = false;
        }
        #endregion
    }
}
