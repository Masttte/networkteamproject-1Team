using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.Rendering;
using UnityEngine.Audio;

public class TEST_PlayerMove : NetworkBehaviour, INetworkUpdateSystem
{
    [Header("Move")]
    [SerializeField] float _moveSpeed = 4f;
    [SerializeField] float _sprintMultiplier = 2f;
    [SerializeField] float _jumpHeight = 4.2f;
    [SerializeField] float _rotationSmoothTime = 0.12f;

    [Header("Camera")]
    [SerializeField] GameObject _cameraPos;
    [SerializeField] SkinnedMeshRenderer _headMesh;
    [SerializeField] float _upClamp = -40f;
    [SerializeField] float _downClamp = 70f;
    //[SerializeField] float _leftClamp = -90f;
    //[SerializeField] float _rightClamp = 90f;
    [SerializeField] float _mouseSensitivity = 0.3f;

    [Header("Jump")]
    public float jumpCooldown = 1.5f;
    float _lastJumpTime;

    // 컴포넌트
    Animator _ac;
    CharacterController _controller;

    // 내부 상태
    Vector2 _moveInput;
    float _initialYaw;
    float _yaw;
    float _pitch;
    float _rotationVelocity;

    // 애니메이션 해시
    int _animIDSpeed;
    int _animIDGrounded;
    int _animIDJump;
    int _animIDFreeFall;
    int _animIDMotionSpeed;

    // 중력
    float _verticalVelocity;
    float _gravity = -9.81f;

    public BattleInputReader input;
#if UNITY_EDITOR
    private void Reset()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BattleInputReader");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            input = UnityEditor.AssetDatabase.LoadAssetAtPath<BattleInputReader>(path);
        }
        else
        {
            Debug.LogWarning("BattleInputReader SO를 찾을 수 없습니다");
        }
    }
#endif
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _ac = GetComponent<Animator>();
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

        input.Enable();
        input.onMove += OnMove;
        input.onJump += OnJump;

        _initialYaw = _cameraPos.transform.rotation.eulerAngles.y;
        _yaw = _initialYaw;

        SetupCinemachineCamera();

        this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _camObj.transform.SetParent(null); // 플레이어가 파괴될 때 시네머신/카메라가 같이 파괴되지 않도록 최상단으로 분리

        input.onMove -= OnMove;
        input.onJump -= OnJump;

        this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
    }

    public void NetworkUpdate(NetworkUpdateStage updateStage)
    {
        if (!IsOwner) return;

        if (updateStage == NetworkUpdateStage.Update)
        {
            MovePlayer();
            ApplyGravity();
            RotateCamera();
        }
    }


    // ────────────────────────────────────────────
    GameObject _camObj;
    void SetupCinemachineCamera()
    {
        _camObj = GameObject.FindWithTag("GameController");

        _camObj.transform.SetParent(_cameraPos.transform, false);
        _camObj.transform.localPosition = Vector3.zero;
        _camObj.transform.localRotation = Quaternion.identity;

        // 자기 머리 보이지 않도록 설정
        _headMesh.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        //Cursor.lockState = CursorLockMode.Locked;
    }



    private void OnMove(Vector2 value) => _moveInput = value;

    private void OnJump()
    {
        if (Time.time >= _lastJumpTime + jumpCooldown)
        {
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _lastJumpTime = Time.time;
            _ac.SetBool(_animIDJump, true);
        }
    }

    private void MovePlayer()
    {
        float targetSpeed = _moveInput == Vector2.zero ? 0f : _moveSpeed * (input.isSprint ? _sprintMultiplier : 1f);

        // 캐릭터가 카메라 방향 바라보게
        float cameraYaw = _cameraPos.transform.eulerAngles.y;
        float smoothAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y, cameraYaw, ref _rotationVelocity, _rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

        Vector3 inputDir = (transform.right * _moveInput.x + transform.forward * _moveInput.y).normalized;

        _controller.Move(inputDir * (targetSpeed * Time.deltaTime)
                         + Vector3.up * (_verticalVelocity * Time.deltaTime));

        _ac.SetFloat(_animIDSpeed, targetSpeed);

        // 애니 블렌드 트리 속도 제어
        float MotionSpeed = _moveInput == Vector2.zero ? 0f : 1f;
        _ac.SetFloat(_animIDMotionSpeed, MotionSpeed);
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _verticalVelocity <= 0.0f)
        {
            _ac.SetBool(_animIDGrounded, true);
            _ac.SetBool(_animIDFreeFall, false);
            _ac.SetBool(_animIDJump, false);

            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f;
        }
        else
        {
            _ac.SetBool(_animIDGrounded, false);
            if (_verticalVelocity < 0f)
                _ac.SetBool(_animIDFreeFall, true);

            _verticalVelocity += _gravity * Time.deltaTime;
        }
    }

    private void RotateCamera()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        _yaw += mouseDelta.x * _mouseSensitivity;
        //_yaw = Mathf.Clamp(_yaw, _initialYaw + _leftClamp, _initialYaw + _rightClamp);

        _pitch -= mouseDelta.y * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, _upClamp, _downClamp);

        _cameraPos.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
}
