using System;
using UnityEngine;

namespace Player
{
    [Flags]
    public enum InputCategory
    {
        None     = 0,
        Movement = 1 << 0,    // 이동 (WASD)
        Camera   = 1 << 1,    // 카메라 회전 (마우스)
        Jump     = 1 << 2,    // 점프 (Space)
        Sprint   = 1 << 3,    // 달리기 (Shift)
        Combat   = 1 << 4,    // 공격 (마우스 클릭)
        Interact = 1 << 5,    // 상호작용 (E)
    
        All = Movement | Camera | Jump | Sprint | Combat | Interact
    }
    
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
        private PlayerCamera _camera;
        // 상호작용
        // private PlayerInteractor _interactor;
        // 카메라 관련 입력 추가시 생성
        // 
        
        private InputCategory _enabledInputs = InputCategory.None;
        public InputCategory EnabledInputs => _enabledInputs;
        
        // 하위 모듈 생성자 할당 및 초기화
        public void Initialize(PlayerMovement move, PlayerCamera cam, PlayerCombat cb)
        {
            // 생성된 모듈 연결
            _movement = move;
            _camera = cam;
            _combat = cb;
            // _interactor = i;
            BindEvents();
        }
        
        // ===== 입력 활성/비활성 제어 =====

        public void EnableInput(InputCategory category)
        {
            // 비트 OR 연산 - 특정 카테고리를 '켬' 상태로 만듬
            _enabledInputs |= category;
        }

        public void DisableInput(InputCategory category)
        {
            // 비트 NOT 연산 - 해당 카테고리 비트만 0으로 만든 뒤 AND 연산
            _enabledInputs &= ~category;
            
            // 비활성 시 즉시 이동 정지 (입력 잔류 방지)
            if((category & InputCategory.Movement) !=0)
                _movement?.SetMoveInput(Vector2.zero);
            if((category & InputCategory.Sprint) !=0)
                _movement?.SetSprint(false);
            if((category & InputCategory.Camera) != 0)
                _camera?.SetLookInput(Vector2.zero);
        }
        
        public bool IsEnabled(InputCategory category)
        {
            return (_enabledInputs & category) == category;
        }
        
        // ===== 입력 라우팅 (각 카테고리별 게이트 체크) =====
        
        // 이동 관련 이벤트 할당
        void BindEvents()
        {
            _input.Enable();
            _input.onMove          += OnMove;
            _input.onJump          += OnJump; 
            _input.onSprintChanged += OnSprintChanged;
            _input.onAttack        += OnAttack;
            _input.onLook          += OnLook;
            // _input.onStartInteract += OnInteractStart;
            // _input.onCanceledInteract += OnInteractCancel;
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
            _input.onLook          -= OnLook;
            // _input.onStartInteract    -= OnInteractStart;
            // _input.onCanceledInteract -= OnInteractCancel;
        }
        
        // 이벤트 할당 함수
        private void OnMove(Vector2 v)
        {
            if(!IsEnabled(InputCategory.Movement)) return;
            _movement?.SetMoveInput(v);
        }

        private void OnJump()
        {
            if(!IsEnabled(InputCategory.Jump)) return;
            _movement?.RequestJump();
        }

        private void OnSprintChanged(bool b)
        {
            if(!IsEnabled(InputCategory.Sprint)) return;
            _movement?.SetSprint(b);
        }

        private void OnAttack()
        {
            if(!IsEnabled(InputCategory.Combat)) return;
            _combat?.RequestAttack();
        }

        private void OnLook(Vector2 delta)
        {
            if(!IsEnabled(InputCategory.Camera)) return;
            _camera?.SetLookInput(delta);
        }
        
        // private void OnInteractStart()
        // {
        //     if (!IsEnabled(InputCategory.Interact)) return;
        //     _interactor?.OnInteractStart();
        // }
        
        // private void OnInteractStart() => _interactor?.OnInteractStart();
        // private void OnInteractCancel() => _interactor?.OnInteractCancel();
    }
}
