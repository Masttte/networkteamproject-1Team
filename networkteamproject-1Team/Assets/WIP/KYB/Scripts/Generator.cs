using System;
using Unity.Netcode;
using UnityEngine;

public class Generator : NetworkBehaviour, IInteractable
{
    private PressAction _pressAction;
    private MeshRenderer _renderer;
    public Material completedMaterials;
    
    public override void OnNetworkSpawn()
    {
        _renderer = GetComponent<MeshRenderer>();
        _pressAction = GetComponent<PressAction>();

        if (IsServer)
        {
            _pressAction.IsPressAction += ChangeToCompletedMaterialClientRpc; 
        }
    }
    
    /// <summary>
    /// 머터리얼 변경 Rpc 메서드
    /// </summary>
    [ClientRpc]
    private void ChangeToCompletedMaterialClientRpc()
    {
        if (_renderer.material == completedMaterials)
        {
            return;
        }
        
        _renderer.material = completedMaterials;
        
        _pressAction.image.canvas.gameObject.SetActive(false);
    }

    public void InteractStart()
    {
        _pressAction.StartInteraction();
    }

    public void InteractStop()
    {
        _pressAction.StopInteraction();
    }
    
    // public void StartInteractServerRpc()
    // public void StopInteractServerRpc()
}
