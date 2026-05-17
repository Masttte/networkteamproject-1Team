using UnityEngine;
using UnityEngine.InputSystem;
using static BattleInputAction;
using System;

// 인풋 액션을 받고 이벤트를 실행하는 SO 입니다. 필요한 곳에 SO를 참조하여 사용
public class BattleInputReader : ScriptableObject, IBattleActions
{
    public BattleInputAction inputAction;

    public event Action<Vector2> onMove;
    public event Action<bool> onSprintChanged;
    public bool isSprint { get; private set; }

    public event Action onAttack;
    public event Action onStartInteract; public event Action onPerformedInteract; public event Action onCanceledInteract;
    public event Action onJump;
    public event Action<Vector2> onLook;
    public event Action onNextTarget;

    public event Action on1; public event Action on2; public event Action on3;

    public void Enable()
    {
        if (inputAction == null)
        {
            inputAction = new BattleInputAction();
            inputAction.Battle.SetCallbacks(this);
        }
        inputAction.Enable();
    }
    public void Disable()
    {
        inputAction.Disable();
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        onMove?.Invoke(context.ReadValue<Vector2>());
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        // 이전 호출 시점과 현재 상태를 비교해서 값이 변했으면 isSprint
        bool newSprint = context.ReadValueAsButton();
        if (newSprint != isSprint)
        {
            isSprint = newSprint;
            onSprintChanged?.Invoke(isSprint);
        }
    }

    void IBattleActions.OnAttack(InputAction.CallbackContext context)
    {
        if (context.started) onAttack?.Invoke();
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started) onStartInteract?.Invoke();
        if (context.performed) onPerformedInteract?.Invoke(); 
        if (context.canceled) onCanceledInteract?.Invoke();  
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) onJump?.Invoke();
    }
    
    public void OnLook(InputAction.CallbackContext context)
    {
        onLook?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnNextTarget(InputAction.CallbackContext context)
    {
        if (context.performed) onNextTarget?.Invoke();
    }

    public void On_1(InputAction.CallbackContext context)
    {
        if (context.started) on1?.Invoke();
    }
    public void On_2(InputAction.CallbackContext context)
    {
        if (context.started) on2?.Invoke();
    }
    public void On_3(InputAction.CallbackContext context)
    {
        if (context.started) on3?.Invoke();
    }
}
