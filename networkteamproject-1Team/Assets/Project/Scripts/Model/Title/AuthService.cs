using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;


public static class AuthService
{
    //[RuntimeInitializeOnLoadMethod]
    public static async UniTask InitializeAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"[Auth] 로그인 완료: {AuthenticationService.Instance.PlayerId}");
        }
    }
}
