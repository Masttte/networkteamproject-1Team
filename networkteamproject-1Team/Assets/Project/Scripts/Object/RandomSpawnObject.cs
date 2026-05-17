using System;
using System.Collections.Generic;
using Monster;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WIP.KYB.Scripts
{
    public class RandomSpawnObject : NetworkBehaviour
    {
        [Header("소환할 프리팹")] [SerializeField] private GameObject generatorPrefab;
        [SerializeField] private GameObject prisonPrefab; // 첫번째 몬스터

        [Header("두 번째 프리팹 & 스폰포인트(영안실)")] [SerializeField]
        private GameObject _secondPrisonPrefab; // 두번째 몬스터

        [SerializeField] private Transform _secondMonsterSpawnPoint; // 영안실 고정 스폰 포인트 (강한 몬스터 스폰)

        [Header("세 번째 프리팹")] 
        [SerializeField] private GameObject _thirdPrisonPrefab;

        [Header("스폰 포인트")] public Transform[] spawnPoints; // 나머지 스폰포인트 (랜덤으로 돌릴 포인트)

        private List<NetworkObject> _spawnedObject = new List<NetworkObject>();

        /// <summary>
        /// 오브젝트 랜덤으로 스폰해주는 메서드
        /// </summary>
        /// <param name="spawnCount">몇 개 생성될 것인지?</param>
        public void SpawnObjects(int spawnCount)
        {
            // 서버에서 처리
            if (!IsServer) return;

            // 기존에 스폰되었던 애들 DeSpawn()
            ClearSpawnedObjects();

            if (spawnCount <= 0 || spawnCount > spawnPoints.Length)
            {
                Debug.LogError("spawnCount의 수가 0보다 작거나 스폰 포인트보다 많습니다.");

                return;
            }

            // 원본 배열 복사
            List<Transform> insPoint = new List<Transform>(spawnPoints);

            // 발전기 스폰
            for (int i = 0; i < spawnCount; i++)
            {
                if (insPoint.Count == 0) return;

                int randomIndex = Random.Range(0, insPoint.Count);
                Transform selectedPoint = insPoint[randomIndex];

                SpawnNetworkObject(generatorPrefab, selectedPoint);

                insPoint.RemoveAt(randomIndex);
            }

            // 첫 번째 몬스터 스폰
            if (insPoint.Count > 0)
            {
                int randomIndex = Random.Range(0, insPoint.Count);
                Transform selectedPoint = insPoint[randomIndex];

                SpawnNetworkObject(prisonPrefab, selectedPoint);

                insPoint.RemoveAt(randomIndex);
            }

            // 세 번째 몬스터 스폰
            if (insPoint.Count > 0)
            {
                int randomIndex = Random.Range(0, insPoint.Count);
                Transform selectedPoint = insPoint[randomIndex];

                SpawnNetworkObject(_thirdPrisonPrefab, selectedPoint);
            }

            if (_secondPrisonPrefab == null || _secondMonsterSpawnPoint == null) return;

            SpawnFixedObject(_secondPrisonPrefab, _secondMonsterSpawnPoint);
        }

        private void SpawnNetworkObject(GameObject prefab, Transform spawnTransform)
        {
            if (prefab == null || spawnTransform == null)
            {
                Debug.Log("프리팹이나 스폰 Transform이 Null입니다.");
                return;
            }

            GameObject spawn = Instantiate(prefab, spawnTransform.position, spawnTransform.rotation);
            NetworkObject networkObj = spawn.GetComponent<NetworkObject>();

            networkObj.Spawn();

            _spawnedObject.Add(networkObj);
        }

        private void SpawnFixedObject(GameObject prefab, Transform secondSpawnTransform)
        {
            if (prefab == null || secondSpawnTransform == null) return;

            GameObject spawn = Instantiate(prefab, secondSpawnTransform.position, secondSpawnTransform.rotation);
            NetworkObject networkObj = spawn.GetComponent<NetworkObject>();

            networkObj.Spawn();

            _spawnedObject.Add(networkObj);

            Prison prison = spawn.GetComponent<Prison>();

            if (prison != null)
                prison.IsSecondMonster = true;
        }

        // 스폰된 오브젝트 싹 다 정리
        private void ClearSpawnedObjects()
        {
            if (!IsServer) return;

            foreach (var obj in _spawnedObject)
            {
                if (obj != null && obj.IsSpawned) obj.Despawn();
            }

            _spawnedObject.Clear();
        }
    }
}