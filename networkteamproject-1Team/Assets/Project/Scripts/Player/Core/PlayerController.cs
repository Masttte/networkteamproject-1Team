using Battle;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    // 캐싱 및 플레이어 모듈 간 의존성 주입을 담당하는 최상단 에이전트
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : NetworkBehaviour
    {
        // 컴포넌트 선언
        private PlayerInputHandler _input;
        private PlayerMovement     _movement;
        private PlayerCamera       _camera;
        private PlayerAnimation    _animation;
        private PlayerCombat       _combat;
        // private PlayerInteractor   _interactor;
        
        // 캐싱
        void Awake()
        {
            // 캐싱 실행
            _input      = GetComponent<PlayerInputHandler>();
            _movement   = GetComponent<PlayerMovement>();
            _camera     = GetComponent<PlayerCamera>();
            _animation  = GetComponent<PlayerAnimation>();
            _combat     = GetComponent<PlayerCombat>();
            // _interactor = GetComponent<PlayerInteractor>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                // 다른 클라이언트의 플레이어 — 입력/카메라/마우스 룩 비활성
                _input.enabled  = false;
                _camera.enabled = false;
                return;
            }
            // 스폰 직후: 카메라 회전만 활성 (둘러보기 OK), 나머지 입력 비활성
            _camera.IsInputEnabled = true;
            // PlayerInputHandler는 기본값 None이라 따로 비활성화 호출 불필요
            
            // 게임 라이프사이클 이벤트 구독
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnGameStart += HandleGameStart;
            
            // Owner만 모듈 간 의존성 주입 허용
            _input.Initialize(_movement, _combat);
            // 자기 시점 처리 (ViewPoint)
            _camera.SetupOwnerView();

            _combat.OnStateChanged += HandleCombatStateChanged;
        }

        public override void OnNetworkDespawn()
        {
            if(!IsOwner) return;

            if (BattleManager.Instance != null)
                BattleManager.Instance.OnGameStart -= HandleGameStart;

            if (_combat != null)
                _combat.OnStateChanged -= HandleCombatStateChanged;
        }

        private void HandleGameStart()
        {
            // 게임 시작: 모든 입력 활성
            _input.EnableInput(InputCategory.All);
        }

        private void HandleCombatStateChanged(PlayerCombatState prev, PlayerCombatState next)
        {
            // Hit 중에도 이동 가능이라 Hit 상태에선 입력 비활성 안 함
            // 공격 중에도 이동/카메라 가능. CanAct 체크가 PlayerCombat 내부에서 처리되니 추가 입력 차단 불필요.
            if (next == PlayerCombatState.Dead)
            {
                // 사망 시 모든 입력 + 카메라 회전 비활성
                _input.DisableInput(InputCategory.All);
                _camera.IsInputEnabled = false;
            }
        }
    }
}
