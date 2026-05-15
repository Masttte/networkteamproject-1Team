using System;
using Unity.Netcode;
using UnityEngine;

namespace Monster
{
    public class MonsterAttack : NetworkBehaviour
    {
        private MonsterController _monsterController;
        private float _timer;
        private Ray _ray;

        public override void OnNetworkSpawn()
        {
            Init();
        }

        private void Init()
        {
            _monsterController = GetComponent<MonsterController>();
            // 테스트를 위해 임시로 Player
        }
    
        public void Attack()
        {
            if (!IsServer) return;
        
            _ray = new Ray(transform.position + _monsterController.MonsterData.offset, transform.forward);
        
            if (Physics.SphereCast(_ray, 0.3f, out RaycastHit hit, _monsterController.MonsterData.attackRange))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_monsterController.MonsterData.attackDamage);
                }
            }
        
        }

        public void LookAtTarget()
        {
            Transform target = _monsterController.MonsterAI.Target;
            if (target == null) return;
        
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
        
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
        }
    }
}
