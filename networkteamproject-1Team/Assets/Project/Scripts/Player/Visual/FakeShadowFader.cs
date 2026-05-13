// 캐릭터에 부착, Decal Projector 자식 참조
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Player
{
    [RequireComponent(typeof(DecalProjector))]
    public class FakeShadowFader : MonoBehaviour
    {
        [SerializeField] DecalProjector _decal;
        [SerializeField] float _maxHeight = 1f;
        [SerializeField] LayerMask _groundLayer;
        
        private void Awake()
        {
            if (_decal == null)
                _decal = GetComponentInChildren<DecalProjector>();
        }
        
        // 바닥과 거리에 따라 데칼을 페이드해서 자연스러운 Fake Shadow 연출
        void LateUpdate()
        {
            if (_decal == null) return;
            
            if (Physics.Raycast(transform.position, Vector3.down,
                    out RaycastHit hit, _maxHeight, _groundLayer))
            {
                // fadeFactor: 거리 따라 페이드
                float distance = hit.distance;
                _decal.fadeFactor = 1f - (distance / _maxHeight);
                
                // Depth: 실제 바닥까지 + 약간만
                var size = _decal.size;
                size.z = distance + 0.2f;
                _decal.size = size;
                
                // Pivot Z: 박스가 캐릭터부터 바닥까지만 영향
                var pivot = _decal.pivot;
                pivot.z = size.z * 0.5f;
                _decal.pivot = pivot;
            }
            else
            {
                _decal.fadeFactor = 0f;
            }
        }
    }
}