using UnityEngine;

namespace Monster
{
    public class MonsterPatrolState : IState
    {
        private MonsterController _monsterController;
        private int _patrolIndex;

        public MonsterPatrolState(MonsterController monsterController)
        {
            _monsterController = monsterController;
        }
    
        public void Enter()
        {
            _monsterController.MonsterAI.Agent.ResetPath();
        }

        public void Update()
        {
            if (!_monsterController.MonsterAI.Agent.pathPending &&
                _monsterController.MonsterAI.Agent.remainingDistance < 0.2f)
            {
                Patrol();
            }
        
            if (_monsterController.MonsterAI.Target != null)
            {
                _monsterController.ChangeState(StateType.Chase);
            }
        }

        public void Exit()
        {
        
        }

        private void Patrol()
        {
            if (_monsterController.MonsterData.patrolPoints == null || _monsterController.MonsterData.isAttacking) return;
        
            _monsterController.MonsterAI.Agent.SetDestination(
                _monsterController.MonsterData.patrolPoints[_patrolIndex]);
            _patrolIndex = (_patrolIndex + 1) % _monsterController.MonsterData.patrolPoints.Count;
        }
    }
}
