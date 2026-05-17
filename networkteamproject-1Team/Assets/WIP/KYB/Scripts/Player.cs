using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KYB
{
    // 플레이어 (임시, 테스트용)
    public class Player : NetworkBehaviour
    {
        public BattleInputReader input;

        private PlayerInput _playerInput;
        public Rigidbody _rb;
        private Vector2 _input;
        public Camera cam;

        [SerializeField] private float moveSpeed;
        
        [Header("레이 사거리")] public float interactionDistance = 3.0f;

        private IInteractable _interactableTarget;
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                GetComponentInChildren<Camera>().gameObject.SetActive(false);
            }

            cam = GetComponentInChildren<Camera>();
            _playerInput = GetComponent<PlayerInput>();
            _rb = GetComponent<Rigidbody>();

            input.Enable();
            input.onStartInteract += OnStartInteractive;
            input.onCanceledInteract += OnCanceledInteractive;
            input.onMove += OnMove;
        }

        public override void OnNetworkDespawn()
        {
            input.onMove -= OnMove;
            input.onStartInteract -= OnStartInteractive;
            input.onCanceledInteract -= OnCanceledInteractive;
        }

        private void OnMove(Vector2 moveInput)
        {
            if (!IsOwner)
            {
                Debug.Log("[테스트]: IsOwner가 아닙니다.");
                return;
            }
            _input = moveInput;
        }
        
        private void FixedUpdate()
        {
            _rb.linearVelocity = new Vector3(_input.x * moveSpeed, _rb.linearVelocity.y, _input.y * moveSpeed);
        }
        

        private void OnStartInteractive()
        {
            if (!IsOwner) return;

            _interactableTarget = InteractiveObject();

            if (_interactableTarget != null)
            {
                _interactableTarget.InteractStart();
            }
        }

        private void OnCanceledInteractive()
        {
            if (!IsOwner) return;

            _interactableTarget = InteractiveObject();

            if (_interactableTarget != null)
            {
                _interactableTarget.InteractStop();
            }
        }


        /// <summary>
        /// 상호작용이 가능한 오브젝트를 return 해주는 메서드
        /// </summary>
        /// <returns>상호작용이 가능한 오브젝트</returns>
        private IInteractable InteractiveObject()
        {
            if (cam == null)
            {
                Debug.Log("[cam] cam이 null입니다.");
            }

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                // 부딪힌 물체에 IInteractable이 가능한지 확인
                IInteractable target = hit.collider.GetComponentInParent<IInteractable>();

                if (target != null)
                {
                    return target;
                }
            }

            return null;
        }

        private void OnDrawGizmos()
        {
            if (cam == null) return;

            Gizmos.color = Color.red;

            Transform camTransform = cam.transform;
            Gizmos.DrawRay(camTransform.position, camTransform.forward * interactionDistance);
        }
    }
}