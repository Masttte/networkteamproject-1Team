using UnityEngine;
using UnityEngine.Audio;

namespace Monster
{
    public class MonsterAnimationEvents : MonoBehaviour
    {
        private MonsterController _monsterController;

        [SerializeField] AudioResource _footStep;

        private void Awake()
        {
            _monsterController = GetComponentInParent<MonsterController>();
        }

        public void OnAttackHit()
        {
            _monsterController.OnAttackHit();
            Debug.LogWarning("몬스터 어택!");
        }

        public void OnFootStep()
        {
            Debug.LogWarning("몬스터 핡!");
            AudioManager.Instance.PlaySfxWet(_footStep, transform.position);
        }
    }
}
