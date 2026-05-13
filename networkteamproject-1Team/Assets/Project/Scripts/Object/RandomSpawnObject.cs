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
        [Header("소환할 프리팹")]
        public GameObject generatorPrefab;
        public GameObject prisonPrefab;

        [SerializeField] private GameObject _secondPrisonPrefab;
        [SerializeField] private Transform _secondMonsterSpawnPoint;

        // public Transform basementFixedPoint; // 영안실 고정 스폰 포인트 (강한 몬스터 스폰) 
        public Transform[] spawnPoints; // 나머지 스폰포인트 (랜덤으로 돌릴 포인트)

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

            for (int i = 0; i < spawnCount; i++)
            {
                if (insPoint.Count == 0) return;
                
                int randomIndex = Random.Range(0, insPoint.Count);
                Transform selectedPoint = insPoint[randomIndex];
                
                SpawnNetworkObject(generatorPrefab, selectedPoint);
                
                insPoint.RemoveAt(randomIndex);
            }

            if (insPoint.Count > 0)
            {
                int randomIndex = Random.Range(0, insPoint.Count);
                Transform selectedPoint = insPoint[randomIndex];
                
                SpawnNetworkObject(prisonPrefab, selectedPoint);
                
                insPoint.RemoveAt(randomIndex);
            }

            if (_secondPrisonPrefab == null || _secondMonsterSpawnPoint == null) return;
            
            GameObject monster = Instantiate(_secondPrisonPrefab, _secondMonsterSpawnPoint.position, Quaternion.identity);
            monster.gameObject.GetComponent<NetworkObject>().Spawn();
            Prison prison = monster.GetComponent<Prison>();
            prison.IsSecondMonster = true;
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
