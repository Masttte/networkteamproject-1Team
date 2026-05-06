using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("ViewPoints (각 모델별)")]
        [SerializeField] private Transform _viewPointA;
        [SerializeField] private Transform _viewPointB;
        
        [Header("Head Bones (각 모델별)")]
        [SerializeField] private Transform _headBoneA;
        [SerializeField] private Transform _headBoneB;
        
        // 헤드 본 기준 로컬 좌표계의 오프셋
        private Vector3 _viewPointLocalOffset;
        
        [Header("Look")]
        [SerializeField] private float _upClamp = -80f;
        [SerializeField] private float _downClamp = 80f;
        [SerializeField] private float _mouseSensitivity = 0.3f;

        private Transform _activeHeadBone; // 현재 활성화된 머리뼈
        private CinemachineCamera _virtualCamera;
        private Transform _activeViewPoint;
        
        private float _yaw;
        private float _pitch;
        private bool _isOwnerView;
        
        public float YawAngle => _yaw;
        public Transform ViewPoint => _activeViewPoint;
        
        // TODO: 추후 인풋 핸들러 기반으로 변경
        public bool IsInputEnabled { get; set; } = false;  // 카메라 회전 입력
        
        public void SetupOwnerView()
        {
            if (_viewPointA == null)
            {
                Debug.LogError("[PlayerCamera] ViewPoint_A is not assigned.");
                return;
            }
            if (_headBoneA == null)  // ← 추가
            {
                Debug.LogError("[PlayerCamera] HeadBone_A is not assigned.");
                return;
            }
            // 초기 ViewPoint = A (기본 모델)
            _activeViewPoint = _viewPointA;
            
            _activeHeadBone = _headBoneA;
            
            // 할당된 A 뼈대를 기준으로 오프셋 캡처
            _viewPointLocalOffset = _activeHeadBone.InverseTransformPoint(_activeViewPoint.position);
            
            AssignToVirtualCamera();
            
            // B 전환 이벤트 구독
            if (LocalManager.Instance != null)
                LocalManager.Instance.OnIamBSet += SwitchToB;
            
            _yaw = transform.eulerAngles.y;
            _isOwnerView = true;
        }
        
        private void AssignToVirtualCamera()
        {
            // 씬에서 시네머신 카메라 찾아서 Target만 갈아끼우기
            if (_virtualCamera == null)
                _virtualCamera = FindAnyObjectByType<CinemachineCamera>();
        
            if (_virtualCamera == null)
            {
                Debug.LogError("[PlayerCamera] CinemachineCamera not found in scene.");
                return;
            }
        
            _virtualCamera.Target.TrackingTarget = _activeViewPoint;
        }
        
        private void SwitchToB()
        {
            if (_viewPointB == null)
            {
                Debug.LogError("[PlayerCamera] ViewPoint_B is not assigned.");
                return;
            }
            _activeViewPoint = _viewPointB;
            _activeHeadBone = _headBoneB;
            
            // 할당된 B 뼈대를 기준으로 오프셋 캡처
            _viewPointLocalOffset = _activeHeadBone.InverseTransformPoint(_activeViewPoint.position);

            AssignToVirtualCamera();  // 카메라 재부착
        }
        
        private void OnDestroy()
        {
            if (LocalManager.Instance != null)
                LocalManager.Instance.OnIamBSet -= SwitchToB;
        }

        private void Update()
        {
            if (!_isOwnerView || !enabled) return;
            if (!IsInputEnabled) return;
            HandleMouseInput();
        }

        private void LateUpdate()
        {
            // 위치: 헤드 본을 직접 추적
            // 회전: 마우스 룩 독립 처리
            if (!_isOwnerView || _activeViewPoint == null || _activeHeadBone == null) return; 
            
            // 캡처한 오프셋 기준으로 헤드 본 위치 추적
            _activeViewPoint.position = _activeHeadBone.TransformPoint(_viewPointLocalOffset);
            _activeViewPoint.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void HandleMouseInput()
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            _yaw += delta.x * _mouseSensitivity;
            _pitch -= delta.y * _mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, _upClamp, _downClamp);
        }
    }
}