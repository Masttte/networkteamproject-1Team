using System.Collections;
using UnityEngine;

public class DeathUIPanel : MonoBehaviour
{
    public CanvasGroup deathPanel;
    public float fadeInDuration   = 2.0f;
    public float visibleDuration  = 2.0f;
    public float fadeOutDuration  = 1.0f;

    public void OnPlayerDeath()
    {
        StopAllCoroutines();
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        deathPanel.gameObject.SetActive(true);
        float timer = 0f;
        
        // 페이드 인 (2초동안)
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            deathPanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            yield return null;
        }
        deathPanel.alpha = 1f;
        
        // 2초간 화면 띄우기
        yield return new WaitForSeconds(visibleDuration);
        
        // 페이드 아웃 (1초)
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            deathPanel.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            yield return null;
        }
        deathPanel.alpha = 0f;
        deathPanel.gameObject.SetActive(false);
    }
}
