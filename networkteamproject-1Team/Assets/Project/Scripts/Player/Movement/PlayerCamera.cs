using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    public class PlayerCamera : MonoBehaviour
    {
        // ===== Inspector Fields =====
        
        [Header("ViewPoints (각 모델별)")]
        [SerializeField] private Transform _viewPointA;
        [SerializeField] private Transform _viewPointB;
        
        [Header("Head Bones (각 모델별)")]
        [SerializeField] private Transform _headBoneA;
        [SerializeField] private Transform _headBoneB;

        [Header("관전 타겟 루트")]
        [SerializeField] private Transform _spectatorRoot;
        
        [Header("Look")]
        [SerializeField] private float _upClamp = -80f;
        [SerializeField] private float _downClamp = 80f;
        [SerializeField] private float _mouseSensitivity = 0.3f;
        
        // ===== Private Fields =====
        
        // 헤드 본 기준 로컬 좌표계의 오프셋
        private Vector3 _viewPointLocalOffset;
        
        private Transform _activeHeadBone;       // 현재 활성화된 머리뼈
        private CinemachineCamera _virtualCamera;
        private Transform _activeViewPoint;
        
        private float _yaw;
        private float _pitch;
        private bool _isOwnerView;
        
        // 본 추적 허용 여부
        private bool _followBoneRotation;
        
        // 마우스 회전 입력값 (매 프레임 InputHandler가 주입, Update에서 적용 후 0으로 리셋)
        private Vector2 _lookInput;
        
        // ===== Constants =====
        
        private const string GAME_CAMERA_TAG = "GameController";
        private const string KEY_SENSITIVITY = "sensitivity";
        private const float SENSITIVITY_SCALE = 1f / 2000f;  // UI 600 = 0.3
        
        // ===== Public Properties =====
        
        public float YawAngle => _yaw;
        public Transform ViewPoint => _activeViewPoint;
        public Transform SpectatorRoot => _spectatorRoot;
        
        // ===== Unity Lifecycle =====
        
        private void Awake()
        {
            // 초기 민감도 PlayerPrefs에서 로드
            ApplySensitivity(PlayerPrefs.GetFloat(KEY_SENSITIVITY, 600));
            
            // 이벤트 구독
            SettingMenu.OnMouseSensitivityChanged += ApplySensitivity;
        }
        
        private void Update()
        {
            if (!_isOwnerView || !enabled) return;
            
            // _lookInput은 PlayerInputHandler가 게이트 체크 후 주입한 값
            ApplyLookInput();
            _lookInput = Vector2.zero;  // 입력 적용 후 리셋 (다음 프레임 마우스 멈추면 회전 정지)
        }
        
        private void LateUpdate()
        {
            // 위치: 헤드 본을 직접 추적
            // 회전: 마우스 룩 독립 처리
            if (!_isOwnerView || _activeViewPoint == null || _activeHeadBone == null) return; 
            
            // 캡처한 오프셋 기준으로 헤드 본 위치 추적
            _activeViewPoint.position = _activeHeadBone.TransformPoint(_viewPointLocalOffset);
            
            // 본 추적 허용시 헤드 본의 회전값을 추적
            _activeViewPoint.rotation = _followBoneRotation
                ? _activeHeadBone.rotation
                : Quaternion.Euler(_pitch, _yaw, 0f);
        }
        
        private void OnDestroy()
        {
            if (LocalManager.Instance != null)
                LocalManager.Instance.OnIamBSet -= SwitchToB;
            
            SettingMenu.OnMouseSensitivityChanged -= ApplySensitivity;
        }
        
        // ===== Public API =====
        
        public void SetupOwnerView()
        {
            if (_viewPointA == null)
            {
                Debug.LogError("[PlayerCamera] ViewPoint_A is not assigned.");
                return;
            }
            if (_headBoneA == null)
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
        
        public void SetFollowBoneRotation(bool follow) => _followBoneRotation = follow;
        
        /// <summary>
        /// PlayerInputHandler가 onLook 이벤트 받아 호출.
        /// InputCategory.Camera 게이트 통과 후 주입.
        /// </summary>
        public void SetLookInput(Vector2 delta)
        {
            _lookInput = delta;
        }
        
        // ===== Internal Logic =====
        
        private void AssignToVirtualCamera()
        {
            // 씬에서 시네머신 카메라 찾아서 Target만 갈아끼우기
            if (_virtualCamera == null)
            {
                var camObj = GameObject.FindGameObjectWithTag(GAME_CAMERA_TAG);
                if (camObj != null)
                    _virtualCamera = camObj.GetComponent<CinemachineCamera>();
            }
            
            if (_virtualCamera == null)
            {
                Debug.LogError($"[PlayerCamera] '{GAME_CAMERA_TAG}' 태그의 CinemachineCamera를 씬에서 찾을 수 없습니다.");
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
        
        private void ApplyLookInput()
        {
            _yaw += _lookInput.x * _mouseSensitivity;
            _pitch -= _lookInput.y * _mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, _upClamp, _downClamp);
        }
        
        private void ApplySensitivity(float uiValue)
        {
            _mouseSensitivity = uiValue * SENSITIVITY_SCALE;
        }
    }
}