using UnityEngine;

// TODO: 미사용. 팀원 IInteractable이 IInteractor를 받지 않아 활용처 없음.
// 미래 조준 기반 상호작용 등 추가 시 활용 검토.
namespace Player
{
    // 상호작용 관련 인터페이스
    public interface IInteractor
    {
        bool IsInteracting { get; }
        ulong OwnerClientId { get; }
        Vector3 OriginPosition { get; }
        Vector3 OriginForward { get; }
    
        void OnInteractStart();
        void OnInteractTick();
        void OnInteractCancel();
    }
}