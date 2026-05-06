using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Monster
{
    public class MonsterSpawnManager : NetworkBehaviour
    {
        [SerializeField] private GameObject _monsterPrefab;
        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
        
            MonsterSpawn();
        }

        private void Init()
        {
        }

        private void MonsterSpawn()
        {
            if (_monsterPrefab == null || _spawnPoints == null || _spawnPoints.Count == 0) return;
        
            Shuffle(_spawnPoints);
            int rand = UnityEngine.Random.Range(0, _spawnPoints.Count);
        
            GameObject monster = Instantiate(_monsterPrefab, _spawnPoints[rand].position, _spawnPoints[rand].rotation);
            monster.GetComponent<NetworkObject>().Spawn();
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int rand = UnityEngine.Random.Range(0, i);
                (list[i], list[rand]) = (list[rand], list[i]);
            }
        }
    }
}