#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DropPhysicsTool
{
    [MenuItem("Tools/물체 떨어트리기 (물리 100프레임)")]
    public static void SimulatePhysicsInEditor()
    {
        Transform[] selectedTransforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
        if (selectedTransforms.Length > 0)
        {
            Undo.RecordObjects(selectedTransforms, "물체 떨어트리기 물리 시뮬레이션");
        }
        else
        {
            Debug.LogWarning("떨어뜨릴 오브젝트를 먼저 선택");
            return;
        }

        // 기존 설정 백업
        SimulationMode originalMode = Physics.simulationMode;
        
        // 연산 중 에러가 터져도 finally 블록은 무조건 실행됨.
        try
        {
            Physics.simulationMode = SimulationMode.Script;
            
            // 물리 시뮬레이션
            for (int i = 0; i < 100; i++)
            {
                Physics.Simulate(Time.fixedDeltaTime);
            }
        }
        finally
        {
            Physics.simulationMode = originalMode;
        }
        
        Debug.Log("에디터 물리 연산 완료");
    }
}
#endif