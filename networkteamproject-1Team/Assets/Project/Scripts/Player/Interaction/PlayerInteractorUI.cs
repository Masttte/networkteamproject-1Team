using Interactable;
using Monster;
using TMPro;
using UnityEngine;

namespace Player
{
    // 상호작용 오브젝트의 정보를 얻어 가이드 UI룰 띄우는 상호작용 피드백 UI
    public class PlayerInteractorUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _guideText;

        private string _guideFormat = "[E] {0}";
        
        private PlayerInteractor _interactor;

        public void Init(PlayerInteractor interactor)
        {
            _interactor = interactor;
            _interactor.OnTargetChanged += HandleTargetChanged;
            
            // 초기 상태 동기화
            HandleTargetChanged(interactor.CurrentTarget);
        }

        private void OnDestroy()
        {
            if (_interactor != null)
                _interactor.OnTargetChanged -= HandleTargetChanged;
        }
        
        // 상호작용 중인 대상이 없으면 가이드 패널을 끄고 있다면 라벨값을 얻어와 텍스트를 출력
        private void HandleTargetChanged(IInteractable target)
        {
            if (target == null)
            {
                _panel.SetActive(false);
                return;
            }
            
            _panel.SetActive(true);
            _guideText.text = string.Format(_guideFormat, GetLabel(target));
        }
        
        // 상호작용한 대상의 클래스를 통해 정보를 분류
        // TODO: 팀원 IInteractable 확장 합의 시 type 분기 제거. using도 제거.
        private string GetLabel(IInteractable target)
        {
            return target switch
            {
                Generator => "발전기 수리",
                Prison => "감옥 열기",
                Phone => "전화 받기",
                _ => "상호작용"
            };
        }
    }
}
