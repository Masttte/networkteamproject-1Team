using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 사망 후 관전 카메라. VFX 완료 후 자기 활성, 살아있는 플레이어의 SpectatorRoot 추적.
    /// Q 키로 다음 관전 대상 순환.
    /// 씬 배치 + Priority 변경으로 Brain 블렌딩 활용.
    /// </summary>
    public class SpectatorCamera : MonoBehaviour
    {
        // ===== Inspector Fields =====
        
        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera _camera;
        
        [Header("Priority")]
        [SerializeField] private int _activePriority = 100;
        [SerializeField] private int _inactivePriority = -1;
        
        [Header("Input")]
        [SerializeField] private BattleInputReader _input;
        [SerializeField] private CinemachineInputAxisController _inputController;
        
        // ===== Private Fields =====
        
        private List<PlayerCamera> _alivePlayers;
        private int _currentIndex;
        
        // 시네머신 Input Axis Controller의 기본 Gain 부호 저장 (Y 반전 등 셋업 보존용)
        private Dictionary<int, float> _baseGains = new Dictionary<int, float>();
        
        // ===== Constants =====
        
        private const string KEY_SENSITIVITY = "sensitivity";
        private const float SENSITIVITY_SCALE = 0.001f;
        
        // ===== Unity Lifecycle =====
        
        private void Awake()
        {
            CacheComponents();
            CaptureBaseGains();
            
            // 초기 민감도 PlayerPrefs에서 로드
            ApplySensitivity(PlayerPrefs.GetFloat(KEY_SENSITIVITY, 600f));
            
            // 이벤트 구독
            SettingMenu.OnMouseSensitivityChanged += ApplySensitivity;
        }
        
        private void OnDestroy()
        {
            UnsubscribeInput();
            SettingMenu.OnMouseSensitivityChanged -= ApplySensitivity;
        }
        
        // ===== Initialization =====
        
        private void CacheComponents()
        {
            if (_camera == null) _camera = GetComponent<CinemachineCamera>();
            
            if (_inputController == null)
                _inputController = GetComponent<CinemachineInputAxisController>();
        }
        
        private void CaptureBaseGains()
        {
            if (_inputController == null) return;
            
            for (int i = 0; i < _inputController.Controllers.Count; i++)
            {
                _baseGains[i] = _inputController.Controllers[i].Input.Gain;
            }
        }
        
        // ===== Public API =====
        
        public void TriggerAfterVFX()
        {
            if (VFXManager.Instance != null)
                VFXManager.Instance.OnDeathVFXCompleted += Activate;
            else
                Activate();  // VFX 없으면 즉시 활성
        }
        
        public void Activate()
        {
            // 호출 후 이벤트 해제
            if (VFXManager.Instance != null)
                VFXManager.Instance.OnDeathVFXCompleted -= Activate;
            
            FindAlivePlayers();
            
            if (_alivePlayers.Count == 0)
            {
                Debug.LogWarning("[SpectatorCamera] 살아있는 플레이어 0명. 모든 플레이어 사망");
                return;  // 살아있는 플레이어 없으면 입력 구독 X
            }
            
            _currentIndex = 0;
            FocusTarget(_currentIndex);
            
            // Priority 변경으로 Brain 블렌딩 활용
            _camera.Priority = _activePriority;
            
            // 입력 구독 (살아있는 플레이어 있을 때만)
            SubscribeInput();
            
            Debug.Log($"[SpectatorCamera] 활성. Priority={_activePriority}");
        }
        
        public void Deactivate()
        {
            _camera.Priority = _inactivePriority;
            UnsubscribeInput();
        }
        
        /// <summary>
        /// 게임 진행 중 다른 플레이어도 죽을 수 있으므로 매번 살아있는 플레이어 재검색.
        /// </summary>
        public void NextTarget()
        {
            if (_alivePlayers == null || _alivePlayers.Count == 0) return;
            
            FindAlivePlayers();
            
            if (_alivePlayers.Count == 0)
            {
                Debug.LogWarning("[SpectatorCamera] 살아있는 플레이어 0명");
                return;
            }
            
            _currentIndex = (_currentIndex + 1) % _alivePlayers.Count;
            FocusTarget(_currentIndex);
        }
        
        // ===== Target Find / Focus =====
        
        private void FindAlivePlayers()
        {
            _alivePlayers = new List<PlayerCamera>();
            
            var allPlayers = FindObjectsByType<PlayerCamera>(FindObjectsSortMode.None);
            foreach (var p in allPlayers)
            {
                // 살아있는 플레이어만 추가
                if (p.TryGetComponent<Battle.PlayerEntity>(out var entity) && !entity.IsDead) 
                    _alivePlayers.Add(p);
            }
        }
        
        private void FocusTarget(int index)
        {
            Debug.Log($"[SpectatorCamera] FocusTarget: index={index}");
            
            if (index < 0 || index >= _alivePlayers.Count)
            {
                Debug.LogWarning($"[SpectatorCamera] index 범위 벗어남. index={index}, count={_alivePlayers.Count}");
                return;
            }
            
            var playerCam = _alivePlayers[index];
            
            if (playerCam == null)
            {
                Debug.LogError($"[SpectatorCamera] playerCam null. index={index}");
                return;
            }
            
            if (playerCam.SpectatorRoot == null)
            {
                Debug.LogError($"[SpectatorCamera] {playerCam.name}의 SpectatorRoot null. 인스펙터 할당 확인.");
                return;
            }
            
            // 3인칭용 카메라 루트 추적 (애니메이션 영향 없는 안정적 Transform)
            // CameraTarget struct 전체 새로 할당
            _camera.Target = new CameraTarget
            {
                TrackingTarget = playerCam.SpectatorRoot,
                LookAtTarget = playerCam.SpectatorRoot
            };
            // _camera.Target.TrackingTarget = playerCam.SpectatorRoot;
            Debug.Log($"[SpectatorCamera] 추적 시작: {playerCam.name}");
        }
        
        // ===== Input =====
        
        private void SubscribeInput()
        {
            if (_input == null)
            {
                Debug.LogWarning("[SpectatorCamera] BattleInputReader가 인스펙터에 할당되지 않았습니다.");
                return;
            }
            
            _input.onNextTarget += NextTarget;
        }
        
        private void UnsubscribeInput()
        {
            if (_input == null) return;
            
            _input.onNextTarget -= NextTarget;
        }
        
        /// <summary>
        /// UI 오픈 등 외부 컨텍스트에서 관전 카메라 입력 전체 차단.
        /// 시네머신 마우스 입력 + NextTarget 입력 모두 토글.
        /// </summary>
        public void SetInputEnabled(bool enable)
        {
            if (_inputController != null)
                _inputController.enabled = enable;
        
            if (enable) SubscribeInput();
            else UnsubscribeInput();
        }
        
        // ===== Sensitivity =====
        
        private void ApplySensitivity(float uiValue)
        {
            if (_inputController == null) return;
            
            float scaled = uiValue * SENSITIVITY_SCALE;
            
            for (int i = 0; i < _inputController.Controllers.Count; i++)
            {
                float baseSign = Mathf.Sign(_baseGains[i]);
                _inputController.Controllers[i].Input.Gain = scaled * baseSign;
            }
        }
    }
}