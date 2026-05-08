using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monster
{
    public class MonsterIdleState : IState
    {
        private MonsterController _monsterController;
        private float _timer;

        public MonsterIdleState(MonsterController monsterController)
        {
            _monsterController = monsterController;
        }
    
        public void Enter()
        {
            _timer = 0f;
        }

        public void Update()
        {
            if (_monsterController.Prison == null) return;
            if (!_monsterController.Prison.Unlocked) return;
            
            _timer += Time.deltaTime;
        
            if (_timer > _monsterController.MonsterData.idleTime)
            {
                _monsterController.ChangeState(StateType.Patrol);
            }
        }

        public void Exit()
        {
            _timer = 0f;
        }
    }
}
