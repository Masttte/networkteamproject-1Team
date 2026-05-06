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

        
    }
}
