using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterView : MonoBehaviour
{
    // UI 인스펙터
    [SerializeField] private TextMeshProUGUI notifyMonster;
    
    
    // 몬스터 풀려났다는 것을 알림
    public void NotifyMonster(bool isUnlocked)
    {
        if (isUnlocked)
        {
            notifyMonster.text = "괴물이 풀려났다!";
            notifyMonster.color = Color.darkRed;
        }
    }
}
