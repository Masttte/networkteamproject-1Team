using Player;
using Unity.Netcode;
using UnityEngine;
using LitMotion;

using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using VolFx;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Battle
{
    public class PlayerEntity : EntityBase
    {
        PlayerCombat _combat;

        // 연출
        Volume _volume;
        CustomScreenVol _screenVFX;

        [Header("Hit VFX Settings")]
        [SerializeField] float _vfxDuration = 0.5f;
        [SerializeField] Ease _vfxEase = Ease.OutQuad;

        [Space(10)]
        [SerializeField] float _intensityInitial = 0f;
        [SerializeField] float _intensityTarget = 1f;

        [SerializeField] float _distortionInitial = 0f;
        [SerializeField] float _distortionTarget = 2f;

        [SerializeField] float _applyToGlitchInitial = 6.26f;
        [SerializeField] float _applyToGlitchTarget = 15f;

        [Header("Death VFX Settings")]
        [SerializeField] float _intensityDuration1 = 3f;
        [SerializeField] float _intensityDuration2 = 0.2f;
        [SerializeField] float _intensityDuration3 = 1.0f;

        [SerializeField] Ease _intensityEase1 = Ease.OutSine;
        [SerializeField] Ease _intensityEase2 = Ease.OutCubic;
        [SerializeField] Ease _intensityEase3 = Ease.InQuad;

        [Space(10)]
        [SerializeField] float _intensityTarget1 = -5f;
        [SerializeField] float _intensityTarget2 = 5f;

        [SerializeField] Color _deathTintHit = new Color(0.11f, 0.11f, 0.11f); // 어두운 톤

        [SerializeField] float _deathTintDuration = 1.0f;
        [SerializeField] Ease _deathTintEase = Ease.OutQuad;

        MotionHandle _intensityHandle;
        MotionHandle _distortionHandle;
        MotionHandle _glitchHandle;

        MotionHandle _deathSequenceHandle;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            onDeath += AlertDeath;

            // 서버에서만 사망 후 처리 흐름 관리
            if (IsServer)
            {
                _combat = GetComponent<PlayerCombat>();
                _combat.OnDeathAnimFinished += FinalizeDeath;
            }

            if (IsOwner)
            {
                var go = GameObject.FindWithTag("VFX");
                if (go == null) return; 

                _volume = go.GetComponentInChildren<Volume>();
                _volume.profile.TryGet(out _screenVFX);
                CurHp.OnValueChanged += HandleHpChanged; // 볼륨이 있어야 구독
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            onDeath -= AlertDeath;
            if (IsServer) _combat.OnDeathAnimFinished -= FinalizeDeath;
            if (IsOwner) CurHp.OnValueChanged -= HandleHpChanged;
        }

        void HandleHpChanged(int prev, int next)
        {
            //오너만 구독함, Hp가 줄어드는 경우만 있음
            if (CurHp.Value > 0) // 살아있을때만 피격 연출
            {
                PlayHitVFX();
            }
        }

        void PlayHitVFX()
        {
            PlayHitVFXAsync().Forget();
        }

        async UniTaskVoid PlayHitVFXAsync()
        {
            await UniTask.Delay(300);

            // Intensity
            _intensityHandle = LMotion.Create(_intensityInitial, _intensityTarget, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._intensity.value = x);

            // Distortion Scale
            _distortionHandle = LMotion.Create(_distortionInitial, _distortionTarget, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._distortionScale.value = x);

            // Apply To Glitch
            _glitchHandle = LMotion.Create(_applyToGlitchInitial, _applyToGlitchTarget, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._applyToGlitch.value = x);
        }

        void PlayDeathVFX()
        {
            // 진행중이던 일반 피격 효과 다 끄기
            if (_intensityHandle.IsActive()) _intensityHandle.Cancel();
            if (_distortionHandle.IsActive()) _distortionHandle.Cancel();
            if (_glitchHandle.IsActive()) _glitchHandle.Cancel();


            var builder = LSequence.Create();

            // 딜레이: _deathVFXDelay 만큼 대기
            builder.Append(LMotion.Create(0f, 0f, 0.3f).Bind(_ => { }));

            // _intensity 조절 시퀀스
            builder.Append(LMotion.Create(0f, _intensityTarget1, _intensityDuration1)
                .WithEase(_intensityEase1)
                .Bind(x => _screenVFX._intensity.value = x));

            builder.Append(LMotion.Create(_intensityTarget1, _intensityTarget2, _intensityDuration2)
                .WithEase(_intensityEase2)
                .Bind(x => _screenVFX._intensity.value = x));

            builder.Append(LMotion.Create(_intensityTarget2, 0f, _intensityDuration3)
                .WithEase(_intensityEase3)
                .Bind(x => _screenVFX._intensity.value = x));

            // _glitchTint 색조 시퀀스 (white -> 어둡게 -> dark)
            builder.Append(LMotion.Create(Color.white, _deathTintHit, _deathTintDuration * 0.3f)
                .WithEase(_deathTintEase)
                .Bind(x => _screenVFX._glitchTint.value = x));

            builder.Insert(_deathTintDuration * 0.3f, LMotion.Create(_deathTintHit, Color.black, _deathTintDuration * 0.7f)
                .WithEase(_deathTintEase)
                .Bind(x => _screenVFX._glitchTint.value = x));

            _deathSequenceHandle = builder.Run();
        }

        void AlertDeath()
        {
            if (!IsServer) return;

            NotifyDeathClientRpc();
        }

        void FinalizeDeath()
        {
            if (!IsServer) return;
            BattleManager.Instance.DestroyPlayer(this);
        }

        [ClientRpc]
        void NotifyDeathClientRpc()
        {
            if (IsOwner) YouDied(); // 오너만 사망 연출
        }
        void YouDied()
        {
            PlayDeathVFX();
        }
    }
}
