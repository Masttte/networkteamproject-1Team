using UnityEngine;

namespace UI
{
    public class WorldSpaceNameBillboard : MonoBehaviour
    {
        Transform _cameraTransform;

        private void Start()
        {
            var cam = Camera.main;
            _cameraTransform = cam != null ? cam.transform : null;
        }

        private void OnDisable()
        {
            _cameraTransform = null;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) // 재시작 오류로 널 채크 추가...
            {
                var cam = Camera.main;
                if (cam == null) return;
                _cameraTransform = cam.transform;
            }

            transform.rotation = _cameraTransform.rotation;
        }
    }
}
