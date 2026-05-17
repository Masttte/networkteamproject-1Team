using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace Battle
{
    public abstract class EntityBase : NetworkBehaviour, IDamageable
    {
        public int maxHp = 100;
        public NetworkVariable<int> CurHp = new NetworkVariable<int>(0);

        bool _isDead;
        public bool IsDead
        {
            get => _isDead;
            set
            {
                if (!EqualityComparer<bool>.Default.Equals(_isDead, value))
                {
                    _isDead = value;
                    if (_isDead) onDeath?.Invoke(); // IsDead가 ture면 자동으로 onDeath 실행
                }
            }
        }
        public event Action onDeath; // 자식에서 구독

        public override void OnNetworkSpawn()
        {
            if (IsServer) CurHp.Value = maxHp; // NetworkVariable은 서버만 초기화 해도 클라이언트에 동기화
            //CurHp.OnValueChanged += OnHpChanged;
        }

        //public override void OnNetworkDespawn()
        //{
        //    CurHp.OnValueChanged -= OnHpChanged;
        //}

        //void OnHpChanged(int prev, int next)
        //{
        //    Debug.LogWarning("누군가 데미지 입음");
        //}

        // 서버에서 호출
        public virtual void TakeDamage(int damage) // Vector3 hitPoint, Vector3 hitNormal (추가 가능 구현시)
        {
            if (IsDead) return;
            CurHp.Value -= damage;
            if (CurHp.Value <= 0) IsDead = true;
        }
    }
}
