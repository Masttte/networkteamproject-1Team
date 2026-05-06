using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimation : MonoBehaviour
    {
        // 애니메이션 파라미터 해싱
        // Animator Param Hashes
        private static readonly int AnimSpeed       = Animator.StringToHash("Speed");
        private static readonly int AnimGrounded    = Animator.StringToHash("Grounded");
        private static readonly int AnimJump        = Animator.StringToHash("Jump");
        private static readonly int AnimFreeFall    = Animator.StringToHash("FreeFall");
        private static readonly int AnimMotionSpeed = Animator.StringToHash("MotionSpeed");
        
        // Action 파라미터 (Layer 1, 상체 Mask)
        private static readonly int AnimAttack      = Animator.StringToHash("Attack");
        private static readonly int AnimHit         = Animator.StringToHash("Hit");
        // Death (Layer 0, 전신 정지)
        private static readonly int AnimDeath       = Animator.StringToHash("Death");
        
        private Animator _animator;
        private PlayerMovement _movement;
        private PlayerCombat _combat;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _movement = GetComponent<PlayerMovement>();
            _combat = GetComponent<PlayerCombat>();
        }
        
        // Animator의 Animation Event에서 호출 (Punching 클립 끝 프레임)
        public void OnAttackAnimEnd() => _combat?.OnAttackAnimEnd();
    
        // Animator의 Animation Event에서 호출 (Getting Hit 클립 끝 프레임)
        public void OnHitAnimEnd() => _combat?.OnHitAnimEnd();

        void Update()
        {
            if (_movement == null) return;
            
            // 이동 관련 ( Layer 0 )
            _animator.SetFloat(AnimSpeed, _movement.CurrentSpeed);
            _animator.SetFloat(AnimMotionSpeed, _movement.MotionSpeed);
            _animator.SetBool(AnimGrounded, _movement.IsGrounded);
            
            if (_movement.JustJumped)
                _animator.SetBool(AnimJump, true);
            else if (_movement.IsGrounded)
                _animator.SetBool(AnimJump, false);
            
            // 땅에 닿아있지 않으며 점프 상승 중이 아닌 아래로 떨어질 때만 낙하로 판정
            _animator.SetBool(AnimFreeFall, !_movement.IsGrounded && _movement.VerticalVelocity < 0.0f);
        }
        
        /// <summary>
        /// 전투 스크립트에서 상태 변경에 따라 호출
        /// NetworkVariable.OnValueChanged 자동 동기화
        /// Layer 1 (상체) 트리거 처리.
        /// </summary>
        public void PlayStateAnimation(PlayerCombatState state)
        {
            // Normal, 공격, 피격, 사망 상태.
            switch (state)
            {
                case PlayerCombatState.Normal:
                    // Action Layer Idle로 자동 트랜지션
                    break;
                case PlayerCombatState.Attacking:
                    _animator.SetTrigger(AnimAttack);
                    break;
                case PlayerCombatState.Hit:
                    _animator.SetTrigger(AnimHit);
                    break;
                case PlayerCombatState.Dead:
                    _animator.SetTrigger(AnimDeath);
                    break;
            }
        }
    }
}
