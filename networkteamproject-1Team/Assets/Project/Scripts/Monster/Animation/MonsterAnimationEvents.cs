using UnityEngine;

namespace Monster
{
    public class MonsterAnimationEvents : MonoBehaviour
    {
        private MonsterController _monsterController;

        private void Awake()
        {
            _monsterController = GetComponentInParent<MonsterController>();
        }

        public void OnAttackHit()
        {
            _monsterController.OnAttackHit();
        }
    }
}
