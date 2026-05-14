using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerAnimation : NetworkBehaviour
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
        
        // 현재 활성 모델의 Animator (Trigger / culling 적용용)
        private Animator _animator;
        
        // TeamA 프리팹의 두 모델 Animator (Owner Update에서 동시 파라미터 설정용)
        private Animator _animatorNormal;
        private Animator _animatorMonster;
        
        private PlayerMovement _movement;
        private PlayerCombat _combat;
        private TeamA _teamA;  // 시민 프리팹 캐싱 (없으면 null)
        
        public PlayerCombat Combat => _combat;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _combat = GetComponent<PlayerCombat>();
            _teamA = GetComponent<TeamA>();
        }

        public override void OnNetworkSpawn()
        {
            CacheAllAnimators();
            RefreshAnimator();
            
            // 시민(TeamA) 프리팹: 모델 전환 시 Animator 재캐싱
            // 마피아(TeamB) 프리팹: _teamA가 null이라 구독 X (단일 모델, 갱신 불필요)
            if (_teamA != null)
                _teamA.OnModelSwitched += RefreshAnimator;
        }
        
        public override void OnNetworkDespawn()
        {
            if (_teamA != null)
                _teamA.OnModelSwitched -= RefreshAnimator;
        }

        void Update()
        {
            if (!IsOwner) return;
            if (_movement == null || _animator == null) return;
            
            SetMovementParams(_animatorNormal);
            SetMovementParams(_animatorMonster);
        }
        
        /// <summary>
        /// PlayerCombat에서 상태 변경에 따라 호출.
        /// Layer 1 (상체) 트리거 처리.
        /// Trigger는 현재 활성 모델의 Animator에만 적용 (NetworkAnimator가 동기화).
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
                    SetTriggerOnBoth(AnimAttack);
                    break;
                case PlayerCombatState.Hit:
                    SetTriggerOnBoth(AnimHit);
                    break;
                case PlayerCombatState.Dead:
                    SetTriggerOnBoth(AnimDeath);
                    Debug.Log("Dead");
                    break;
            }
        }
        
        private void SetTriggerOnBoth(int hash)
        {
            if (_animatorNormal != null) _animatorNormal.SetTrigger(hash);
            if (_animatorMonster != null) _animatorMonster.SetTrigger(hash);
        }
        
        /// <summary>
        /// 두 모델의 Animator 모두 캐싱.
        /// Owner Update에서 두 Animator 동시 파라미터 설정용.
        /// </summary>
        private void CacheAllAnimators()
        {
            if (_teamA != null)
            {
                // 시민 프리팹: 두 모델 각각의 Animator
                _animatorNormal = _teamA.NormalModel.GetComponent<Animator>();
                _animatorMonster = _teamA.MonsterModel.GetComponent<Animator>();
            }
            else
            {
                // 마피아 프리팹: 단일 Animator
                _animatorNormal = GetComponentInChildren<Animator>();
                _animatorMonster = null;
            }
        }
        
        // <summary>
        /// 활성 모델의 Animator를 캐싱 (Trigger / culling 적용용).
        /// 시민(TeamA) 프리팹은 OnModelSwitched 이벤트로 모델 전환 시 재호출.
        /// 마피아(TeamB) 프리팹은 OnNetworkSpawn 시 1회만 호출.
        /// </summary>
        private void RefreshAnimator()
        {
            // GetComponentInChildren: 활성 GameObject의 컴포넌트만 검색
            // - TeamA 시민 모델 활성 시: A 자식의 Animator 발견
            // - TeamA 괴물 모델 활성 시: B 자식의 Animator 발견
            // - TeamB: A 자식의 Animator 발견 (단일 모델)
            _animator = GetComponentInChildren<Animator>();
    
            if (_animator == null)
            {
                Debug.LogError("[PlayerAnimation] Animator를 찾을 수 없습니다.");
                return;
            }
            
            ApplyCullingMode();
        }
        
        private void SetMovementParams(Animator anim)
        {
            if (anim == null) return;
            
            // 이동 관련 파라미터 ( Layer 0 )
            anim.SetFloat(AnimSpeed, _movement.CurrentSpeed);
            anim.SetFloat(AnimMotionSpeed, _movement.MotionSpeed);
            anim.SetBool(AnimGrounded, _movement.IsGrounded);
            
            if (_movement.JustJumped)
                anim.SetBool(AnimJump, true);
            else if (_movement.IsGrounded)
                anim.SetBool(AnimJump, false);
            
            // 땅에 닿아있지 않으며 점프 상승 중이 아닌 아래로 떨어질 때만 낙하로 판정
            anim.SetBool(AnimFreeFall, !_movement.IsGrounded && _movement.VerticalVelocity < 0.0f);
        }
        
        private void ApplyCullingMode()
        {
            // 자신의 캐릭터는 백그라운드에서도 무조건 연산
            // 포커싱 중이거나 그려지지 않는 판정에서 애니메이션 업데이트가 중단되어
            // 피격에 반응 못 하거나 카메라 추적 못 하는 문제 방지
            var mode = IsOwner 
                ? AnimatorCullingMode.AlwaysAnimate 
                : AnimatorCullingMode.CullUpdateTransforms;
            
            if (_animatorNormal != null) _animatorNormal.cullingMode = mode;
            if (_animatorMonster != null) _animatorMonster.cullingMode = mode;
        }
    }
}
