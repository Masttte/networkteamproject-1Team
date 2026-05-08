using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace Battle
{
    public class Weapon : NetworkBehaviour
    {
        static readonly Collider[] s_AttackResults = new Collider[16];

        public enum State
        {
            None, Ready,//Empty, Reloading,
        }
        State _state;
        public WeaponSO weaponSO;

        [SerializeField] Transform _attackPoint;
        float _lastAttackTime;

        public BattleInputReader input;
#if UNITY_EDITOR
        private void Reset()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BattleInputReader");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                input = UnityEditor.AssetDatabase.LoadAssetAtPath<BattleInputReader>(path);
            }
            string[] guids2 = UnityEditor.AssetDatabase.FindAssets("t:WeaponSO");
            if (guids2.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids2[0]);
                weaponSO = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponSO>(path);
            }
        }
#endif
        public override void OnNetworkSpawn()
        {
            // 모든 인스턴스에서 OnGameStart 구독
            BattleManager.Instance.OnGameStart += Ready;
            if (!IsOwner) return;
            
            input.Enable();
        }
        public override void OnNetworkDespawn()
        {
            // 모든 인스턴스에서 OnGameStart 구독 해제
            BattleManager.Instance.OnGameStart -= Ready;
            if (!IsOwner) return;
        }

        void Ready() => _state = State.Ready;

        public bool IsReady => _state == State.Ready
                               && Time.time >= _lastAttackTime + weaponSO.cooltime;
        
        // 서버에서만 호출. PlayerCombat이 ServerRpc 안에서 호출.
        public void TryAttack()
        {
            if (!IsServer) return;       // 서버 전용
            if (!IsReady) return;
    
            _lastAttackTime = Time.time;
            Attack();
        }
        
        // AttackOnServer
        public void Attack()
        {
            var origin = _attackPoint.position;
            var forward = transform.forward;
            var hitCount = Physics.OverlapSphereNonAlloc(origin, weaponSO.range, s_AttackResults);

            Collider bestCollider = null;
            float bestDistance = float.MaxValue;
            var halfAngle = weaponSO.angle * 0.5f;

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = s_AttackResults[i];
                s_AttackResults[i] = null;

                if (candidate == null)
                    continue;

                if (candidate.transform == transform || candidate.transform.IsChildOf(transform))
                    continue;

                var closestPoint = candidate.ClosestPointOnBounds(origin);
                var toTarget = closestPoint - origin;
                var distance = toTarget.magnitude;

                // origin이 콜라이더 내부에 있는 경우(distance == 0) 중심 방향으로 판정
                var dirToTarget = distance > 0.001f ? toTarget.normalized : (candidate.bounds.center - origin).normalized;

                if (distance > weaponSO.range)
                    continue;

                if (Vector3.Angle(forward, dirToTarget) > halfAngle)
                    continue;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCollider = candidate;
                }

            }

            if (bestCollider == null)
            {
                BroadcastMissClientRpc(origin);
                return;
            }

            var hitPoint = bestCollider.ClosestPoint(origin);
            
            // 맞았지만 NetworkObject가 없음: Blocked (히트 위치로 전파)
            NetworkObject targetNetObj = bestCollider.GetComponent<NetworkObject>();
            if (targetNetObj == null)
            {
                BlockedClientRpc(hitPoint);
                return;
            }

            // 네트워크 오브젝트에 명중 (히트 위치로 전파): Hit
            if (targetNetObj.TryGetComponent(out PlayerEntity damageable))
            {
                if (damageable.IsDead) return;
                damageable.TakeDamage(weaponSO.damage);
            }
            
            AttackClientRpc(hitPoint);
        }
        
        // 서버에서 처리하니 ClientRpc로 모든 클라에게 전파 (Miss 사운드도 모두에게 들림)
        [ClientRpc]
        void BroadcastMissClientRpc(Vector3 attackPoint)
        {
            AudioManager.Instance.PlaySfxWet(weaponSO.attackMiss, attackPoint);
        }
        
        // BlockedServerRpc 제거 (ClientRpc만 남김, 서버에서 직접 호출)
        /*[ServerRpc(RequireOwnership = false)]
        void BlockedServerRpc(Vector3 hitPoint) => BlockedClientRpc(hitPoint);*/
        
        
        [ClientRpc]
        void BlockedClientRpc(Vector3 hitPoint) => AudioManager.Instance.PlaySfxWet(weaponSO.attackBlocked, hitPoint);

        // AttackServerRpc 제거 (서버 직접 처리로 대체)
        /*[ServerRpc(RequireOwnership = false)]
        void AttackServerRpc(ulong targetId, int damage, Vector3 hitPoint)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetNetObj);
            if (targetNetObj.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(damage);

            AttackClientRpc(OwnerClientId, damage, targetNetObj.OwnerClientId, hitPoint);
        }*/
        
        [ClientRpc]
        void AttackClientRpc(Vector3 hitPoint)
        {
            // 타격 위치 기준 3D 공간음 재생
            AudioManager.Instance.PlaySfxWet(weaponSO.attackHit, hitPoint);
        }
    }
}
