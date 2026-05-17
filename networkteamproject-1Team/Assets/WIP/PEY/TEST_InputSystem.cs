using UnityEngine;

public class TEST_InputSystem : MonoBehaviour
{
    public BattleInputReader input;
#if UNITY_EDITOR
    private void Reset()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BattleInputReader");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            input = UnityEditor.AssetDatabase.LoadAssetAtPath<BattleInputReader>(path);
        }
        else
        {
            Debug.LogWarning("BattleInputReader SO를 찾을 수 없습니다");
        }
    }
#endif
    private void OnEnable()
    {
        // 김영빈이 Action Name 변경
        // 변경점
        // => onInteract -> onStartInteract
        // => Interact -> onPerformedInteract
        // => offInteract -> onCanceledInteract
        input.Enable();
        input.onStartInteract += HandleInteractStart;
        input.onPerformedInteract += HandleInteractPerformed;
        input.onCanceledInteract += HandleInteractCancel;
        input.onAttack += HandleAttack;
    }
    private void OnDisable()
    {
        // 김영빈이 Action Name 변경
        // 변경점
        // => onInteract -> onStartInteract
        // => Interact -> onPerformedInteract
        // => offInteract -> onCanceledInteract
        input.onStartInteract -= HandleInteractStart;
        input.onPerformedInteract -= HandleInteractPerformed;
        input.onCanceledInteract -= HandleInteractCancel;
        input.onAttack -= HandleAttack;
    }

    void HandleInteractStart()
    {
        Debug.Log("Interact Started!");
    }
    void HandleInteractPerformed()
    {
        Debug.Log("Interact Performed!");
    }
    void HandleInteractCancel()
    {
        Debug.Log("Interact Canceled!");
    }

    void HandleAttack()
    {
        Debug.Log("Attack!");
    }
}
