using System.IO;
using UnityEngine.SceneManagement;

public static class SceneIdExtensions
{
    /// <summary>
    /// Build Settings 등록된 씬의 이름 (확장자 없음).
    /// Unity SceneManager / NGO NetworkSceneManager 가 string 만 받으므로 변환 유틸로 사용
    /// </summary>
    public static string GetName(this SceneId id)
    {
        string path = SceneUtility.GetScenePathByBuildIndex((int)id);
        return Path.GetFileNameWithoutExtension(path);
    }
}
