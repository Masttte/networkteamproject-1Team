using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monster
{
    public class PathSettingManager : MonoBehaviour
    {
        public List<Vector3> pathSettings = new List<Vector3>();

        private void OnDrawGizmos()
        {
            for (int i = 0; i < pathSettings.Count; i++)
            {
                Gizmos.color = new Color(1, 0, 0, 1.0f);
                Vector3 pos = pathSettings[i];
                Gizmos.DrawSphere(pos, 1.0f);
            }
        }
    }
}
