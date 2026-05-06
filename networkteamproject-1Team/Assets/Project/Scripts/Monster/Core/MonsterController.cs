using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Monster
{
    public enum StateType
    {
        Idle = 0,
        Patrol = 1,
        Chase = 2,
        Attack = 3
    }
    /// <summary>
    /// 몬스터의 전반적인 기능을 관리
    /// </summary>
    public class MonsterController : NetworkBehaviour
    {
        [field: SerializeField] public MonsterData MonsterData { get; set; }
        private StateMachine _state;
        private LayerMask _layerMask;
        private Animator _anim;
        private PathSettingManager _path;

        public NetworkVariable<StateType> currentState = new NetworkVariable<StateType>(
            writePerm: NetworkVariableWritePermission.Server);

        public NetworkVariable<int> attackRand = new NetworkVariable<int>(
            writePerm: NetworkVariableWritePermission.Server);

        public MonsterAI MonsterAI { get; private set; }
        public MonsterAttack MonsterAttack { get; private set; }

        public MonsterIdleState IdleState { get; private set; }
        public MonsterPatrolState PatrolState { get; private set; }
        public MonsterChaseState ChaseState { get; private set; }
        public MonsterAttackState AttackState { get; private set; }

        public override void OnNetworkSpawn()
        {
            Init();

            if (!IsServer)
            {
                MonsterAI.enabled = false;
            }

            currentState.OnValueChanged += OnStateChanged;
            OnStateChanged(currentState.Value, currentState.Value);
        }

        private void Update()
        {
            if (!IsServer) return;

            _state.Update();
        }

        public override void OnNetworkDespawn()
        {
            currentState.OnValueChanged -= OnStateChanged;
        }

        private void Init()
        {
            _state = new StateMachine();
            MonsterAI = GetComponent<MonsterAI>();
            MonsterAttack = GetComponent<MonsterAttack>();
            _anim = GetComponentInChildren<Animator>();
            _layerMask = LayerMask.GetMask("Player");
            _path = FindAnyObjectByType<PathSettingManager>();
            PathSet();

            IdleState = new MonsterIdleState(this);
            PatrolState = new MonsterPatrolState(this);
            ChaseState = new MonsterChaseState(this);
            AttackState = new MonsterAttackState(this);

            if (MonsterData.speed != 0f) MonsterAI.Agent.speed = MonsterData.speed;
            if (IsServer)
            {
                _state.ChangeState(IdleState);
                currentState.Value = StateType.Idle;
            }
        }

        public Transform DetectPlayer()
        {
            Collider[] colliders =
                Physics.OverlapSphere(transform.position + MonsterData.offset, MonsterData.chaseRange);

            Transform target = null;
            float minDistance = MonsterData.chaseRange;

            foreach (Collider col in colliders)
            {
                col.TryGetComponent(out TeamA playerA);
                if (playerA == null)
                {
                    continue;
                }

                float dis = Vector3.Distance(transform.position, col.transform.position);

                if (dis < minDistance)
                {
                    minDistance = dis;
                    target = col.transform;
                }
            }

            return target;
        }

        public void ChangeState(StateType newState)
        {
            if (currentState.Value == newState) return;

            switch (newState)
            {
                case StateType.Patrol:
                    _state.ChangeState(PatrolState);
                    break;
                case StateType.Chase:
                    _state.ChangeState(ChaseState);
                    break;
                case StateType.Attack:
                    _state.ChangeState(AttackState);
                    break;
            }

            currentState.Value = newState;
        }

        private void OnStateChanged(StateType oldState, StateType newState)
        {
            switch (newState)
            {
                case StateType.Patrol:
                case StateType.Chase:
                    _anim.SetFloat("MoveSpeed", 1f);
                    break;
                case StateType.Attack:
                    _anim.SetFloat("MoveSpeed", 0f);
                    AttackAnimRandServerRpc();
                    _anim.SetTrigger("Attack");
                    break;
            }
        }

        [ServerRpc]
        public void AttackAnimRandServerRpc()
        {
            attackRand.Value = UnityEngine.Random.Range(0, 2);

            AnimRandClientRpc(attackRand.Value);
        }

        [ClientRpc]
        public void AnimRandClientRpc(int rand)
        {
            _anim.SetInteger("AttackRand", rand);
        }

        [ClientRpc]
        public void TriggerAttackClientRpc()
        {
            _anim.SetTrigger("Attack");
        }

        public void OnAttackHit()
        {
            MonsterAttack.Attack();
        }

        public float DistanceToPlayer()
        {
            return Vector3.Distance(transform.position, MonsterAI.Target.position);
        }

        private void PathSet()
        {
            if (_path.pathSettings.Count == 0) return;
            MonsterData.patrolPoints = _path.pathSettings;
        }

        public void AttackCor()
        {
            StartCoroutine(Attacking());
        }

        private IEnumerator Attacking()
        {
            yield return new WaitForSeconds(MonsterData.attackCooldown);

            MonsterData.isAttacking = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawSphere(transform.position + MonsterData.offset, MonsterData.chaseRange);
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            Gizmos.DrawSphere(transform.position + MonsterData.offset, MonsterData.attackRange);
        }
    }
}


