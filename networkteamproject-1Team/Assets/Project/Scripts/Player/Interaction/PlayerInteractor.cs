using Unity.Netcode;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 시점에서 IInteractable 감지 + 입력 라우팅.
    /// IInteractable 측에 InteractStart / InteractStop 호출.
    /// 실제 상호작용 로직(게이지, 효과)은 IInteractable 측 책임.
    /// </summary>
    public class PlayerInteractor : NetworkBehaviour
    {
        [Header("Detection")]
        [SerializeField, Tooltip("상호작용 감지 거리")]
        private float _interactRange = 3f;
        [SerializeField, Tooltip("Raycast 대상 레이어")]
        private LayerMask _interactableLayer;
        
        [Header("References")]
        [SerializeField, Tooltip("Raycast 시작점 (카메라 ViewPoint 권장, 폴백용)")]
        private Transform _detectOrigin;
        [SerializeField] private PlayerCamera _camera;
        
        // 현재 시야에 잡힌 타겟 (시야 변경 시 변경됨)
        private IInteractable _currentTarget;
        
        // 누르고 있는 동안 유지되는 활성 상호작용 (시야 변경되어도 유지)
        private IInteractable _activeInteraction;
        
        public IInteractable CurrentTarget => _currentTarget;
        public bool IsInteracting => _activeInteraction != null;
        
        private void Awake()
        {
            if (_camera == null)
                _camera = GetComponent<PlayerCamera>();
        }
        
        private void Update()
        {
            if (!IsOwner) return;
            DetectTarget();
        }
        
        // ===== 외부 주입 (PlayerController에서 호출) =====
        
        public void SetDetectOrigin(Transform origin) => _detectOrigin = origin;
        public void SetCamera(PlayerCamera cam) => _camera = cam;
        
        // ===== 입력 진입점 (PlayerInputHandler에서 호출) =====
        
        public void OnInteractStart()
        {
            if (!IsOwner) return;
            if (_currentTarget == null) return;
            if (_activeInteraction != null) return;  // 이미 다른 상호작용 중이면 차단
            
            _activeInteraction = _currentTarget;
            _activeInteraction.InteractStart();
        }
        
        public void OnInteractCancel()
        {
            if (!IsOwner) return;
            if (_activeInteraction == null) return;
            
            _activeInteraction.InteractStop();
            _activeInteraction = null;
        }
        
        // ===== 타겟 감지 =====
        
        private void DetectTarget()
        {
            // 카메라 ViewPoint 우선, 없으면 _detectOrigin 폴백
            Transform origin = _camera != null ? _camera.ViewPoint : _detectOrigin;
            if (origin == null) return;
            
            Ray ray = new Ray(origin.position, origin.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, _interactRange, _interactableLayer))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    _currentTarget = interactable;
                    return;
                }
            }
            
            _currentTarget = null;
        }
    }
}