using UnityEngine;

namespace Player
{
    public class PlayerAnimationEvent : MonoBehaviour
    {
        [SerializeField] private PlayerAnimation _playerAnimation;
    
        // Animator의 Animation Event에서 호출 (Punching 클립 끝 프레임)
        public void OnAttackAnimEnd() => _playerAnimation.Combat?.HandleAttackAnimEnd();
    
        // Animator의 Animation Event에서 호출 (Getting Hit 클립 끝 프레임)
        public void OnHitAnimEnd() => _playerAnimation.Combat?.HandleHitAnimEnd();
        
        // Animator의 Animation Event에서 호출 (Dying Backwards 클립 끝 프레임)
        public void OnDeathAnimEnd() => _playerAnimation.Combat?.HandleDeathAnimEnd();
    }
}
