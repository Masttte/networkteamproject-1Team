using UnityEngine;
using UnityEngine.Audio;

namespace Monster
{
    public class MonsterAnimationEvents : MonoBehaviour
    {
        private MonsterController _monsterController;

        [SerializeField] AudioResource _attackA;
        [SerializeField] AudioResource _attackB;
        [SerializeField] AudioResource _footStep;

        private void Awake()
        {
            _monsterController = GetComponentInParent<MonsterController>();
        }

        public void OnAttackHit()
        {
            _monsterController.OnAttackHit();
            if (LocalManager.Instance.IamB) AudioManager.Instance.PlaySfxWet(_attackB, transform.position);
            else AudioManager.Instance.PlaySfxWet(_attackA, transform.position);
        }

        public void OnFootStep()
        {
            AudioManager.Instance.PlaySfxWet(_footStep, transform.position);
        }
    }
}
