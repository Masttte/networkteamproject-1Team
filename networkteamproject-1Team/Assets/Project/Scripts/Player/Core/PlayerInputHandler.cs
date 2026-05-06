using UnityEngine;

namespace Player
{
    // BattleInputReader (SO) 구독 → 각 모듈로 라우팅.
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private BattleInputReader _input;
#if UNITY_EDITOR
        private void Reset()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BattleInputReader");
            if (guids.Length > 0)
                _input = UnityEditor.AssetDatabase.LoadAssetAtPath<BattleInputReader>(
                    UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
            else
                Debug.LogWarning("BattleInputReader SO를 찾을 수 없습니다");
        }
#endif
        
        // 하위로 입력을 받는 모듈 선언
        private PlayerMovement _movement;
        private PlayerCombat _combat;
        // 상호작용
        // 카메라 관련 입력 추가시 생성
        // private PlayerCamera _camera;
        
        // 하위 모듈 생성자 할당 및 초기화
        public void Initialize(PlayerMovement move, PlayerCombat cb)
        {
            // 생성된 모듈 연결
            _movement = move;
            _combat = cb;
            // _camera = c; _interactor = i;
            BindEvents();
        }
        
        // 이동 관련 이벤트 할당
        void BindEvents()
        {
            _input.Enable();
            _input.onMove          += OnMove;
            _input.onJump          += OnJump; 
            _input.onSprintChanged += OnSprintChanged;
            _input.onAttack        += OnAttack;
        }
        
        // 이동 관련 이벤트 할당 해제(메모리 누수 방지)
        private void OnDestroy()
        {
            // _input이 없다면 할당 해제할 이벤트도 없으므로 return
            if (_input == null) return;
            _input.onMove          -= OnMove;
            _input.onJump          -= OnJump;
            _input.onSprintChanged -= OnSprintChanged;
            _input.onAttack        -= OnAttack;
            // _input.onStartInteract    -= OnInteractStart;
            // _input.onCanceledInteract -= OnInteractCancel;
        }
        
        // 이벤트 할당 함수(람다식 연결)
        private void OnMove(Vector2 v) => _movement?.SetMoveInput(v);
        private void OnJump() => _movement?.RequestJump();
        private void OnSprintChanged(bool b) => _movement?.SetSprint(b);
        private void OnAttack()
        {
            _combat?.RequestAttack();
        }
        // private void OnInteractStart() => _interactor?.OnInteractStart();
        // private void OnInteractCancel() => _interactor?.OnInteractCancel();
    }
}
