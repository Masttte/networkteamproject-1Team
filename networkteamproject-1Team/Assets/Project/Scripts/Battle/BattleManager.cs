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
        public override void OnDestroy()
        {
            base.OnDestroy();
            Instance = null;
        }

        [HideInInspector] public TeamManager tm;
        private void Awake()
        {
            Instance = this;
            tm = GetComponent<TeamManager>();
        }

        [Header("스폰 시스템 연결")]
        [SerializeField] private RandomSpawnObject randomSpawnObject;
        [SerializeField] private int spawnCount; // 스폰할 발전기 개수
        
        [Header("목표 발전기 수")] 
        public int generatorRequiredCount; // 승리 조건에 대한 목표 발전기 개수

        public event Action OnNameSetup;
        public event Action OnGameStart;
        public event Action<TeamType> OnGameEnd;

        [Header("오디오")]
        public AudioResource countSound;

        public NetworkVariable<int> repairedGenerators = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        
        // 재시작 겸용
        public void StartGame() // 로비없이 실행하는 테스트 코드
        {
            if (!IsServer) return;

            // 아직 살아있는 플레이어 제거
            for (int i = tm.activePlayers.Count - 1; i >= 0; i--) tm.activePlayers[i].NetworkObject.Despawn();

            // 재스폰 + 팀 재배정 + StartCountdown
            tm.SpawnAllPlayers();
        }

        // 모든 클라이언트에서 실행
        public async UniTaskVoid StartCountdown(List<TeamBase> players)
        {
            // 인원 수에 따라 목표 발전기 수 설정
            generatorRequiredCount = players.Count <= 3 ? 10 : players.Count <= 4 ? 16 : 19;

            await UniTask.Delay(2300);

            OnNameSetup?.Invoke();
            await UniTask.Delay(noStartDelay ? 0 : 7700);
            AudioManager.Instance.PlaySfxDry(countSound);
            // ----- 발전기 배치 -----
            // 필요한 발전기 개수 초기화
            if (IsServer) repairedGenerators.Value = 0;
            // 발전기 스폰 (서버에서 실행)
            randomSpawnObject.SpawnObjects(spawnCount); 

            await UniTask.Delay(noStartDelay ? 100 : 3000); // 오디오 싱크 시작 딜레이 
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
                //DeclareResultRpc(TeamType.A);
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
            
            repairedGenerators.Value++;
            Debug.Log($"발전기 개수 진행도 업데이트: {repairedGenerators.Value} / {generatorRequiredCount}");
            
            // A팀이 발전기를 모두 돌리면 즉시 게임 승리
            if (repairedGenerators.Value >= generatorRequiredCount)
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
        }

        public void RestartGame()
        {
            //if (!IsServer) return;

            // 씬에 있는 모든 런타임 스폰 네트워크 오브젝트를 디스폰 (플레이어 포함)
            // (씬 고유 오브젝트(In-Scene NetworkObjects)는 디스폰하지 않아야 씬 로드 시 복사되지 않음)
            var networkObjects = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
            foreach (var netObj in networkObjects)
            {
                // In-Scene 오브젝트가 아니고 스폰된 오브젝트라면 디스폰
                if (netObj != null && netObj.IsSpawned && netObj.IsSceneObject == false)
                {
                    netObj.Despawn(true);
                }
            }
            //if (tm.activePlayers != null)
            //    tm.activePlayers.Clear();

            SceneLoader.LoadNetworked(SceneId.Map1);
        }
    }
}

