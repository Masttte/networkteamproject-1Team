using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using LitMotion;
using VFX;

namespace Battle
{
    public class PlayerEntity : EntityBase
    {
        PlayerCombat _combat;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            onDeath += AlertDeath;

            // 서버에서만 사망 후 처리 흐름 관리
            if (IsServer)
            {
                _combat = GetComponent<PlayerCombat>();
                _combat.OnDeathAnimFinished += FinalizeDeath;
            }

            if (IsOwner)
            {
                if (VFXManager.Instance != null)
                    CurHp.OnValueChanged += HandleHpChanged;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            onDeath -= AlertDeath;
            if (IsServer) _combat.OnDeathAnimFinished -= FinalizeDeath;
            if (IsOwner) CurHp.OnValueChanged -= HandleHpChanged;
        }

        void HandleHpChanged(int prev, int next)
        {
            if (next > 0)
                VFXManager.Instance.PlayHitVFX();
        }

        void AlertDeath()
        {
            if (!IsServer) return;

            NotifyDeathClientRpc();
        }

        void FinalizeDeath()
        {
            if (!IsServer) return;
            BattleManager.Instance.DestroyPlayer(this);
        }

        [ClientRpc]
        void NotifyDeathClientRpc()
        {
            if (IsOwner) YouDied(); // 오너만 사망 연출
        }
        void YouDied()
        {
            VFXManager.Instance.PlayDeathVFX();
        }
    }
}
