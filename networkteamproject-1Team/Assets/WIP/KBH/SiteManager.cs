using UnityEngine;

public class SiteManager : MonoBehaviour
{
    public void OnClick_OpenOurURL()
    {
        Application.OpenURL(" ");       // 깃 사이트
    }
    
    public void OnClick_OpenItchioURL()
    {
        Application.OpenURL(" ");       // itch.io 커뮤니티
    }
    
    public void OnClick_OpenBugReportURL()
    {
        Application.OpenURL(" ");       // 버그 제보 (구글 시트..?)
    }
}
