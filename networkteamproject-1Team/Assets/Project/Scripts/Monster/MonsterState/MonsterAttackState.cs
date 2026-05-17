using UnityEngine;

namespace Monster
{
    public class MonsterAttackState : IState
    {
        private MonsterController _monsterController;
        private float _timer;

        public MonsterAttackState(MonsterController monsterController)
        {
            _monsterController = monsterController;
        }
    
        public void Enter()
        {
            _monsterController.MonsterAI.Agent.ResetPath();
            _timer = 0f;
            AttackCheck();
        }

        public void Update()
        {
            _timer += Time.deltaTime;
        
            _monsterController.MonsterAttack.LookAtTarget();

            if (_monsterController.MonsterData.isAttacking) return;
        
            if (_monsterController.MonsterAI.Target == null)
            {
                _monsterController.ChangeState(StateType.Patrol);
                return;
            }
        
            if (_monsterController.DistanceToPlayer() > _monsterController.MonsterData.attackRange)
            {
                _monsterController.ChangeState(StateType.Chase);
                return;
            }
        
            if (_timer >= _monsterController.MonsterData.attackCooldown)
            {
                AttackCheck(); 
                _monsterController.attackRand.Value = UnityEngine.Random.Range(0, 2);
                _monsterController.TriggerAttackClientRpc();
                _timer = 0;
            }
        }

        public void Exit()
        {
        }

        private void AttackCheck()
        {
            _monsterController.MonsterData.isAttacking = true;
            _monsterController.AttackCor();
        }
    }
}