using System.Collections;
using UnityEngine;
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
            notifyMonster.color = Color.red;

            StartCoroutine(VanishEffect());
        }
        
        //TODO : 1~3초동안 보여주고 서서히 사라지는것을 구현
        // Vertex값의 Alpha값을 줄이기?
    }

    private IEnumerator VanishEffect()
    {
        Color color = notifyMonster.color;
        
        while (notifyMonster.color.a > 0)
        {
            color.a -= Time.deltaTime / 3f;
            notifyMonster.color = color;
            
            yield return null;
        }
    }
}
