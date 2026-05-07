using Player;
using Unity.Netcode;
using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using VolFx;

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

        [SerializeField] Color _tintInitial = Color.white;
        [SerializeField] Color _tintTarget = Color.red;

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
        MotionHandle _tintHandle;

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
                CurHp.OnValueChanged += HandleHpChanged;

                var go = GameObject.FindWithTag("GameController");
                if (go.TryGetComponent<Volume>(out _volume)) _volume.profile.TryGet(out _screenVFX);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            onDeath -= AlertDeath;
            if (IsServer) _combat.OnDeathAnimFinished -= FinalizeDeath;
            if (IsOwner)
            {
                CurHp.OnValueChanged -= HandleHpChanged;
                if (_intensityHandle.IsActive()) _intensityHandle.Cancel();
                if (_distortionHandle.IsActive()) _distortionHandle.Cancel();
                if (_glitchHandle.IsActive()) _glitchHandle.Cancel();
                if (_tintHandle.IsActive()) _tintHandle.Cancel();
                if (_deathSequenceHandle.IsActive()) _deathSequenceHandle.Cancel();
            }
        }

        void HandleHpChanged(int prev, int next)
        {
            //오너만 구독함, Hp가 줄어드는 경우만 있음
            if ( CurHp.Value > 0) // 살아있을때만 피격 연출
            {
                PlayHitVFX();
            }
        }

        void PlayHitVFX()
        {
            // Intensity
            _screenVFX._intensity.value = _intensityTarget;
            _intensityHandle = LMotion.Create(_intensityTarget, _intensityInitial, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._intensity.value = x);

            // Distortion Scale
            _screenVFX._distortionScale.value = _distortionTarget;
            _distortionHandle = LMotion.Create(_distortionTarget, _distortionInitial, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._distortionScale.value = x);

            // Apply To Glitch
            _screenVFX._applyToGlitch.value = _applyToGlitchTarget;
            _glitchHandle = LMotion.Create(_applyToGlitchTarget, _applyToGlitchInitial, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._applyToGlitch.value = x);

            // Distort Tint
            _screenVFX._distortYTint.value = _tintTarget;
            _tintHandle = LMotion.Create(_tintTarget, _tintInitial, _vfxDuration)
                .WithEase(_vfxEase)
                .Bind(x => _screenVFX._distortYTint.value = x);
        }

        void PlayDeathVFX()
        {
            if (_screenVFX == null) return;

            // 진행중이던 일반 피격 효과 다 끄기
            if (_intensityHandle.IsActive()) _intensityHandle.Cancel();
            if (_distortionHandle.IsActive()) _distortionHandle.Cancel();
            if (_glitchHandle.IsActive()) _glitchHandle.Cancel();
            if (_tintHandle.IsActive()) _tintHandle.Cancel();

            var builder = LSequence.Create();

            // _intensity 조절 시퀀스 (0 -> Target1(-5) -> Target2(5) -> 0)
            builder.Append(LMotion.Create(0f, _intensityTarget1, _intensityDuration1)
                .WithEase(_intensityEase1)
                .Bind(x => _screenVFX._intensity.value = x));

            builder.Append(LMotion.Create(_intensityTarget1, _intensityTarget2, _intensityDuration2)
                .WithEase(_intensityEase2)
                .Bind(x => _screenVFX._intensity.value = x));

            builder.Append(LMotion.Create(_intensityTarget2, 0f, _intensityDuration3)
                .WithEase(_intensityEase3)
                .Bind(x => _screenVFX._intensity.value = x));

            // _distortYTint(색조) 조절 시퀀스 (White -> 어둡게 -> White)
            builder.Insert(0f, LMotion.Create(_tintInitial, _deathTintHit, _deathTintDuration * 0.3f)
                .WithEase(_deathTintEase)
                .Bind(x => _screenVFX._distortYTint.value = x));

            builder.Insert(_deathTintDuration * 0.3f, LMotion.Create(_deathTintHit, _tintInitial, _deathTintDuration * 0.7f)
                .WithEase(_deathTintEase)
                .Bind(x => _screenVFX._distortYTint.value = x));

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

        private void Update()
        {
            //Debug.Log(_screenVFX._intensity.value);

            // 테스트 용 코드 shift 버튼 누르면 자해
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                CurHp.Value--;
            }
        }
    }
}
