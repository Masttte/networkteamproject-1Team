using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Player
{
    /// <summary>
    /// 모델 루트(A, B)에 부착. Animator/NetworkAnimator/Rig는 항상 활성 유지하면서
    /// 시각 표현(Renderer, DecalProjector)만 토글한다.
    /// SetActive(false)를 쓰면 Owner의 Animator가 멈춰 NetworkAnimator 동기화가
    /// 끊기는 문제를 회피하기 위함.
    /// </summary>
    public class ModelVisual : MonoBehaviour
    {
        private Renderer[] _renderers;
        private DecalProjector[] _decalProjectors;
        
        private bool _cached;

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            if (_cached) return;
            
            _renderers       = GetComponentsInChildren<Renderer>(true);
            _decalProjectors = GetComponentsInChildren<DecalProjector>(true);
            
            _cached = true;
        }

        public void SetVisible(bool visible)
        {
            CacheComponents();
            
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].enabled = visible;
            
            for (int i = 0; i < _decalProjectors.Length; i++)
                _decalProjectors[i].enabled = visible;
        }
    }
}