using System;
using Battle;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 공격 입력 처리 + 상태 머신.
    /// 실제 공격 판정은 Weapon 컴포넌트에 위임.
    /// 본 클래스는 상태 관리 / 애니메이션 호출 / 입력 차단을 담당.
    /// </summary>
    public class PlayerCombat : NetworkBehaviour
    {
        [Header("상태 전환 백업 타이머")]
        [SerializeField, Tooltip("공격 애니메이션 최대 지속 시간 — Animation Event 누락 시 백업")]
        private float _attackMaxDuration = 1.0f;
        [SerializeField, Tooltip("피격 애니메이션 최대 지속 시간 — Animation Event 누락 시 백업")]
        private float _hitMaxDuration = 0.8f;
        
        private NetworkVariable<PlayerCombatState> _state = new(
            PlayerCombatState.Normal,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    
        private Weapon _weapon;
        private PlayerAnimation _playerAnimation;
        private PlayerEntity _playerEntity;

        public PlayerCombatState CombatState => _state.Value;
        // 플레이어는 노말 상태(공격중이거나 공격 받거나, 죽지 않은 상태)에서만 새 공격 가능
        public bool CanAct => _state.Value == PlayerCombatState.Normal;
        // 플레이어는 죽지 않은 상태라면 언제든지 이동 가능
        public bool CanMove => _state.Value != PlayerCombatState.Dead;

        public event Action<PlayerCombatState, PlayerCombatState> OnStateChanged;

        private void Awake()
        {
            _weapon = GetComponent<Weapon>();
            _playerAnimation = GetComponent<PlayerAnimation>();
            _playerEntity = GetComponent<PlayerEntity>();
        }

        protected override void OnNetworkPostSpawn()
        {
            _state.OnValueChanged += HandleStateChanged;
            HandleStateChanged(PlayerCombatState.Normal, _state.Value);
        
            if (_playerEntity != null)
            {
                _playerEntity.CurHp.OnValueChanged += HandleHpChanged;
                _playerEntity.onDeath += HandleDeath;
            }
        }
        
        public override void OnNetworkDespawn()
        {
            _state.OnValueChanged -= HandleStateChanged;
            
            if (_playerEntity != null)
            {
                _playerEntity.CurHp.OnValueChanged -= HandleHpChanged;
                _playerEntity.onDeath -= HandleDeath;
            }
        }
        
        // ===== 공격 입력 =====

        public void RequestAttack()
        {
            if(!IsOwner) return;
            if (_playerEntity == null) return;
            if(!CanAct) return;
            if (!_weapon.IsReady) return;  // Weapon 쿨타임 체크
    
            Debug.Log("[PlayerCombat] RequestAttack");
            SubmitAttackServerRpc();
        }

        [ServerRpc]
        private void SubmitAttackServerRpc()
        {
            if (_state.Value != PlayerCombatState.Normal) return;
            
            _state.Value = PlayerCombatState.Attacking;
            _weapon.TryAttack();
            
            // 백업 타이머: Animation Event 누락 시 강제 복귀
            SafeReturnAsync(PlayerCombatState.Attacking, _attackMaxDuration).Forget();
        }
        
        // ===== HP/사망 이벤트 =====

        private void HandleHpChanged(int prev, int next)
        {
            if (!IsServer) return;
            if (next <= 0) return;     // 사망은 onDeath에서 처리
            if (next >= prev) return;  // 회복/변화 없음

            EnterHit();
        }
        
        private void HandleDeath()
        {
            if (!IsServer) return;
            _state.Value = PlayerCombatState.Dead;
        }

        private void EnterHit()
        {
            if (_state.Value == PlayerCombatState.Dead) return;

            _state.Value = PlayerCombatState.Hit;

            // 백업 타이머
            SafeReturnAsync(PlayerCombatState.Hit, _hitMaxDuration).Forget();
        }
        
        // ===== Animation Event 콜백 (PlayerAnimation에서 이벤트 발행) =====

        public void OnAttackAnimEnd() => AnimEndAction(PlayerCombatState.Attacking);
        public void OnHitAnimEnd() => AnimEndAction(PlayerCombatState.Hit);
        
        private void AnimEndAction(PlayerCombatState state)
        {
            if (!IsServer) return;
            if (_state.Value != state) return;

            _state.Value = PlayerCombatState.Normal;
        }

        // ===== 백업 타이머 (Animation Event 누락 대비) =====
        
        private async UniTaskVoid SafeReturnAsync(PlayerCombatState targetState, float duration)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration),
                    cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) { return; }

            if (!IsSpawned) return;
            if (_state.Value != targetState) return;
            if (_playerEntity != null && _playerEntity.IsDead) return;

            _state.Value = PlayerCombatState.Normal;
        }

        // ===== 상태 변경 (모든 클라이언트) =====

        private void HandleStateChanged(PlayerCombatState prev, PlayerCombatState next)
        {
            _playerAnimation.PlayStateAnimation(next);
            OnStateChanged?.Invoke(prev, next);
        }
    }
}
