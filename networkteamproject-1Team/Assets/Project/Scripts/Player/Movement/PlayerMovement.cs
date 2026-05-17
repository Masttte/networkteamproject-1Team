using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerCamera))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 4f;
        [SerializeField] private float _sprintMultiplier = 1.5f;
        [SerializeField] private float _rotationSmoothTime = 0.12f;

        [Header("Jump")]
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _jumpCooldown = 0.5f;
        
        private const float Gravity = -9.81f;
        
        private CharacterController _controller;
        // 카메라 연결
        private PlayerCamera _camera;
        // 전투 연결
        private PlayerCombat _combat;
        
        // 이동 연산에 사용되는 변수
        private Vector2 _moveInput;
        private bool _isSprinting;
        private bool _jumpRequested;
        private float _lastJumpTime;
        private float _rotationVelocity;
        private float _currentSpeed;
        
        // 외부 공유용 변수
        public bool IsGrounded => _controller.isGrounded;
        public float CurrentSpeed => _currentSpeed;
        public float MotionSpeed => _moveInput == Vector2.zero ? 0f : 1f;
        public bool JustJumped { get; private set; }
        public float VerticalVelocity { get; private set; }

        // 캐싱
        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _camera = GetComponent<PlayerCamera>();
            _combat = GetComponent<PlayerCombat>();
        }
        
        // 설정
        public void SetMoveInput(Vector2 input) => _moveInput = input;
        public void SetSprint(bool sprint) => _isSprinting = sprint;
        public void RequestJump() => _jumpRequested = true;

        private void Update()
        {
            // 사망 상태일시 입력 차단
            bool canMove = _combat == null || _combat.CanMove;
            
            HandleRotation();
            HandleJumpAndGravity(canMove);
            HandleMove(canMove);
        }
        
        private void HandleRotation()
        {
            // 회전 구현
            // 카메라 yaw 따라 캐릭터 본체 회전
            float cameraYaw = _camera.YawAngle;
            float smoothAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, cameraYaw, ref _rotationVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }
        
        private void HandleJumpAndGravity(bool canMove)
        {
            JustJumped = false;
            
            if (_controller.isGrounded)
            {
                if (VerticalVelocity < 0f) VerticalVelocity = -2f;
                
                if (canMove && _jumpRequested && Time.time >= _lastJumpTime + _jumpCooldown)
                {
                    VerticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * Gravity);
                    _lastJumpTime = Time.time;
                    JustJumped = true;
                }
            }
            else
            {
                VerticalVelocity += Gravity * Time.deltaTime;
            }
            _jumpRequested = false;
        }
        
        private void HandleMove(bool canMove)
        {
            float targetSpeed = (!canMove || _moveInput == Vector2.zero)
                ? 0f
                : _moveSpeed * (_isSprinting ? _sprintMultiplier : 1f);
            _currentSpeed = targetSpeed;
            
            Vector3 inputDir = canMove
                ? (transform.right * _moveInput.x + transform.forward * _moveInput.y).normalized
                : Vector3.zero;
            
            CollisionFlags flags = _controller.Move(
                inputDir * (targetSpeed * Time.deltaTime)
                + Vector3.up * (VerticalVelocity * Time.deltaTime));
    
            // 천장 충돌 시 즉시 낙하 전환
            if ((flags & CollisionFlags.Above) != 0 && VerticalVelocity > 0f)
                VerticalVelocity = 0f;
        }
    }
}