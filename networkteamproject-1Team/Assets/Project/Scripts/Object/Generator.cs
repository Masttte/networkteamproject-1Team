                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    using System;
using Unity.Netcode;
using Battle;
using Cysharp.Threading.Tasks;
using Player;
using UnityEngine.Audio;

public class Generator : NetworkBehaviour, IInteractable
{
    private PressAction _pressAction;

    public MaterialChanger changer;

    private NetworkVariable<bool> _isRepaired = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    public override void OnNetworkSpawn()
    {
        _pressAction = GetComponent<PressAction>();

        _isRepaired.OnValueChanged += OnRepairedStateChange;

        if (IsServer)
        {
            _pressAction.IsPressAction += OnPressCompleted; 
        }
        
        // 게임 중간에 들어온거 방어로직
        if (_isRepaired.Value)
        {
            ApplyCompletedVisual();
        }
    }
    
    public override void OnNetworkDespawn()
    {
        _isRepaired.OnValueChanged -= OnRepairedStateChange;

        if (IsServer && _pressAction != null)
        {
            _pressAction.IsPressAction -= OnPressCompleted;
        }
    }

    // PressAction (발전기 키는 조건) 게이지가 10초 이상이 됐을 때
    private void OnPressCompleted()
    {
        if (_isRepaired.Value) return;

        _isRepaired.Value = true;
    }
    
    // 수정 => IsServer 플래그 변수에 걸리지 않게 변경
    private void OnRepairedStateChange(bool previousValue, bool newValue)
    {
        if (newValue == true)
        {
            ApplyCompletedVisual();
            
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnGeneratorCondition();
            }
        }
    }

    private void ApplyCompletedVisual()
    {
        gameObject.layer = 2; // Ignore Raycast
        
        if (_pressAction != null)
        {
            if (_pressAction.image != null && _pressAction.image.canvas != null)
            {
                _pressAction.image.canvas.gameObject.SetActive(false);
            }

            _pressAction.enabled = false;
        }
        
        if (changer != null)
        {
            changer.ChangeMaterial();
        }
        
        var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        var input = playerObj.GetComponent<PlayerInputHandler>();
        
        input.EnableInput(InputCategory.Movement); // 이동 복구
    }

    public void InteractStart()
    {
        if (_isRepaired.Value) return;
        _pressAction.StartInteraction();
        
        var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        var input = playerObj.GetComponent<PlayerInputHandler>();

        input.DisableInput(InputCategory.Movement); // 발전기 상호작용 중에는 움직이지 못하게
    }

    public void InteractStop()
    {
        if (_isRepaired.Value) return;
        _pressAction.StopInteraction();
        
        var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        var input = playerObj.GetComponent<PlayerInputHandler>();
        
        input.EnableInput(InputCategory.Movement); // 이동 복구
    }
}
