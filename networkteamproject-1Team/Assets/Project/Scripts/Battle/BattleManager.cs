using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;
using WIP.KYB.Scripts;

public interface IDamageable
{
    public void TakeDamage(int damage);
}

namespace Battle
{
    [RequireComponent(typeof(TeamManager))]
    public class BattleManager : NetworkBehaviour
    {
        [SerializeField] bool noStartDelay; // 테스트 전용

        public static BattleManager Instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init() => Instance = null;

        [Header("스폰 시스템 연결")]
        [SerializeField] private RandomSpawnObject randomSpawnObject;

        [SerializeField] private int spawnCount = 20; // 스폰할 발전기 개수
        
        [Header("목표 발전기 수")] 
        [SerializeField] private int generatorRequiredCount = 10; // 승리 조건에 대한 목표 발전기 개수
        public override void OnNetworkSpawn()
        {
            Instance = this;

            randomSpawnObject.SpawnObjects(spawnCount);
        }
        

        
        [HideInInspector] public TeamManager tm;
        private void Awake() => tm = GetComponent<TeamManager>();

        public event Action OnGameStart;
        public event Action<TeamType> OnGameEnd;

        [Header("오디오")]
        public AudioResource countSound;

        public NetworkVariable<int> _repairedGenerators = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        
        // 재시작 겸용
        public void StartGame()
        {
            if (!IsServer) return;

            // 필요한 발전기 개수 초기화
            _repairedGenerators.Value = 0;

            // 아직 살아있는 플레이어 제거
            for (int i = tm.activePlayers.Count - 1; i >= 0; i--) tm.activePlayers[i].NetworkObject.Despawn();

            // 재스폰 + 팀 재배정 + StartCountdown
            tm.SpawnAllPlayers();
        }

        // 모든 클라이언트에서 실행
        public async UniTaskVoid StartCountdown(List<TeamBase> players)
        {
            AudioManager.Instance.PlaySfxDry(countSound);
            await UniTask.Delay(noStartDelay ? 0 : 3000); // 시작 딜레이 (임시로 짧게)
            OnGameStart?.Invoke();
            Debug.Log("게임을 시작하지");
        }

        // 사망한 플레이어를 제거하고 승패 판정 실행 (서버만 호출)
        public void DestroyPlayer(EntityBase entity)
        {
            if (entity.TryGetComponent(out TeamBase tb)) tm.activePlayers.Remove(tb);

            entity.NetworkObject.Despawn();

            CheckWinCondition();
        }

        // 각 팀 생존자 수를 확인하여 한 팀이 전멸했을 때 승리팀을 선언
        void CheckWinCondition()
        {
            int aliveA = tm.GetPlayersByTeam(TeamType.A).Count;
            int aliveB = tm.GetPlayersByTeam(TeamType.B).Count;

            Debug.Log($"[BattleManager] 생존: A팀={aliveA}, B팀={aliveB}");

            if (aliveA == 0)
            {
                // A팀 전멸: B팀 승리
                DeclareResultRpc(TeamType.B);
            }
            else if (aliveB == 0)
            {
                // B팀 전멸: A팀 승리 (B 팀이 전멸해도 게임끝나지 않는 게임디자인 고려중)
                DeclareResultRpc(TeamType.A);
            }
            else if (aliveA == 0 && aliveB == 0)
            {
                // 동시 사망: 무승부?!
                DeclareResultRpc(TeamType.None);
            }
        }
        
        public void OnGeneratorCondition()
        {
            if (!IsServer) return;
            
            _repairedGenerators.Value++;
            Debug.Log($"발전기 개수 진행도 업데이트: {_repairedGenerators.Value} / {generatorRequiredCount}");
            
            // A팀이 발전기를 모두 돌리면 즉시 게임 승리
            if (_repairedGenerators.Value >= generatorRequiredCount)
            {
                Debug.Log("모든 발전기 가동 완료");
                DeclareResultRpc(TeamType.A);
            }
        }

        // 승리팀을 모든 클라이언트에 전파
        [Rpc(SendTo.Everyone)]
        void DeclareResultRpc(TeamType winner)
        {
            string msg = winner == TeamType.None ? "무승부?!" : $"{winner}팀 승리!";
            Debug.Log($"<color=green>[BattleManager] 게임 종료: {msg}</color>");
            OnGameEnd?.Invoke(winner);
            // TODO: [BattleManager] 게임 종료 UI 표시등 추가 작업
        }

    }
}

