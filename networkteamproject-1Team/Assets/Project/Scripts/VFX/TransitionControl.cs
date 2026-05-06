using Michsky.UI.Dark;
using UnityEngine;

/// <summary>
/// SplashScreenManager의 transitionHelper(UIDissolveEffect)를 원하는 타이밍에 직접 제어하는 헬퍼
/// 사용:
///   1. transitionHelper 필드에 SplashScreenManager가 사용하는 UIDissolveEffect 연결
///   2. 코드에서 TransitionController.Instance.Play~ 호출
/// </summary>

namespace VFX
{
    public class TransitionControl : MonoBehaviour
    {
        public static TransitionControl Instance { get; private set; }

        [SerializeField] UIDissolveEffect _transitionHelper;
        [Range(0.03f, 2f)]
        [SerializeField] float _animationSpeed = 0.11f;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// 화면을 덮으며 나타나는 전환 (Dissolve In: location 1 → 0)
        /// </summary>
        public void PlayIn()
        {
            if (_transitionHelper == null) return;
            Prepare();
            _transitionHelper.DissolveIn();
        }

        /// <summary>
        /// 화면을 걷어내는 전환 (Dissolve Out: location 0 → 1)
        /// </summary>
        public void PlayOut()
        {
            if (_transitionHelper == null) return;
            Prepare();
            _transitionHelper.DissolveOut();
        }

        /// <summary>
        /// 애니메이션 속도를 설정한 뒤 오브젝트 활성화
        /// </summary>
        private void Prepare()
        {
            _transitionHelper.animationSpeed = _animationSpeed;
            _transitionHelper.gameObject.SetActive(true);
        }
    }
}
