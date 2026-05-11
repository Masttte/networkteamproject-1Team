using UnityEngine;

namespace UI
{
    public class WorldSpaceNameBillboard : MonoBehaviour
    {
        Camera _targetCamera;

        private void Awake()
        {
            _targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            transform.rotation = _targetCamera.transform.rotation;
        }
    }
}
