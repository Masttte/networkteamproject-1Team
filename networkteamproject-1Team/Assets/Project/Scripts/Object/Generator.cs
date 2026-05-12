                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    using System;
using Unity.Netcode;
using UnityEngine;
using Battle;
using UnityEditor.Rendering;

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

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnGeneratorCondition();
        }
    }
    
    private void OnRepairedStateChange(bool previousValue, bool newValue)
    {
        if (newValue == true)
        {
            ApplyCompletedVisual();
            
            // TODO: 전구의 머티리얼 변경
        }
    }

    private void ApplyCompletedVisual()
    {
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
    }

    public void InteractStart()
    {
        if (_isRepaired.Value) return;
        _pressAction.StartInteraction();
    }

    public void InteractStop()
    {
        if (_isRepaired.Value) return;
        _pressAction.StopInteraction();
    }
}
