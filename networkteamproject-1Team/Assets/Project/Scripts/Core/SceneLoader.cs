using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 build index 매핑. Build Settings 의 Scenes In Build 순서와 반드시 일치해야 한다.
///   0: TitleScene, 1~: GameScene
/// 새 씬 추가 시 enum 값 + Build Settings 순서를 동시에 맞춘다
/// </summary>
public enum SceneId
{
    Title = 0,
    Map1 = 1
}

/// <summary>
/// 씬 전환 진입점. 모든 씬 전환은 이 클래스를 통해서만 처리한다.
/// </summary>
public static class SceneLoader
{
    // 로컬 씬 로드 (SceneId / int). NGO 미실행 상태에서 사용
    public static void LoadLocal(SceneId id) => LoadLocal((int)id);
    public static void LoadLocal(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
    }

    /// <summary>
    /// NGO 동기화 씬 로드 (SceneId 권장). 호스트에서만 호출하면 모든 멤버에게 자동 전파됨
    /// </summary>
    public static bool LoadNetworked(SceneId id) => LoadNetworked(id.GetName());

    /// <summary>
    /// NGO 동기화 씬 로드 (string). 호스트에서만 호출하면 모든 멤버에게 자동 전파됨
    /// </summary>
    /// <returns>로드 요청에 성공하면 true (호스트가 아니거나 SceneManager 미세팅이면 false)</returns>
    public static bool LoadNetworked(string sceneName)
    {
        if (!NetworkManager.Singleton.IsServer || NetworkManager.Singleton.SceneManager == null)
        {
            Debug.LogError($"SceneLoader: NGO 동기화 로드 실패 - Host가 아니거나 SceneManager 없음 (scene='{sceneName}')");
            return false;
        }
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        return true;
    }
}
