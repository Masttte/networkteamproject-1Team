using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using Player;

namespace Interactable
{
    public class Phone : NetworkBehaviour, IInteractable
    {
        [Header("벨소리")]
        [SerializeField] AudioResource _ringingClip;

        [Header("A팀 수신 메시지 (적대적)")]
        [SerializeField] AudioResource _teamAAnswerClip;

        [Header("B팀 수신 메시지 (우호적?)")]
        [SerializeField] AudioResource _teamBAnswerClip;

        [Header("벨 언제 울릴지 (초)")]
        [SerializeField] int _ringIntervalMin = 20;
        [SerializeField] int _ringIntervalMax = 40;
        int _ringMs = 15578; // 벨소리 길이. 하드코딩 중

        AudioSource _ringSource; // 벨소리 전용 (Stop() 가능하도록)
        CancellationTokenSource _ringCts; // 서버 전용 루프 취소용

        NetworkVariable<bool> _isAnswered = new NetworkVariable<bool>(false);

        public override void OnNetworkSpawn()
        {
            _ringSource = GetComponent<AudioSource>();
            _isAnswered.OnValueChanged += OnAnsweredChanged;
            gameObject.layer = 2; // Interactable 비활성화 (벨소리 울리기 전까지 안전장치)

            if (IsServer)
            {
                _ringCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                RingLoopAsync(_ringCts.Token).Forget();
            }
        }

        public override void OnNetworkDespawn()
        {
            _isAnswered.OnValueChanged -= OnAnsweredChanged;
            _ringCts?.Cancel();
            _ringCts?.Dispose();
        }

        private async UniTaskVoid RingLoopAsync(CancellationToken ct)
        {
            int delay = UnityEngine.Random.Range(_ringIntervalMin, _ringIntervalMax);
            await UniTask.Delay(delay * 1000, cancellationToken: ct);

            while (!ct.IsCancellationRequested)
            {
                PlayRingRpc(); // 모든 클라이언트에 벨 재생 요청
                await UniTask.Delay(_ringMs, cancellationToken: ct); // 벨 한 사이클 재생 대기
            }
        }

        [Rpc(SendTo.Everyone)]
        private void PlayRingRpc()
        {
            gameObject.layer = 12; // Interactable 레이어 활성화
            _ringSource.resource = _ringingClip;
            _ringSource.Play();
        }

        public void InteractStart()
        {
            //if (_isAnswered.Value) return;
            AnswerPhoneRpc(LocalManager.Instance.IamB); // 클라이언트가 자신의 팀 정보를 직접 넘겨줌
            DisablePhoneRpc();
        }
        [Rpc(SendTo.Everyone)]
        void DisablePhoneRpc()
        {
            gameObject.layer = 2; // 상호작용 레이어 비활성화
        }


        public void InteractStop()
        {
            // 단순 클릭 상호작용이라 홀드 종료 처리 없음
        }

        [Rpc(SendTo.Server)]
        private void AnswerPhoneRpc(bool isTeamB, RpcParams rpcParams = default)
        {
            if (_isAnswered.Value) return;
            _isAnswered.Value = true;

            RpcParams targetParams = RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp);
            PlayResponseRpc(isTeamB, targetParams);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void PlayResponseRpc(bool isTeamB, RpcParams rpcParams)
        {
            PlayResponse(isTeamB).Forget();
        }

        async UniTaskVoid PlayResponse(bool isTeamB)
        {
            // 로컬 플레이어의 PlayerInputHandler 가져오기
            var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            var input = playerObj.GetComponent<PlayerInputHandler>();

            input.DisableInput(InputCategory.Movement); // 전화중엔 이동불가

            await UniTask.Delay(500);
            AudioResource clip = isTeamB ? _teamBAnswerClip : _teamAAnswerClip;
            AudioManager.Instance.PlaySfxDry(clip);
            if (!isTeamB) { VFXManager.Instance.PlayAlertVFX(); }

            // 클립 길이 만큼 대기
            float clipLength = AudioManager.Instance.drySfxSource.clip.length;
            await UniTask.Delay((int)(clipLength * 1000) - 560); // 살짝 빨리 풀리기

            input.EnableInput(InputCategory.Movement); // 이동 복구
        }


        private void OnAnsweredChanged(bool previousValue, bool newValue)
        {
            if (!newValue) return;

            // 모든 클라이언트: 벨소리 즉시 정지
            _ringSource.Stop();

            // 서버: 루프 취소
            if (IsServer)
            {
                _ringCts?.Cancel();
            }
        }
    }
}