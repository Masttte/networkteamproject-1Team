using Unity.Netcode;
using UnityEngine;

public class TestPlayer : NetworkBehaviour
{
    private int _hp = 100;

    public override void OnNetworkSpawn()
    {
        
    }
    
    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        
        _hp -= damage;
        if (_hp <= 0)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
