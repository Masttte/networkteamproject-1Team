using System.Threading;
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
        [SerializeField] AudioResource _locked;
        [SerializeField] AudioResource _talk;
        AudioSource _source;

        [Header("Audio Timing")]
        [SerializeField] float _lockedIntervalMin = 7f;
        [SerializeField] float _lockedIntervalMax = 14f;
        [SerializeField] float _talkRange = 3f;

        NetworkVariable<bool> _isTalked = new NetworkVariable<bool>(false);
        CancellationTokenSource _lockedCts;

        static readonly int BLayer = 1 << 3;

        #region 아바타 설정
        private void Awake()
        {
            _rootAnimator = GetComponent<Animator>();
            _source = GetComponent<AudioSource>();
        }

        protected override void OnNetworkPostSpawn()
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
            _isTalked.OnValueChanged += OnLockedChanged;

            if (IsServer)
            {
                _lockedCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                LockedLoopAsync(_lockedCts.Token).Forget();
            }
        }

        public override void OnNetworkDespawn()
        {
            _isTalked.OnValueChanged -= OnLockedChanged;
            _lockedCts?.Cancel();
            _lockedCts?.Dispose();
        }

        void OnLockedChanged(bool prev, bool next)
        {
            if (next && IsServer)
                _lockedCts?.Cancel();
        }

        async UniTaskVoid LockedLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                float delay = UnityEngine.Random.Range(_lockedIntervalMin, _lockedIntervalMax);
                await UniTask.Delay((int)(delay * 1000), cancellationToken: ct);
                if (ct.IsCancellationRequested) break;

                PlayLockedRpc();
            }
        }

        private void Update()
        {
            if (!IsServer || _isTalked.Value) return;

            bool isBInRange = Physics.CheckSphere(transform.position, _talkRange, BLayer);

            if (isBInRange)
            {
                _isTalked.Value = true;
                PlayTalkRpc();
            }
        }

        [Rpc(SendTo.Everyone)]
        void PlayLockedRpc()
        {
            if (!LocalManager.Instance.IamB) return;
            AudioManager.Instance.PlaySfxWet(_locked, transform.position);
        }

        [Rpc(SendTo.Everyone)]
        void PlayTalkRpc()
        {
            if (!LocalManager.Instance.IamB) return;
            _source.resource = _talk;
            _source.Play();
        }
        #endregion
    }
}
