using UnityEngine;
using TMPro;

public class GeneratorView : MonoBehaviour
{
    // UI 인스펙터
    [SerializeField] private TextMeshProUGUI repairGenerator;
    
    public void NecessaryGenerator(int repairedNumber, int repairedCount)
    {
        repairGenerator.text = $"활성화된 발전기 개수 : {repairedNumber.ToString()} / {repairedCount.ToString()}";
    }
}
