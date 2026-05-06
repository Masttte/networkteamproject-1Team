using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WIP.KYB.Scripts
{
    public class RandomSpawnObject : NetworkBehaviour
    {
        private static RandomSpawnObject instance;

        public GameObject spawnObject;
        public Transform[] spawnPoints;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public static RandomSpawnObject Instance
        {
            get
            {
                if (instance == null)
                {
                    return null;
                }

                return instance;
            }
        }

        /// <summary>
        /// 오브젝트 랜덤으로 스폰해주는 메서드
        /// </summary>
        /// <param name="spawnCount">몇 개 생성될 것인지?</param>
        public void SpawnObjects(int spawnCount)
        {
            // 서버에서 처리
            if (!IsServer) return;
            
            if (spawnCount <= 0 || spawnCount > spawnPoints.Length)
            {
                Debug.LogError("GameManager에서 설정한 spawnCount의 수가 0보다 작거나 스폰 포인트보다 많습니다.");

                return;
            }
            
            // 원본 배열 복사
            List<Transform> insPoint = new List<Transform>(spawnPoints);

            for (int i = 0; i < spawnCount; i++)
            {
                if (insPoint.Count == 0) return;
                
                int randomIndex = Random.Range(0, insPoint.Count);
                Transform selectedPoint = insPoint[randomIndex];
                
                GameObject spawn = Instantiate(spawnObject, selectedPoint.position, Quaternion.identity);
                spawn.GetComponent<NetworkObject>().Spawn(); // Instantiate로 만든 오브젝트 네트워크 동기화
                
                insPoint.RemoveAt(randomIndex);
            }
        }
    }
}
