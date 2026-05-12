using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace Battle
{
    public class PlayerEntity : EntityBase
    {
        PlayerCombat _combat;
        PlayerInputHandler _inputHandler;

        [SerializeField] AudioResource _deathSound;

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
                _inputHandler = GetComponent<PlayerInputHandler>();
                PauseMenu.Instance.Inject(_inputHandler); // 두근두근 의존성 주입
                WinPanel.Instance.Inject(_inputHandler); // 두근두근 의존성 주입

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
            if (VFXManager.Instance != null) // 방어코드중
                VFXManager.Instance.PlayDeathVFX();

            AudioManager.Instance.PlaySfxDry(_deathSound);
        }
    }
}
