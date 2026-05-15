using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Michsky.UI.Dark;

/// <summary>
/// QualityManager에 누락된 해상도(주사율 포함) PlayerPrefs 영속화 보완 컴포넌트.
/// QualityManager가 붙은 GameObject에 함께 부착한다.
/// </summary>
public class ResolutionPersistence : MonoBehaviour
{
    // RefreshRate는 numerator/denominator 분수형이라 두 키로 분할 저장
    const string PREF_W      = "ResolutionWidth";
    const string PREF_H      = "ResolutionHeight";
    const string PREF_RR_NUM = "ResolutionRefreshNumerator";
    const string PREF_RR_DEN = "ResolutionRefreshDenominator";

    [SerializeField] QualityManager qualityManager;
    
    private FullScreenMode _originalMode;

    private void Awake()
    {
        _originalMode = Screen.fullScreenMode;
        if (qualityManager == null) qualityManager = GetComponent<QualityManager>();
    }

    void Start()
    {
        InitAsync(destroyCancellationToken).Forget();
    }

    async UniTaskVoid InitAsync(System.Threading.CancellationToken ct)
    {
        // HorizontalSelector(WindowMode)의 invokeAtStart가 fullScreenMode를 복원할 시간 확보
        await UniTask.NextFrame(ct);
        
        // QualityManager.Start가 망친 fullScreenMode 복원
        if (Screen.fullScreenMode != _originalMode)
        {
            Debug.Log("Restore Screen.fullScreenMode");
            Screen.fullScreenMode = _originalMode;
        }

        if (PlayerPrefs.HasKey(PREF_W) && PlayerPrefs.HasKey(PREF_H))
        {
            int w = PlayerPrefs.GetInt(PREF_W);
            int h = PlayerPrefs.GetInt(PREF_H);
            uint rrNum = (uint)PlayerPrefs.GetInt(PREF_RR_NUM, 0);
            uint rrDen = (uint)PlayerPrefs.GetInt(PREF_RR_DEN, 0);

            if (rrNum > 0 && rrDen > 0)
            {
                var ratio = new RefreshRate { numerator = rrNum, denominator = rrDen };
                Screen.SetResolution(w, h, Screen.fullScreenMode, ratio);
            }
            else
            {
                // 주사율 키가 없는 기존 저장값 마이그레이션 경로
                Screen.SetResolution(w, h, Screen.fullScreenMode);
            }

            SyncDropdownIndex(w, h, rrNum, rrDen);
        }

        if (qualityManager.defaultDropdown != null)
            qualityManager.defaultDropdown.onValueChanged.AddListener(SaveResolution);
    }

    void SyncDropdownIndex(int w, int h, uint rrNum, uint rrDen)
    {
        if (qualityManager.defaultDropdown == null) return;

        var resolutions = Screen.resolutions;
        int matched = -1;

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width != w || resolutions[i].height != h) continue;

            if (rrNum > 0 && rrDen > 0)
            {
                var r = resolutions[i].refreshRateRatio;
                if (r.numerator == rrNum && r.denominator == rrDen)
                {
                    matched = i;
                    break;
                }
            }
            else if (matched < 0)
            {
                matched = i; // 주사율 정보 없을 때 폴백
            }
        }

        if (matched >= 0)
        {
            // 이벤트 재발동 방지를 위해 WithoutNotify 사용
            qualityManager.defaultDropdown.SetValueWithoutNotify(matched);
            qualityManager.defaultDropdown.RefreshShownValue();
        }
    }

    void SaveResolution(int index)
    {
        Debug.Log($"[ResolutionPersistence] SaveResolution(index={index}) 호출");
        var resolutions = Screen.resolutions;
        if (index < 0 || index >= resolutions.Length) return;

        var r = resolutions[index];
        PlayerPrefs.SetInt(PREF_W, r.width);
        PlayerPrefs.SetInt(PREF_H, r.height);
        PlayerPrefs.SetInt(PREF_RR_NUM, (int)r.refreshRateRatio.numerator);
        PlayerPrefs.SetInt(PREF_RR_DEN, (int)r.refreshRateRatio.denominator);
        PlayerPrefs.Save();
    }
}