using Player;
using Unity.Netcode;
using UnityEngine;

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
                if (_combat != null)
                    _combat.OnDeathAnimFinished += FinalizeDeath;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            onDeath -= AlertDeath;
            if (IsServer && _combat != null)
                _combat.OnDeathAnimFinished -= FinalizeDeath;
        }

        void AlertDeath()
        {
            if (!IsServer) return;

            NotifyDeathClientRpc();

            // 게임 규칙 처리 위임
            // BattleManager.Instance.DestroyPlayer(this);
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
            Debug.LogWarning("[PlayerEntity] You Died!");
            // TODO: 사망 UI, 카메라 전환 등
        }
    }
}
