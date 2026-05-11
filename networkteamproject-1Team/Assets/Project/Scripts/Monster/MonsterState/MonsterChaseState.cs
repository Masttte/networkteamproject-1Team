using UnityEngine;

namespace Monster
{
    public class MonsterChaseState : IState
    {
        private MonsterController _monsterController;

        public MonsterChaseState(MonsterController monsterController)
        {
            _monsterController = monsterController;
        }
    
        public void Enter()
        {
            _monsterController.MonsterAI.IsDetected = true;
        }

        public void Update()
        {
            if (_monsterController.MonsterAI.Target == null)
            {
                _monsterController.ChangeState(StateType.Patrol);
                return;
            }

            if (_monsterController.DistanceToPlayer() <= _monsterController.MonsterData.attackRange)
            {
                _monsterController.attackRand.Value = UnityEngine.Random.Range(0, 2);
                _monsterController.ChangeState(StateType.Attack);
            }
        }

        public void Exit()
        {
            _monsterController.MonsterAI.IsDetected = false;
        }
    }
}
