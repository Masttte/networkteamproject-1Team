using System.Collections.Generic;
using UnityEngine;

namespace Monster
{
    [System.Serializable]
    public class MonsterData
    {
        public List<Vector3> patrolPoints = new List<Vector3>();
        public float speed;
        public Vector3 offset;
        public float idleTime;
        public float chaseRange;
        public float attackRange;
        public int attackDamage;
        public float attackCooldown;
        public bool isAttacking;
    }
}
