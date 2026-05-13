using System;
using System.Collections.Generic;
using Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    public class SpectatorCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _camera;
        
        [SerializeField] private int _activePriority = 100;
        [SerializeField] private int _inactivePriority = -1;
    
        private List<PlayerCamera> _alivePlayers;
        private int _currentIndex;
        
        private const string SPECTATOR_CAMERA_TAG = "Spectator";

        private void Awake()
        {
            if(_camera == null) _camera = GetComponent<CinemachineCamera>();
        }

        public void TriggerAfterVFX()
        {
            if (VFXManager.Instance != null)
                VFXManager.Instance.OnDeathVFXCompleted += Activate;
            else
                Activate();  // VFX 없으면 즉시 활성
        }
        
        public void Activate()
        {
            // 호출후 이벤트 삭제
            if (VFXManager.Instance != null)
                VFXManager.Instance.OnDeathVFXCompleted -= Activate;
            
            FindAlivePlayers();
            
            if (_alivePlayers.Count == 0)
            {
                Debug.LogWarning("[SpectatorCamera] 살아있는 플레이어 0명. 모든 플레이어 사망");
                // 게임 종료 처리 또는 빈 화면 처리
                return;
            }
            
            _currentIndex = 0;
            FocusTarget(0);
            
            // Priority 변경으로 Brain 블렌딩 활용
            _camera.Priority = _activePriority;
            Debug.Log($"[SpectatorCamera] 활성. Priority={_activePriority}");
        }

        public void Deactivate()
        {
            _camera.Priority = _inactivePriority;
        }
        
        // 게임 진행 중 다른 플레이어도 죽을 수 있음.
        // NextTarget 호출 시 죽은 플레이어 자동 제거 또는 재검색
        public void NextTarget()
        {
            if (_alivePlayers == null || _alivePlayers.Count == 0) return;
            
            // 살아있는 플레이어 재검색
            FindAlivePlayers();
            
            if (_alivePlayers.Count == 0)
            {
                Debug.LogWarning("[SpectatorCamera] 살아있는 플레이어 0명");
                return;
            }
            
            _currentIndex = (_currentIndex + 1) % _alivePlayers.Count;
            FocusTarget(_currentIndex);
        }

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
            _camera.Target.TrackingTarget = playerCam.SpectatorRoot;
            Debug.Log($"[SpectatorCamera] 추적 시작: {playerCam.name}");
        }
    }
}
