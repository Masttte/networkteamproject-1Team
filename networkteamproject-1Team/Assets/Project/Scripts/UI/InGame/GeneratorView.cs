using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GeneratorView : MonoBehaviour
{
    // UI 인스펙터
    [SerializeField] private TextMeshProUGUI repairGenerator;
    
    public void NecessaryGenerator(int repairedNumber, int repairedCount)
    {
        repairGenerator.text = $"{repairedNumber.ToString()} / {repairedCount.ToString()}";
    }
}
