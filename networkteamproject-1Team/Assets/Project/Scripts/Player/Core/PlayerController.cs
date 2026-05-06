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
            
            // Owner만 모듈 간 의존성 주입 허용
            _input.Initialize(_movement, _combat);
            // 자기 시점 처리 (ViewPoint)
            _camera.SetupOwnerView();
        }
    }
}
